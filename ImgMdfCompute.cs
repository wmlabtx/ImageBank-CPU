using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static int _added;
        private static int _found;
        private static int _bad;
        private static readonly StringBuilder _sb = new StringBuilder();

        private static void ComputeInternal(BackgroundWorker backgroundworker)
        {
            Img[] shadowcopy = null;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                shadowcopy = _imgList.OrderBy(e => e.Key).Select(e => e.Value).ToArray();
            }

            var lc = shadowcopy.Min(e => e.LastCheck);
            var img1 = shadowcopy.First(e => e.LastCheck == lc);
            if (img1.BestId != 0) {
                lock (_imglock) {
                    if (!_imgList.ContainsKey(img1.BestId)) {
                        img1.BestId = 0;
                        img1.BestPDistance = 256;
                        img1.BestVDistance = 100f;
                    }
                }
            }

            var filename = FileHelper.NameToFileName(img1.Name);
            var imagedata = FileHelper.ReadData(filename);
            if (imagedata == null) {
                Delete(img1.Id);
                return;
            }

            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
                    Delete(img1.Id);
                    return;
                }

                var matrix = BitmapHelper.GetMatrix(imagedata);
                var phashex = new PHashEx(matrix);
                var descriptors = SiftHelper.GetDescriptors(matrix);
                if (img1.Vector == null || img1.Vector.Length == 0) {
                    SiftHelper.Populate(descriptors);
                }

                var vector = SiftHelper.ComputeVector(descriptors);
                img1.Vector = vector;
            }

            if (img1.History.Count > 0) {
                var history = img1.History.Select(e => e.Key).ToArray();
                lock (_imglock) {
                    foreach (var e in history) {
                        if (!_imgList.ContainsKey(img1.BestId)) {
                            img1.History.Remove(e);
                        }
                    }

                    if (img1.History.Count < history.Length) {
                        img1.SaveHistory();
                    }
                }
            }

            var candidates = shadowcopy.Where(e => e.Id != img1.Id && !img1.History.ContainsKey(e.Id)).ToArray();
            if (candidates.Length > 0) {
                var bestid = img1.BestId;
                var bestpdistance = 256;
                var bestvdistance = 100f;
                for (var i = 0; i < candidates.Length; i++) {
                    var img2 = candidates[i];
                    var pdistance = img1.PHashEx.HammingDistance(img2.PHashEx);
                    var vdistance = SiftHelper.GetDistance(img1.Vector, img2.Vector);
                    if (pdistance < 80) {
                        if (pdistance < bestpdistance) {
                            bestid = img2.Id;
                            bestpdistance = pdistance;
                            bestvdistance = vdistance;

                        }
                    }
                    else {
                        if (bestpdistance >= 80) {
                            if (vdistance < bestvdistance) {
                                bestid = img2.Id;
                                bestpdistance = pdistance;
                                bestvdistance = vdistance;
                            }
                        }
                    }
                }

                if (bestid != img1.BestId) {
                    var nodecount = SiftHelper.Count;

                    _sb.Clear();
                    _sb.Append($"n:{nodecount} a:{_added}/f:{_found}/b:{_bad} [{img1.Id}-{bestid}] {img1.BestPDistance} ({img1.BestVDistance:F2}) -> {bestpdistance} ({bestvdistance:F2})");
                    backgroundworker.ReportProgress(0, _sb.ToString());
                    img1.BestId = bestid;
                }

                if (img1.BestPDistance != bestpdistance) {
                    img1.BestPDistance = bestpdistance;
                }

                if (img1.BestVDistance != bestvdistance) {
                    img1.BestVDistance = bestvdistance;
                }

            }
            else {
                if (img1.History.Count > 0) {
                    img1.History.Clear();
                    img1.SaveHistory();
                }
            }

            img1.LastCheck = DateTime.Now;
        }

        private static void ImportInternal(BackgroundWorker backgroundworker)
        {
            FileInfo fileinfo;
            lock (_rwlock) {
                if (_rwList.Count == 0) {
                    //ComputeInternal(backgroundworker);
                    return;
                }

                fileinfo = _rwList.ElementAt(0);
                _rwList.RemoveAt(0);
            }

            /*
            _sb.Clear();
            _sb.Append($"a:{_added}/f:{_found}/b:{_bad}");
            backgroundworker.ReportProgress(0, _sb.ToString());
            */

            var orgfilename = fileinfo.FullName;
            if (!File.Exists(orgfilename)) {
                return;
            }

            var imagedata = File.ReadAllBytes(orgfilename);
            if (imagedata == null || imagedata.Length < 256) {
                FileHelper.MoveCorruptedFile(orgfilename);
                _bad++;
                return;
            }

            int year = DateTime.Now.Year;
            var orgextension = Path.GetExtension(orgfilename);
            if (orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ||
                orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                year = 0;
                var password = Path.GetFileNameWithoutExtension(orgfilename);
                var decrypteddata = orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ?
                    EncryptionHelper.DecryptDat(imagedata, password) :
                    EncryptionHelper.Decrypt(imagedata, password);

                if (decrypteddata != null) {
                    imagedata = decrypteddata;
                }
            }

            var hash = MD5HashHelper.Compute(imagedata);
            bool found;
            Img imgfound;
            lock (_imglock) {
                found = _hashList.TryGetValue(hash, out imgfound);
            }

            if (found) {
                // we found the same image in a database
                var filenamefound = FileHelper.NameToFileName(imgfound.Name);
                if (File.Exists(filenamefound)) {
                    // no reason to add the same image from a heap; we have one
                    FileHelper.DeleteToRecycleBin(orgfilename);
                    if (imgfound.Year == 0 && year != 0) {
                        imgfound.Year = year;
                    }

                    _found++;
                    return;
                }
                
                // found image is gone; delete it
                Delete(imgfound.Id);
            }

            var bitmap = BitmapHelper.ImageDataToBitmap(imagedata);
            if (bitmap == null) {
                var badname = Path.GetFileName(orgfilename);
                var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                if (File.Exists(badfilename)) {
                    FileHelper.DeleteToRecycleBin(badfilename);
                }

                File.WriteAllBytes(badfilename, imagedata);
                FileHelper.DeleteToRecycleBin(orgfilename);
                _bad++;
                return;
            }

            var matrix = BitmapHelper.GetMatrix(imagedata);
            var phashex = new PHashEx(matrix);
            bitmap.Dispose();

            var descriptors = SiftHelper.GetDescriptors(matrix);
            SiftHelper.Populate(descriptors);
            SiftHelper.SaveNodes();
            var vector = SiftHelper.ComputeVector(descriptors);
            
            // we have to create unique name and a location in Hp folder
            string newname;
            string newfilename;
            var iteration = -1;
            do {
                iteration++;
                newname = FileHelper.HashToName(hash, iteration);
                newfilename = FileHelper.NameToFileName(newname);
            } while (File.Exists(newfilename));

            var lv = GetMinLastView();
            var lc = GetMinLastCheck();
            var emptyhistory = new SortedList<int, int>();
            var id = AllocateId();

            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                phashex: phashex,
                vector: vector,
                year: year,
                history: emptyhistory,
                bestid: id,
                bestpdistance: 256,
                bestvdistance: 100f,
                lastview: lv,
                lastcheck: lc);

            Add(nimg);

            var lastmodified = File.GetLastWriteTime(orgfilename);
            if (lastmodified > DateTime.Now) {
                lastmodified = DateTime.Now;
            }

            if (!orgfilename.Equals(newfilename, StringComparison.OrdinalIgnoreCase)) {
                FileHelper.WriteData(newfilename, imagedata);
                File.SetLastWriteTime(newfilename, lastmodified);
                FileHelper.DeleteToRecycleBin(orgfilename);
            }

            _added++;
        }

        public static void Compute(BackgroundWorker backgroundworker)
        {
            ImportInternal(backgroundworker);
            ComputeInternal(backgroundworker);
            ComputeInternal(backgroundworker);
        }   
    }
}