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
            Img img2 = null;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                img1 = _imgList
                    .OrderBy(e => e.Value.LastCheck)
                    .FirstOrDefault()
                    .Value;

                if (!_hashList.TryGetValue(img1.BestHash, out img2)) {
                    img2 = img1;
                    if (!img1.BestHash.Equals(img1.Hash, StringComparison.OrdinalIgnoreCase)) {
                        img1.BestHash = img1.Hash;
                    }
                    
                    if (img1.Distance < 486f) {
                        img1.Distance = 486f;
                    }
                }

                var names = _imgList.Keys.ToArray();
                var i = 0;
                while (i < names.Length) {
                    if (!names[i].Equals(img1.Name)) {
                        if (names[i].CompareTo(img1.LastName) > 0) {
                            break;
                        }
                    }

                    i++;
                }

                if (i == names.Length) {
                    i = 0;
                    while (i < names.Length) {
                        if (!names[i].Equals(img1.Name)) {
                            break;
                        }

                        i++;
                    }
                }

                if (!_imgList.TryGetValue(names[i], out img2)) {
                    return;
                }
            }

            var idx = Helper.ReadData(Helper.GetFileName(img1.Name));
            if (idx == null || idx.Length < 16) {
                Delete(img1.Name);
                return;
            }

            var idy = Helper.ReadData(Helper.GetFileName(img2.Name));
            if (idy == null || idy.Length < 16) {
                Delete(img2.Name);
                return;
            }

            var distance = ImageHelper.GetDistance(idx, idy);
            if (distance < img1.Distance) {
                var sb = new StringBuilder();
                sb.Append($"a{_added}/f{_found}/b{_bad}/i{_rwList.Count / 1024}K ");
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img1.Name}[{img1.Generation}]: ");
                sb.Append($"{img1.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                sb.Append($"{distance:F2} ");
                backgroundworker.ReportProgress(0, sb.ToString());
                lock (_imglock) {
                    img1.Generation = 0;
                    img1.BestHash = img2.Hash;
                    img1.Distance = distance;
                    img1.LastChanged = DateTime.Now;
                }
            }

            distance = ImageHelper.GetDistance(idy, idx);
            if (distance < img2.Distance) {
                var sb = new StringBuilder();
                sb.Append($"a{_added}/f{_found}/b{_bad}/i{_rwList.Count / 1024}K ");
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img2.Name}[{img2.Generation}]: ");
                sb.Append($"{img2.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                sb.Append($"{distance:F2} ");
                backgroundworker.ReportProgress(0, sb.ToString());
                lock (_imglock) {
                    img2.Generation = 0;
                    img2.BestHash = img1.Hash;
                    img2.Distance = distance;
                    img2.LastChanged = DateTime.Now;
                }
            }

            lock (_imglock) {
                img1.LastCheck = DateTime.Now;
            }
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
                lastname: newname,
                besthash: hash,
                distance: 486f,
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