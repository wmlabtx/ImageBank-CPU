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
                        img1.BestDistance = 1000f;
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

            var candidates = img1.Family == 0 ?
                shadowcopy.Where(e => e.Id != img1.Id && e.Family > 0 && !img1.History.ContainsKey(e.Id)).ToArray() :
                shadowcopy.Where(e => e.Id != img1.Id && e.Family > 0 && !img1.History.ContainsKey(e.Id) && img1.Family == e.Family).ToArray();

            if (candidates.Length > 0) {
                var bestid = img1.BestId;
                var bestdistance = 1000f;
                for (var i = 0; i < candidates.Length; i++) {
                    var img2 = candidates[i];
                    var distance = ImageHelper.GetDistance(img1.Descriptors[0], img2.Descriptors);
                    if (distance < bestdistance) {
                        bestid = img2.Id;
                        bestdistance = distance;
                    }
                }

                if (bestid != img1.BestId) {
                    _sb.Clear();
                    _sb.Append($"a:{_added}/f:{_found}/b:{_bad} [{img1.Id}-{bestid}] {img1.BestDistance:F1} -> {bestdistance:F1}");
                    backgroundworker.ReportProgress(0, _sb.ToString());
                    img1.BestId = bestid;
                }

                if (img1.BestDistance != bestdistance) {
                    img1.BestDistance = bestdistance;
                }
            }
            else {
                CreateFamily(img1);
            }

            img1.LastCheck = DateTime.Now;
        }

        private static void ImportInternal(BackgroundWorker backgroundworker)
        {
            FileInfo fileinfo;
            lock (_rwlock) {
                if (_rwList.Count == 0) {
                    ComputeInternal(backgroundworker);
                    return;
                }

                fileinfo = _rwList.ElementAt(0);
                _rwList.RemoveAt(0);
            }

            _sb.Clear();
            _sb.Append($"a:{_added}/f:{_found}/b:{_bad}");
            backgroundworker.ReportProgress(0, _sb.ToString());

            var orgfilename = fileinfo.FullName;
            if (!File.Exists(orgfilename)) {
                return;
            }

            var orgextension = Path.GetExtension(orgfilename);
            if (
                !orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase) &&
                !orgextension.Equals(AppConsts.DbxExtension, StringComparison.OrdinalIgnoreCase) &&
                !orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) &&
                !orgextension.Equals(AppConsts.PngExtension, StringComparison.OrdinalIgnoreCase) &&
                !orgextension.Equals(AppConsts.BmpExtension, StringComparison.OrdinalIgnoreCase) &&
                !orgextension.Equals(AppConsts.WebpExtension, StringComparison.OrdinalIgnoreCase) &&
                !orgextension.Equals(AppConsts.JpgExtension, StringComparison.OrdinalIgnoreCase) &&
                !orgextension.Equals(AppConsts.JpegExtension, StringComparison.OrdinalIgnoreCase)
                ) {
                return;
            }

            var imagedata = File.ReadAllBytes(orgfilename);
            if (imagedata == null || imagedata.Length < 16) {
                File.Move(orgfilename, $"{orgfilename}{AppConsts.CorruptedExtension}");
                _bad++;
                return;
            }

            if (orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ||
                orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                var password = Path.GetFileNameWithoutExtension(orgfilename);
                var decrypteddata = orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ?
                    EncryptionHelper.DecryptDat(imagedata, password) :
                    EncryptionHelper.Decrypt(imagedata, password);

                if (decrypteddata != null) {
                    imagedata = decrypteddata;
                }
            }

            var magicformat = ImageHelper.GetMagicFormat(imagedata);
            if (magicformat == MagicFormat.Jpeg) {
                if (imagedata[0] != 0xFF || imagedata[1] != 0xD8 || imagedata[imagedata.Length - 2] != 0xFF || imagedata[imagedata.Length - 1] != 0xD9) {
                    if (!ImageHelper.GetBitmapFromImageData(imagedata, out var corruptedbitmap)) {
                        var badname = Path.GetFileName(orgfilename);
                        var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                        FileHelper.DeleteToRecycleBin(badfilename);
                        File.Move(orgfilename, badfilename);
                        _bad++;
                        return;
                    }
                    else {
                        if (!ImageHelper.GetImageDataFromBitmap(corruptedbitmap, out var fixedimagedata)) {
                            var badname = Path.GetFileName(orgfilename);
                            var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                            FileHelper.DeleteToRecycleBin(badfilename);
                            File.Move(orgfilename, badfilename);
                            _bad++;
                            return;
                        }
                        else {
                            var badname = Path.GetFileNameWithoutExtension(orgfilename);
                            var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}{AppConsts.JpgExtension}";
                            FileHelper.DeleteToRecycleBin(badfilename);
                            File.WriteAllBytes(badfilename, fixedimagedata);
                            FileHelper.DeleteToRecycleBin(orgfilename);
                            _bad++;
                            return;
                        }
                    }
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
                    _found++;
                    return;
                }
                else {
                    // found image is gone; delete it
                    Delete(imgfound.Id);
                }
            }

            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                var badname = Path.GetFileName(orgfilename);
                var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                FileHelper.DeleteToRecycleBin(badfilename);
                File.Move(orgfilename, badfilename);
                _bad++;
                return;
            }

            var descriptors = ImageHelper.GetAkaze2Descriptors(bitmap);

            if (bitmap != null) {
                bitmap.Dispose();
            }

            MetadataHelper.GetMetadata(imagedata, out var datetaken);
           
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
            int family;
            lock (_imglock) {
                family = _imgList.Count == 0 ? 1 : 0;
            }

            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                datetaken: datetaken,
                descriptors: descriptors,
                family: family,
                history: emptyhistory,
                bestid: id,
                bestdistance: 1000f,
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
        }
    }
}