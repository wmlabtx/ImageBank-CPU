using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static int _added = 0;
        private static int _found = 0;
        private static int _sim = 0;
        private static int _bad = 0;

        private static void ComputeInternal(BackgroundWorker backgroundworker)
        {
            Img img1 = null;
            Img[] candidates;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                foreach (var e in _imgList) {
                    var eX = e.Value;

                    if (eX.Hash.Equals(eX.NextHash)) {
                        img1 = eX;
                        break;
                    }

                    if (!_hashList.TryGetValue(eX.NextHash, out var eY)) {
                        img1 = eX;
                        break;
                    }

                    if (img1 == null || eX.LastCheck < img1.LastCheck) {
                        img1 = eX;
                    }
                }

                if (!_hashList.ContainsKey(img1.NextHash)) {
                    img1.NextHash = img1.Hash;
                    if (img1.KazeMatch > 0) {
                        img1.KazeMatch = 0;
                    }
                }

                candidates = _imgList
                    .Where(e => !e.Value.Name.Equals(img1.Name, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (candidates.Length == 0) {
                if (!img1.NextHash.Equals(img1.Hash, StringComparison.OrdinalIgnoreCase)) {
                    img1.NextHash = img1.Hash;
                    img1.KazeMatch = 0;
                    img1.LastChanged = DateTime.Now;
                }

                img1.LastCheck = DateTime.Now;
                return;
            }

            var nexthash = img1.NextHash;
            var kazematch = img1.KazeMatch;
            var lastchanged = img1.LastChanged;
            var dmax = img1.KazeOne.Length;
            var img2 = img1;

            for (var i = 0; i < candidates.Length; i++) {
                var m = ImageHelper.ComputeKazeMatch(img1.KazeOne, candidates[i].KazeOne, candidates[i].KazeTwo);
                var d = Math.Abs(img1.KazeOne.Length - candidates[i].KazeOne.Length);
                if (m > kazematch || (m == kazematch && d < dmax)) {
                    lock (_imglock) {
                        if (_imgList.ContainsKey(img1.Name) && _imgList.ContainsKey(candidates[i].Name)) {
                            nexthash = candidates[i].Hash;
                            kazematch = m;
                            lastchanged = DateTime.Now;
                            dmax = d;
                            img2 = candidates[i];
                        }
                    }
                }
            }

            if (!nexthash.Equals(img1.NextHash)) {
                var sb = new StringBuilder();
                sb.Append($"a{_added}/f{_found}/s{_sim}/b{_bad}/{_rwList.Count / 1024}K ");
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img1.Name}[{img1.Generation}]: ");
                sb.Append($"{img1.KazeMatch} ");
                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                sb.Append($"{kazematch} ");
                backgroundworker.ReportProgress(0, sb.ToString());
            }

            if (!nexthash.Equals(img1.NextHash, StringComparison.OrdinalIgnoreCase)) {
                img1.NextHash = nexthash;
            }

            if (img1.KazeMatch != kazematch) {
                img1.KazeMatch = kazematch;
            }

            if (img1.LastChanged != lastchanged) {
                img1.LastChanged = lastchanged;
            }

            img1.LastCheck = DateTime.Now;
        }

        private static void ImportInternal(BackgroundWorker backgroundworker)
        {
            lock (_imglock) {
                if (_imgList.Count >= AppConsts.MaxImages) {
                    return;
                }

                /*
                var g0 = GetGenerationSize(0);
                if (g0 >= AppConsts.MaxImagesInGeneration) {
                    return;
                }
                */
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

            // it is a new image; we don't have one 

            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                var badname = Path.GetFileName(orgfilename);
                var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                Helper.DeleteToRecycleBin(badfilename);
                File.Move(orgfilename, badfilename);
                return;
            }

            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                bitmap = ImageHelper.RepixelBitmap(bitmap);
            }

            ImageHelper.ComputeKazeDescriptors(bitmap, out var kazeone, out var kazetwo);
            if (kazeone == null || kazeone.Length == 0) {
                var badname = Path.GetFileNameWithoutExtension(orgfilename);
                var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}{AppConsts.JpgExtension}";
                Helper.DeleteToRecycleBin(badfilename);
                File.WriteAllBytes(badfilename, imagedata);
                Helper.DeleteToRecycleBin(orgfilename);
                _bad++;
                return;
            }

            MetadataHelper.GetMetadata(imagedata, out var datetaken, out var metadata);

            var lc = GetMinLastCheck();
            var lv = DateTime.Now/*GetMinLastView()*/;

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
                width: bitmap.Width,
                height: bitmap.Height,
                size: imagedata.Length,
                datetaken: datetaken,
                metadata: metadata,
                kazeone: kazeone,
                kazetwo: kazetwo,
                nexthash: hash,
                kazematch: 0,
                lastchanged: lc, 
                lastview: lv,
                lastcheck: lc,
                generation: 0);

            Add(nimg);

            var lastmodified = File.GetLastWriteTime(orgfilename);
            if (lastmodified > DateTime.Now) {
                lastmodified = DateTime.Now;
            }

            Helper.WriteData(newfilename, imagedata);
            File.SetLastWriteTime(newfilename, lastmodified);
            if (!orgfilename.Equals(newfilename, StringComparison.OrdinalIgnoreCase)) {
                Helper.DeleteToRecycleBin(orgfilename);
            }

            bitmap.Dispose();
            _added++;
        }

        private static float _importsum = 0f;
        private static int _importcnt = 0;

        public static void Compute(BackgroundWorker backgroundworker)
        {
            var dt = DateTime.Now;
            ImportInternal(backgroundworker);
            var st = (float)DateTime.Now.Subtract(dt).TotalSeconds;
            _importsum += st;
            _importcnt++;
            var avg = _importsum / _importcnt;
            backgroundworker.ReportProgress(0, $"a{_added}/f{_found}/b{_bad}/{avg:F2}s");

            for (var i = 0; i < 0; i++) {
                ComputeInternal(backgroundworker);
            }
        }
    }
}