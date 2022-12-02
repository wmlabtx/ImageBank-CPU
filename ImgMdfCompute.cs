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
            var filename = $"{AppConsts.PathRoot}\\{name}";
            if (!File.Exists(filename)) {
                progress?.Report($"({imgX.Hash}) removed");
                Delete(imgX.Hash);
                return null;
            }

            var vector = imgX.GetVector();
            if (vector == null || vector.Length == 0) {
                var imagedata = File.ReadAllBytes(filename);
                if (imagedata == null) {
                    progress?.Report($"({imgX.Hash}) removed");
                    Delete(imgX.Hash);
                    return null;
                }

                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    if (bitmap == null) {
                        progress?.Report($"({imgX.Hash}) removed");
                        Delete(imgX.Hash);
                        return null;
                    }

                    progress?.Report($"{imgX.Hash.Substring(0, 7)}... calculating vector...");
                    vector = VggHelper.CalculateVector(bitmap);
                    imgX.SetVector(vector);
                }
            }

            var similars = AppImgs.GetSimilars(imgX);
            return similars;
        }

        private static void ImportLegacyFile(string orgfilename)
        {
            if (!File.Exists(orgfilename)) {
                return;
            }

            byte[] imagedata;
            int year = DateTime.Now.Year;
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
                if (imagedata.Length < 256) {
                    FileHelper.MoveCorruptedFile(orgfilename);
                    _bad++;
                    return;
                }
            }

            var hash = HashHelper.Compute(imagedata);
            bool found;
            found = AppImgs.TryGetValue(hash, out var imgfound);
            if (found) {
                // we found the same image in a database
                var filenamefound = $"{AppConsts.PathRoot}\\{imgfound.Name}";
                if (File.Exists(filenamefound)) {
                    // no reason to add the same image from a heap; we have one
                    FileHelper.DeleteToRecycleBin(orgfilename);
                    if (imgfound.Year == 0 && year != 0) {
                        imgfound.SetActualYear();
                    }

                    _found++;
                    return;
                }

                // found image is gone; delete it
                Delete(imgfound.Hash);
            }

            float[] vector;
            string newext;
            using (var image = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                if (image == null) {
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

                newext = BitmapHelper.GetRecommendedExt(image);
                using (var bitmap = image.ToBitmap()) {
                    vector = VggHelper.CalculateVector(bitmap);
                }
            }

            // we have to create unique name and a location in Hp folder
            string newname;
            string newfilename;
            var iteration = 6;
            do {
                iteration++;
                newname = $"{AppConsts.FolderLe}\\{hash.Substring(0, 1)}\\{hash.Substring(1, 1)}\\{hash.Substring(2, iteration - 2)}{newext}";
                newfilename = $"{AppConsts.PathRoot}\\{newname}";
            } while (File.Exists(newfilename));

            var lastview = AppImgs.GetMinLastView();
            var nimg = new Img(
                name: newname,
                hash: hash,
                year: year,
                lastview: lastview,
                vector: vector);

            Add(nimg);
            AppDatabase.AddImage(nimg);

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

        public static void Import(IProgress<string> progress)
        {
            _added = 0;
            _found = 0;
            _bad = 0;
            progress?.Report($"importing {AppConsts.PathLe}...");
            var directoryInfo = new DirectoryInfo(AppConsts.PathLe);
            var fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                var name = orgfilename.Substring(AppConsts.PathRoot.Length + 1);
                if (AppImgs.ContainsName(name)) {
                    continue;
                }

                ImportLegacyFile(orgfilename);
                progress?.Report($"importing {AppConsts.PathLe} (a:{_added})/f:{_found}/b:{_bad}...");
            }

            progress?.Report($"importing {AppConsts.PathRw}...");
            directoryInfo = new DirectoryInfo(AppConsts.PathRw);
            fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(e => e.Length).Take(1000).ToArray();
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                progress?.Report($"importing {AppConsts.PathRw} (a:{_added})/f:{_found}/b:{_bad}...");
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    ImportLegacyFile(orgfilename);
                    progress?.Report($"importing {AppConsts.PathLe} (a:{_added})/f:{_found}/b:{_bad}...");
                }
            }

            progress?.Report($"clean-up {AppConsts.PathLe}...");
            Helper.CleanupDirectories(AppConsts.PathLe, AppVars.Progress);
            progress?.Report($"clean-up {AppConsts.PathRw}...");
            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);
            progress?.Report($"Import done (a:{_added})/f:{_found}/b:{_bad})");
        }
    }
}
