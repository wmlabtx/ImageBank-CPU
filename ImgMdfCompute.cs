using OpenCvSharp;
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

        private static void ComputeInternal(BackgroundWorker backgroundworker)
        {
            Img img1 = null;
            var candidates = new SortedDictionary<string, float>();
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                img1 = _imgList
                    .OrderBy(e => e.Value.LastCheck)
                    .FirstOrDefault()
                    .Value;
            }

            var filename = Helper.GetFileName(img1.Name);
            var imagedata = Helper.ReadData(filename);
            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                Delete(img1.Name);
                return;
            }

            Mat[] descriptors;
            try {
                descriptors = ImageHelper.GetDescriptors(bitmap);
                if (descriptors == null) {
                    Delete(img1.Name);
                    return;
                }
            }
            finally {
                if (bitmap != null) {
                    bitmap.Dispose();
                }
            }

            AddDescriptors(img1.Name, descriptors, backgroundworker);
            descriptors[0].Dispose();
            descriptors[1].Dispose();

            lock (_imglock) {
                var imagescount = _imgList.Count - 1;
                foreach (var enode in _nodeList.Values) {
                    if (!enode.Members.ContainsKey(img1.Name)) {
                        continue;
                    }

                    if (enode.Members.Count == 1) {
                        continue;
                    }

                    var k = (float)Math.Log10((double)imagescount / (enode.Members.Count - 1));
                    foreach (var ename in enode.Members) {
                        if (ename.Key.Equals(img1.Name)) {
                            continue;
                        }

                        if (candidates.ContainsKey(ename.Key)) {
                            candidates[ename.Key] += k;
                        }
                        else {
                            candidates.Add(ename.Key, k);
                        }
                    }
                }
            }

            if (candidates.Count == 0) {
                if (!string.IsNullOrEmpty(img1.BestNames)) {
                    img1.BestNames = string.Empty;
                    img1.LastChanged = DateTime.Now;
                }

                img1.LastCheck = DateTime.Now;
                return;
            }

            if (img1.Family > 0) {
                var maxk = candidates.Max(e => e.Value);
                lock (_imglock) {
                    var enames = candidates.Keys.ToArray();
                    foreach (var ename in enames) {
                        if (_imgList.TryGetValue(ename, out var cimg)) {
                            if (cimg.Family == img1.Family) {
                                candidates[ename] += maxk + 1f;
                            }
                        }
                    }
                }
            }

            var bestnames = string.Concat(candidates.OrderByDescending(e => e.Value).Take(100).Select(e => e.Key).ToArray());
            if (string.IsNullOrEmpty(img1.BestNames) || !bestnames.Equals(img1.BestNames)) {
                var sb = new StringBuilder();
                var rc = _rwList.Count;
                var nc = GetLiveNodesCount();
                sb.Append($"a:{_added}/f:{_found}/b:{_bad}/i:{rc:n0}/n:{nc:n0} ");
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img1.Name}[{img1.Generation}]");
                backgroundworker.ReportProgress(0, sb.ToString());
                lock (_imglock) {
                    img1.BestNames = bestnames;
                    img1.LastChanged = DateTime.Now;
                }
            }

            img1.LastCheck = DateTime.Now;
        }

        private static void ImportInternal(BackgroundWorker backgroundworker)
        {
            lock (_imglock) {
                if (_imgList.Count >= AppConsts.MaxImages) {
                    return;
                }
            }

            FileInfo fileinfo;
            lock (_rwlock) {
                if (_rwList.Count == 0) {
                    return;
                }

                fileinfo = _rwList.ElementAt(0);
                _rwList.RemoveAt(0);
            }

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
                    Helper.DecryptDat(imagedata, password) :
                    Helper.Decrypt(imagedata, password);

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
                        Helper.DeleteToRecycleBin(badfilename);
                        File.Move(orgfilename, badfilename);
                        _bad++;
                        return;
                    }
                    else {
                        if (!ImageHelper.GetImageDataFromBitmap(corruptedbitmap, out var fixedimagedata)) {
                            var badname = Path.GetFileName(orgfilename);
                            var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                            Helper.DeleteToRecycleBin(badfilename);
                            File.Move(orgfilename, badfilename);
                            _bad++;
                            return;
                        }
                        else {
                            var badname = Path.GetFileNameWithoutExtension(orgfilename);
                            var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}{AppConsts.JpgExtension}";
                            Helper.DeleteToRecycleBin(badfilename);
                            File.WriteAllBytes(badfilename, fixedimagedata);
                            Helper.DeleteToRecycleBin(orgfilename);
                            _bad++;
                            return;
                        }
                    }
                }
            }

            var hash = Helper.ComputeHash(imagedata);
            bool found;
            Img imgfound;
            lock (_imglock) {
                found = _hashList.TryGetValue(hash, out imgfound);
            }

            if (found) {
                // we found the same image in a database
                var filenamefound = Helper.GetFileName(imgfound.Name);
                if (File.Exists(filenamefound)) {
                    // no reason to add the same image from a heap; we have one
                    Helper.DeleteToRecycleBin(orgfilename);
                    _found++;
                    return;
                }
                else {
                    // found image is gone; delete it
                    Delete(imgfound.Name);
                }
            }

            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                var badname = Path.GetFileName(orgfilename);
                var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                Helper.DeleteToRecycleBin(badfilename);
                File.Move(orgfilename, badfilename);
                return;
            }

            Mat[] descriptors;
            try {
                descriptors = ImageHelper.GetDescriptors(bitmap);
                if (descriptors == null) {
                    var badname = Path.GetFileName(orgfilename);
                    var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                    Helper.DeleteToRecycleBin(badfilename);
                    File.Move(orgfilename, badfilename);
                    return;
                }
            }
            finally {
                if (bitmap != null) {
                    bitmap.Dispose();
                }
            }

            MetadataHelper.GetMetadata(imagedata, out var datetaken);

            var lc = GetMinLastCheck();
            var lv = new DateTime(2021, 1, 1);

            // we have to create unique name and a location in Hp folder
            string newname;
            string newfilename;
            var iteration = -1;
            do {
                iteration++;
                newname = Helper.GetName(hash, iteration);
                newfilename = Helper.GetFileName(newname);
            } while (File.Exists(newfilename));

            var nimg = new Img(
                name: newname,
                hash: hash,
                datetaken: datetaken,
                family: 0,
                bestnames: string.Empty,
                lastchanged: lc,
                lastview: lv,
                lastcheck: lc,
                generation: 0);

            Add(nimg);

            var lastmodified = File.GetLastWriteTime(orgfilename);
            if (lastmodified > DateTime.Now) {
                lastmodified = DateTime.Now;
            }

            if (!orgfilename.Equals(newfilename, StringComparison.OrdinalIgnoreCase)) {
                Helper.WriteData(newfilename, imagedata);
                File.SetLastWriteTime(newfilename, lastmodified);
                Helper.DeleteToRecycleBin(orgfilename);
            }

            AddDescriptors(newname, descriptors, backgroundworker);
            descriptors[0].Dispose();
            descriptors[1].Dispose();
            
            _added++;
        }

        public static void Compute(BackgroundWorker backgroundworker)
        {
            ImportInternal(backgroundworker);
            for (var i = 0; i < 3; i++) {
                ComputeInternal(backgroundworker);
            }
        }
    }
}