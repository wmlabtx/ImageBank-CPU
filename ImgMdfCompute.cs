using OpenCvSharp;
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

            var minlastid = shadowcopy.Min(e => e.LastId);
            var img1 = shadowcopy.First(e => e.LastId == minlastid);
            var filename = Helper.GetFileName(img1.Name);
            var imagedata = Helper.ReadData(filename);
            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                Delete(img1.Id);
                return;
            }

            Mat d1 = ImageHelper.GetSiftDescriptors(bitmap);
            if (d1 == null) {
                Delete(img1.Id);
                return;
            }

            if (bitmap != null) {
                bitmap.Dispose();
            }

            if (img1.BestId != 0) {
                lock (_imglock) {
                    if (!_imgList.ContainsKey(img1.BestId)) {
                        img1.LastId = 0;
                    }
                }
                
            }

            var lastid = img1.LastId;
            for (var i = 0; i < shadowcopy.Length; i++) {
                var img2 = shadowcopy[i];
                if (img2.Id <= img1.LastId || img2.Id == img1.Id) {
                    continue;
                }

                filename = Helper.GetFileName(img2.Name);
                imagedata = Helper.ReadData(filename);
                if (!ImageHelper.GetBitmapFromImageData(imagedata, out bitmap)) {
                    Delete(img2.Id);
                    continue;
                }

                Mat[] d2 = ImageHelper.GetSift2Descriptors(bitmap);
                if (d2 == null) {
                    Delete(img2.Id);
                    continue;
                }

                if (bitmap != null) {
                    bitmap.Dispose();
                }

                var distance = ImageHelper.GetDistance(d1, d2);
                if (lastid == 0 || (lastid > 0 && distance < img1.BestDistance)) {
                    _sb.Clear();
                    _sb.Append($"a:{_added}/f:{_found}/b:{_bad} [{img1.Id}-{img2.Id}] {img1.BestDistance * 100f:F2} -> {distance*100f:F2}");
                    backgroundworker.ReportProgress(0, _sb.ToString());
                    img1.BestDistance = distance;
                    img1.BestId = img2.Id;
                }

                lastid = img2.Id;

                if (d2[0] != null) {
                    d2[0].Dispose();
                }

                if (d2[1] != null) {
                    d2[1].Dispose();
                }
            }

            if (d1 != null) {
                d1.Dispose();
            }

            if (img1.LastId != lastid) {
                img1.LastId = lastid;
            }
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
                    Delete(imgfound.Id);
                }
            }

            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                var badname = Path.GetFileName(orgfilename);
                var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                Helper.DeleteToRecycleBin(badfilename);
                File.Move(orgfilename, badfilename);
                _bad++;
                return;
            }

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
                newname = Helper.GetName(hash, iteration);
                newfilename = Helper.GetFileName(newname);
            } while (File.Exists(newfilename));

            var lv = GetMinLastView();
            var id = AllocateId();
            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                datetaken: datetaken,
                lastid: 0,
                bestid: 0,
                bestdistance: 0f,
                lastview: lv);

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
        }
    }
}