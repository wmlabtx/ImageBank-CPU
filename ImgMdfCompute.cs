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
        private static int _computedcounter = 0; 

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
                    .Where(e => !e.Value.FileName.Equals(img1.FileName, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (candidates.Length == 0) {
                backgroundworker.ReportProgress(0, "no candidates");
                return;
            }

            var nexthash = img1.NextHash;
            var kazematch = img1.KazeMatch;
            var lastchanged = img1.LastChanged;
            var dmax = img1.KazeOne.Length;

            for (var i = 0; i < candidates.Length; i++) {
                var m = ImageHelper.ComputeKazeMatch(img1.KazeOne, candidates[i].KazeOne, candidates[i].KazeTwo);
                var d = Math.Abs(img1.KazeOne.Length - candidates[i].KazeOne.Length);
                if (m > kazematch || (m == kazematch && d < dmax)) {
                    lock (_imglock) {
                        if (_imgList.ContainsKey(img1.FileName) && _imgList.ContainsKey(candidates[i].FileName)) {
                            nexthash = candidates[i].Hash;
                            kazematch = m;
                            lastchanged = DateTime.Now;
                            dmax = d;
                        }
                    }
                }
            }

            if (!nexthash.Equals(img1.NextHash)) {
                var sb = new StringBuilder();
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img1.FileName}: ");
                sb.Append($"{img1.KazeMatch} ");
                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                sb.Append($"{kazematch} ");
                backgroundworker.ReportProgress(0, sb.ToString());
            }

            if (!nexthash.Equals(img1.NextHash)) {
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

            lock (_imglock) {
                if (_imgList.ContainsKey(orgfilename)) {
                    return;
                }
            }

            var imagedata = File.ReadAllBytes(orgfilename);
            if (imagedata == null || imagedata.Length < 16) {
                File.Move(orgfilename, $"{orgfilename}{AppConsts.CorruptedExtension}");
                return;
            }

            if (orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ||
                orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                string newfilename;
                var password = Path.GetFileNameWithoutExtension(orgfilename);
                var descrypteddata = orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ?
                    Helper.DecryptDat(imagedata, password) :
                    Helper.Decrypt(imagedata, password);

                if (descrypteddata == null) {
                    newfilename = Path.ChangeExtension(orgfilename, AppConsts.JpgExtension);
                    var directory = Path.GetDirectoryName(orgfilename);
                    while (File.Exists(newfilename)) {
                        var randomname = Helper.GetRandomName();
                        newfilename = $"{directory}\\{randomname}{AppConsts.JpgExtension}";
                    }

                    File.Move(orgfilename, newfilename);
                }
                else {
                    imagedata = descrypteddata;
                    newfilename = Path.ChangeExtension(orgfilename, AppConsts.JpgExtension);
                    File.WriteAllBytes(newfilename, imagedata);
                    Helper.DeleteToRecycleBin(orgfilename);
                }

                orgfilename = newfilename;
            }

            var magicformat = ImageHelper.GetMagicFormat(imagedata);
            if (magicformat == MagicFormat.Jpeg) {
                if (imagedata[0] != 0xFF || imagedata[1] != 0xD8 || imagedata[imagedata.Length - 2] != 0xFF || imagedata[imagedata.Length - 1] != 0xD9) {
                    if (!ImageHelper.GetBitmapFromImageData(imagedata, out var corruptedbitmap)) {
                        ImageHelper.SaveCorruptedFile(orgfilename);
                        return;
                    }
                    else {
                        if (!ImageHelper.GetImageDataFromBitmap(corruptedbitmap, out var fixedimagedata)) {
                            ImageHelper.SaveCorruptedFile(orgfilename);
                            corruptedbitmap.Dispose();
                            return;
                        }
                        else {
                            orgfilename = ImageHelper.SaveCorruptedImage(orgfilename, fixedimagedata);
                            imagedata = fixedimagedata;
                            corruptedbitmap.Dispose();
                        }
                    }
                }
            }

            var hash = Helper.ComputeHash(imagedata);
            lock (_imglock) {
                if (_hashList.TryGetValue(hash, out var imgfound)) {

                    // we found the same image in a database
                    if (File.Exists(imgfound.FileName)) {
                        // no reason to add the same image from a heap; we have one
                        Helper.DeleteToRecycleBin(orgfilename);
                        return;
                    }
                    else {
                        // found image is gone; delete it
                        Delete(imgfound.FileName);
                    }
                }

                // it is a new image; we don't have one 
                if (_imgList.Count >= AppConsts.MaxImages) {
                    return;
                }

                if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                    ImageHelper.SaveCorruptedFile(orgfilename);
                    return;
                }

                if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                    bitmap = ImageHelper.RepixelBitmap(bitmap);
                }

                ImageHelper.ComputeKazeDescriptors(bitmap, out var kazeone, out var kazetwo);
                if (kazeone == null || kazeone.Length == 0) {
                    var baddir = $"{AppConsts.PathHp}\\{AppConsts.CorruptedExtension}";
                    if (!Directory.Exists(baddir)) {
                        Directory.CreateDirectory(baddir);
                    }

                    var badname = Path.GetFileName(orgfilename);
                    var badfilename = $"{baddir}\\{badname}{AppConsts.CorruptedExtension}";
                    File.Move(orgfilename, badfilename);
                    return;
                }

                ImageHelper.GetExif(orgfilename, out var datetaken, out var metadata);

                var lc = GetMinLastCheck();
                var lv = GetMinLastView();

                var orgpath = Path.GetDirectoryName(orgfilename);
                if (orgpath.StartsWith(AppConsts.PathRw, StringComparison.OrdinalIgnoreCase)) {

                    // we have to create unique name and a location in Hp folder
                    var hpsubfolder = hash.Substring(hash.Length - 2, 2);
                    var newfilename = orgfilename;
                    var extension = Path.GetExtension(orgfilename);
                    do {
                        var randomname = Helper.GetRandomName();
                        newfilename = $"{AppConsts.PathHp}\\{hpsubfolder}\\{randomname}{extension}";
                    }
                    while (File.Exists(newfilename));

                    var dir = $"{AppConsts.PathHp}\\{hpsubfolder}";
                    if (!Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }

                    var nimg = new Img(
                        filename: newfilename,
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
                        counter: 0);

                    Add(nimg);

                    var lastmodified = File.GetLastWriteTime(orgfilename);
                    if (lastmodified > DateTime.Now) {
                        lastmodified = DateTime.Now;
                    }

                    File.WriteAllBytes(newfilename, imagedata);
                    File.SetLastWriteTime(newfilename, lastmodified);
                    if (!orgfilename.Equals(newfilename, StringComparison.OrdinalIgnoreCase)) {
                        Helper.DeleteToRecycleBin(orgfilename);
                    }
                }
                else {

                    // we are adding new image to a database
                    var nimg = new Img(
                        filename: orgfilename,
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
                        counter: 0);

                    Add(nimg);
                }

                bitmap.Dispose();
            }
        }

        public static void Compute(BackgroundWorker backgroundworker)
        {
            if (_computedcounter >= 2) {
                ImportInternal(backgroundworker);
                _computedcounter = 0;
            }
            else {
                ComputeInternal(backgroundworker);
                _computedcounter++;
            }
        }
    }
}