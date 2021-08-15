using System;
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

            var nexthash = img1.Hash;
            var maxsim = 0f;
            for (var i = 0; i < 2; i++) {
                var candidates = GetCandidates(i, img1);
                for (var j = 0; j < candidates.Length; j++) {
                    if (img1.Name.Equals(candidates[j].Name, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    var sim = ImageHelper.GetCosineSimilarity(img1.Vector[0], candidates[j].Vector[i]);
                    if (sim > maxsim) {
                        nexthash = candidates[i].Hash;
                        maxsim = sim;
                    }
                }
            }

            if (!nexthash.Equals(img1.NextHash) || img1.Sim != maxsim) {
                img1.Generation = 0;
                img1.LastChanged = DateTime.Now;
                var sb = new StringBuilder();
                sb.Append($"a{_added}/f{_found}/b{_bad}/{_rwList.Count / 1024}K ");
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img1.Name}[{img1.Generation}]: ");
                sb.Append($"{img1.Sim:F2} ");
                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                sb.Append($"{maxsim:F2} ");
                backgroundworker.ReportProgress(0, sb.ToString());
            }

            if (!nexthash.Equals(img1.NextHash, StringComparison.OrdinalIgnoreCase)) {
                img1.NextHash = nexthash;
            }

            if (img1.Sim != maxsim) {
                img1.Sim = maxsim;
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

            // it is a new image; we don't have one 
            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                var badname = Path.GetFileName(orgfilename);
                var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                Helper.DeleteToRecycleBin(badfilename);
                File.Move(orgfilename, badfilename);
                return;
            }

            ImageHelper.ComputeVector(bitmap, out var net0, out var net1);
            if (net0 == null || net0.Length == 0 || net1 == null || net1.Length == 0) {
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
                width: bitmap.Width,
                height: bitmap.Height,
                size: imagedata.Length,
                datetaken: datetaken,
                metadata: metadata,
                vector0: net0,
                node0: 0,
                vector1: net1,
                node1: 0,
                nexthash: hash,
                sim: 0f,
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

            lock (_imglock) {
                var sb = new StringBuilder();
                sb.Append($"a{_added}/f{_found}/b{_bad}/{_rwList.Count / 1024}K");
                backgroundworker.ReportProgress(0, sb.ToString());
            }
        }

        public static void Compute(BackgroundWorker backgroundworker)
        {
            ImportInternal(backgroundworker);
            for (var i = 0; i < 1; i++) {
                ComputeInternal(backgroundworker);
            }
        }
    }
}