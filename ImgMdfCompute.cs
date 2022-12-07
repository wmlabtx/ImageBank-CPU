using ImageMagick;
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

        public static List<Tuple<string, float>> GetSimilars(Img imgX, IProgress<string> progress)
        {
            var name = imgX.Name;
            var filename = FileHelper.NameToFileName(name);
            if (!File.Exists(filename)) {
                progress?.Report($"({name}) removed");
                Delete(imgX.Hash, progress);
                return null;
            }

            var similars = AppImgs.GetSimilars(imgX);
            return similars;
        }

        private static void ImportFile(string orgfilename, IProgress<string> progress)
        {
            if (!File.Exists(orgfilename)) {
                return;
            }

            var fpath = Path.GetDirectoryName(orgfilename).Substring(AppConsts.PathHp.Length + 1);
            var fname = Path.GetFileNameWithoutExtension(orgfilename);
            var name = $"{fpath}\\{fname}";
            if (AppImgs.ContainsName(name)) {
                return;
            }

            progress.Report($"importing {name} (a:{_added})/f:{_found}/b:{_bad}){AppConsts.CharEllipsis}");

            var lastmodified = File.GetLastWriteTime(orgfilename);
            if (lastmodified > DateTime.Now) {
                lastmodified = DateTime.Now;
            }

            byte[] imagedata;
            var orgextension = Path.GetExtension(orgfilename);
            if (orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ||
                orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                var password = Path.GetFileNameWithoutExtension(orgfilename);
                imagedata = File.ReadAllBytes(orgfilename);
                var decrypteddata = orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ?
                    EncryptionHelper.DecryptDat(imagedata, password) :
                    EncryptionHelper.Decrypt(imagedata, password);

                if (decrypteddata != null) {
                    imagedata = decrypteddata;
                }
            }
            else {
                imagedata = File.ReadAllBytes(orgfilename);
            }

            var hash = HashHelper.Compute(imagedata);
            var found = AppImgs.TryGetValue(hash, out var imgfound);
            if (found) {
                var filenamefound = FileHelper.NameToFileName(imgfound.Name);
                if (File.Exists(filenamefound)) {
                    var foundimagedata = FileHelper.ReadEncryptedFile(filenamefound);
                    if (foundimagedata != null) {
                        var foundhash = HashHelper.Compute(foundimagedata);
                        if (imgfound.Hash.Equals(foundhash)) {
                            var foundlastmodified = File.GetLastWriteTime(orgfilename);
                            if (foundlastmodified > lastmodified) {
                                File.SetLastWriteTime(filenamefound, lastmodified);
                            }

                            FileHelper.DeleteToRecycleBin(orgfilename);
                            _found++;
                            return;
                        }
                    }
                }

                Delete(imgfound.Hash, progress);
            }

            byte[] vector;
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

                using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, RotateFlipType.RotateNoneFlipNone)) {
                    vector = VggHelper.CalculateVector(bitmap);
                }
            }

            string newname;
            string newfilename;
            var iteration = 7;
            do {
                iteration++;
                newname = $"{hash.Substring(0, 1)}\\{hash.Substring(1, 1)}\\{hash.Substring(2, iteration - 2)}";
                 newfilename = $"{AppConsts.PathHp}\\{newname}{AppConsts.MzxExtension}";
            } while (File.Exists(newfilename));

            var lastview = AppImgs.GetMinLastView();
            var lastcheck = AppImgs.GetMinLastCheck();
            var nimg = new Img(
                name: newname,
                hash: hash,
                orientation: RotateFlipType.RotateNoneFlipNone,
                counter: 0,
                lastview: lastview,
                besthash: string.Empty,
                distance: 1f,
                lastcheck: lastcheck,
                vector: vector);

            FileHelper.WriteEncryptedFile(newfilename, imagedata);
            File.SetLastWriteTime(newfilename, lastmodified);

            var vimagedata = FileHelper.ReadEncryptedFile(newfilename);
            if (vimagedata == null) {
                FileHelper.DeleteToRecycleBin(newfilename);
                return;
            }

            var vhash = HashHelper.Compute(vimagedata);
            if (!hash.Equals(vhash)) {
                FileHelper.DeleteToRecycleBin(newfilename);
                return;
            }

            FileHelper.DeleteToRecycleBin(orgfilename);
            Add(nimg);
            AppDatabase.AddImage(nimg);
            _added++;
        }

        public static void ImportFiles(string path, IProgress<string> progress)
        {
            progress.Report($"importing {path}{AppConsts.CharEllipsis}");
            var directoryInfo = new DirectoryInfo(path);
            var fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    ImportFile(orgfilename, progress);
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
            /*
            for (var i = 0; i < _added + 10; i++) {
                ComputeInternal(progress);
            }
            */

            progress.Report($"Import done (a:{_added})/f:{_found}/b:{_bad})");
        }

        public static void ComputeInternal(IProgress<string> progress)
        {
            var img1 = AppImgs.GetNextCheck();
            if (img1 == null) {
                return;
            }

            AppImgs.GetSimilar(img1, out string besthash, out float distance);
            if (!besthash.Equals(img1.BestHash)) {
                var age = Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastView));
                progress.Report($"[{age} ago] {img1.Name}: {img1.Distance:F2} {AppConsts.CharRightArrow} {distance:F2}");
                img1.SetBestHash(besthash);
                img1.SetCounter(0);
            }

            if (Math.Abs(distance - img1.Distance) > 0.01f) {
                img1.SetDistance(distance);
            }

            img1.SetLastCheck(DateTime.Now);
        }
    }
}
