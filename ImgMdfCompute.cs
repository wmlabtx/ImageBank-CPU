using System;
using System.Collections.Generic;
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

        private static void ImportFile(string orgfilename, IProgress<string> progress)
        {
            var name = Path.GetFileNameWithoutExtension(orgfilename);
            if (AppImgs.ContainsHash(name)) {
                return;
            }

            var lastview = new DateTime(1990, 1, 1);

            progress.Report($"importing {name} (a:{_added})/f:{_found}/b:{_bad}){AppConsts.CharEllipsis}");

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
            var folder = AppImgs.GetFolder();
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
                    Delete(imgfound.Hash, progress);
                }
            }

            float[] histogram;
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
                    histogram = ColorHelper.CalculateHistogram(bitmap);
                    vector = VggHelper.CalculateVector(bitmap);
                    extention = magickImage.Format.ToString().ToLowerInvariant();
                }
            }

            if (lastview.Year == 1990) {
                lastview = AppImgs.GetMinLastView();
            }

            var nimg = new Img(
                hash: hash,
                folder: folder,
                datetaken: lastmodified,
                histogram: histogram,
                vector: vector,
                lastview: lastview,
                orientation: RotateFlipType.RotateNoneFlipNone,
                besthash: hash);

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

        public static void ImportFiles(string path, IProgress<string> progress)
        {
            progress.Report($"importing {path}{AppConsts.CharEllipsis}");
            var directoryInfo = new DirectoryInfo(path);
            var fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            var count = 0;
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    ImportFile(orgfilename, progress);
                    count++;
                    if (count == AppConsts.MaxImportFiles) {
                        break;
                    }
                }
            }

            progress.Report($"clean-up {path}{AppConsts.CharEllipsis}");
            Helper.CleanupDirectories(path, AppVars.Progress);
        }

        public static void Import(IProgress<string> progress)
        {
            _added = 0;
            _found = 0;
            _bad = 0;
            ImportFiles(AppConsts.PathHp, progress);
            ImportFiles(AppConsts.PathRw, progress);
            progress.Report($"Import done (a:{_added}/f:{_found}/b:{_bad})");
        }
    }
}
