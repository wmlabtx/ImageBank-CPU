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
                        img1.BestDistance = 256;
                    }
                }
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
                var bestdistance = 256;
                for (var i = 0; i < candidates.Length; i++) {
                    var img2 = candidates[i];
                    var distance = img1.PHashEx.HammingDistance(img2.PHashEx);
                    if (distance < bestdistance) {
                        bestid = img2.Id;
                        bestdistance = distance;
                    }
                }

                if (bestid != img1.BestId) {
                    _sb.Clear();
                    _sb.Append($"a:{_added}/f:{_found}/b:{_bad} [{img1.Id}-{bestid}] {img1.BestDistance} -> {bestdistance}");
                    backgroundworker.ReportProgress(0, _sb.ToString());
                    img1.BestId = bestid;
                }

                if (img1.BestDistance != bestdistance) {
                    img1.BestDistance = bestdistance;
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
                year: year,
                history: emptyhistory,
                bestid: id,
                bestdistance: 256,
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