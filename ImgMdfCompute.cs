using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static int _added;
        private static int _bad;
        private static int _found;

        public static List<string> GetSimilars(Img imgX, IProgress<string> progress)
        {
            var filename = imgX.GetFileName();
            if (!File.Exists(filename)) {
                var shortname = Path.GetFileName(filename);
                progress?.Report($"({shortname}) removed");
                Delete(imgX.Hash, progress);
                return null;
            }

            var similars = AppImgs.GetSimilars(imgX);
            return similars;
        }

        private static void ImportFile(string orgfilename, BackgroundWorker backgroundworker)
        {
            var name = Path.GetFileNameWithoutExtension(orgfilename);
            if (AppImgs.ContainsHash(name)) {
                return;
            }

            var lastview = new DateTime(1990, 1, 1);

            backgroundworker.ReportProgress(0, $"importing {name} (a:{_added})/f:{_found}/b:{_bad}){AppConsts.CharEllipsis}");

            var lastmodified = File.GetLastWriteTime(orgfilename);
            if (lastmodified > DateTime.Now) {
                lastmodified = DateTime.Now;
            }

            byte[] imagedata;
            var orgextension = Path.GetExtension(orgfilename);
            if (orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ||
                orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                imagedata = File.ReadAllBytes(orgfilename);
                var decrypteddata = orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ?
                    EncryptionHelper.DecryptDat(imagedata, name) :
                    EncryptionHelper.Decrypt(imagedata, name);

                if (decrypteddata != null) {
                     imagedata = decrypteddata;
                }
            }
            else {
                imagedata = File.ReadAllBytes(orgfilename);
            }

            var hash = FileHelper.GetHash(imagedata);
            var found = AppImgs.TryGetValue(hash, out var imgfound);
            if (found) {
                // we have a record with the same hash...
                lastview = imgfound.LastView;
                var filenamefound = imgfound.GetFileName();
                if (File.Exists(filenamefound)) {
                    // we have a file
                    var foundimagedata = FileHelper.ReadEncryptedFile(filenamefound);
                    var foundhash = FileHelper.GetHash(foundimagedata);
                    if (imgfound.Hash.Equals(foundhash)) {
                        // and file is okay
                        var foundlastmodified = File.GetLastWriteTime(orgfilename);
                        if (foundlastmodified > lastmodified) {
                            File.SetLastWriteTime(filenamefound, lastmodified);
                            imgfound.SetDateTaken(lastmodified);
                        }
                    }
                    else {
                        // but found file was changed or corrupted
                        FileHelper.WriteEncryptedFile(filenamefound, imagedata);
                        File.SetLastWriteTime(filenamefound, lastmodified);
                        imgfound.SetDateTaken(lastmodified);
                    }

                    FileHelper.DeleteToRecycleBin(orgfilename);
                    _found++;
                    return;
                }
                else {
                    // ...but file is missing
                    Delete(imgfound.Hash, null);
                }
            }

            byte[] vector;
            string extention;
            using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                if (magickImage == null) {
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

                var datetaken = BitmapHelper.GetDateTaken(magickImage, DateTime.Now);
                if (datetaken < lastmodified) {
                    lastmodified = datetaken;
                }

                using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, RotateFlipType.RotateNoneFlipNone)) {
                    vector = VggHelper.CalculateVector(bitmap);
                    extention = magickImage.Format.ToString().ToLowerInvariant();
                }
            }

            /*
            if (lastview.Year == 1990) {
                lastview = AppImgs.GetMinLastView();
            }
            */

            var folder = AppImgs.GetFolder();
            var nimg = new Img(
                hash: hash,
                folder: folder,
                datetaken: lastmodified,
                vector: vector,
                lastview: lastview,
                orientation: RotateFlipType.RotateNoneFlipNone,
                distance: 1f,
                lastcheck: lastview,
                review: 0,
                next: hash);

            var newfilename = nimg.GetFileName();
            if (!orgfilename.Equals(newfilename)) {
                FileHelper.WriteEncryptedFile(newfilename, imagedata);
                File.SetLastWriteTime(newfilename, lastmodified);
            }

            var vimagedata = FileHelper.ReadEncryptedFile(newfilename);
            if (vimagedata == null) {
                FileHelper.DeleteToRecycleBin(newfilename);
                return;
            }

            var vhash = FileHelper.GetHash(vimagedata);
            if (!hash.Equals(vhash)) {
                FileHelper.DeleteToRecycleBin(newfilename);
                return;
            }

            if (!orgfilename.Equals(newfilename)) {
                FileHelper.DeleteToRecycleBin(orgfilename);
            }

            Add(nimg);
            AppDatabase.AddImage(nimg);

            _added++;
        }

        public static void ImportFiles(string path, BackgroundWorker backgroundworker)
        {
            var directoryInfo = new DirectoryInfo(path);
            var fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            var count = 0;
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    ImportFile(orgfilename, backgroundworker);
                    count++;
                    if (count == AppConsts.MaxImportFiles) {
                        break;
                    }
                }
            }

            backgroundworker.ReportProgress(0, $"clean-up {path}{AppConsts.CharEllipsis}");
            Helper.CleanupDirectories(path, AppVars.Progress);
        }

        public static void BackgroundWorker(BackgroundWorker backgroundworker)
        {
            Compute(backgroundworker);
        }

        private static void Compute(BackgroundWorker backgroundworker)
        {
            if (AppVars.ImportRequested) {
                _added = 0;
                _found = 0;
                _bad = 0;
                ImportFiles(AppConsts.PathHp, backgroundworker);
                ImportFiles(AppConsts.PathRw, backgroundworker);
                AppVars.ImportRequested = false;
            }

            var imgX = AppImgs.GetNextCheck();
            if (imgX != null) {
                var shadow = AppImgs.GetShadow();
                shadow.Remove(imgX.Hash);

                var mindistance = 1f;
                var minnext = string.Empty;
                foreach (var img in shadow.Values) {
                    var distance = VggHelper.GetDistance(imgX.GetVector(), img.GetVector());
                    if (distance < mindistance) {
                        mindistance = distance;
                        minnext = img.Hash;
                    }
                        
                }

                if (mindistance < imgX.Distance || !minnext.Equals(imgX.Next)) {
                    var age = Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck));
                    var shortfilename = imgX.GetShortFileName();
                    backgroundworker.ReportProgress(0, $"[{age} ago] {shortfilename}: {imgX.Distance:F4} {AppConsts.CharRightArrow} {mindistance:F4}");
                    imgX.SetDistance(mindistance);
                    imgX.SetNext(minnext);
                    imgX.SetReview(0);
                }

                imgX.SetLastCheck(DateTime.Now);

                var imgY = shadow[imgX.Next];
                if (imgY != null) {
                    if (imgY.Hash.Equals(imgY.Next) || !shadow.ContainsKey(imgY.Next) || mindistance < imgY.Distance) {
                        var age = Helper.TimeIntervalToString(DateTime.Now.Subtract(imgY.LastCheck));
                        var shortfilename = imgY.GetShortFileName();
                        backgroundworker.ReportProgress(0, $"[{age} ago] {shortfilename}: {imgY.Distance:F4} {AppConsts.CharRightArrow} {mindistance:F4}");
                        imgY.SetDistance(mindistance);
                        imgY.SetNext(imgX.Hash);
                        imgY.SetReview(0);
                        imgY.SetLastCheck(DateTime.Now);
                    }
                }
            }
        }
    }
}
