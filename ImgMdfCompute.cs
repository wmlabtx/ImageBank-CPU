using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static int _added;
        private static int _bad;
        private static int _found;

        public static List<Tuple<int, float>> GetSimilars(Img imgX, IProgress<string> progress)
        {
            var name = imgX.Name;
            var filename = FileHelper.NameToFileName(name);
            if (!File.Exists(filename)) {
                progress?.Report($"({imgX.Id}) removed");
                Delete(imgX.Id);
                return null;
            }

            var hist = imgX.GetHist();
            if (hist == null || hist.Length == 0) {
                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    progress?.Report($"({imgX.Id}) removed");
                    Delete(imgX.Id);
                    return null;
                }

                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    if (bitmap == null) {
                        progress?.Report($"({imgX.Id}) removed");
                        Delete(imgX.Id);
                        return null;
                    }

                    progress?.Report($"{imgX.Id} calculating vector...");
                    hist = AppPalette.ComputeHist(bitmap);
                    imgX.SetHist(hist);
                }
            }

            var similars = AppImgs.GetSimilars(imgX);
            return similars;
        }

        private static void ImportFile(string orgfilename)
        {
            if (!File.Exists(orgfilename)) {
                return;
            }

            var imagedata = File.ReadAllBytes(orgfilename);
            if (imagedata.Length < 256) {
                FileHelper.MoveCorruptedFile(orgfilename);
                _bad++;
                return;
            }

            int year = DateTime.Now.Year;
            var orgextension = Path.GetExtension(orgfilename);
            if (orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ||
                orgextension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                year = 0;
                var password = Path.GetFileNameWithoutExtension(orgfilename);
                var decrypteddata = orgextension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) ?
                    EncryptionHelper.DecryptDat(imagedata, password) :
                    EncryptionHelper.Decrypt(imagedata, password);

                if (decrypteddata != null) {
                    imagedata = decrypteddata;
                }
            }

            var hash = Md5HashHelper.Compute(imagedata);
            bool found;
            found = AppImgs.TryGetHash(hash, out var imgfound);
            if (found) {
                // we found the same image in a database
                var filenamefound = FileHelper.NameToFileName(imgfound.Name);
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
                Delete(imgfound.Id);
            }

            float[] hist;
            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
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

                hist = AppPalette.ComputeHist(bitmap);
            }

            // we have to create unique name and a location in Hp folder
            string newname;
            string newfilename;
            var iteration = -1;
            do {
                iteration++;
                newname = FileHelper.HashToName(hash, iteration);
                newfilename = FileHelper.NameToFileName(newname);
            } while (File.Exists(newfilename));

            var id = AppVars.AllocateId();
            var lastview = AppImgs.GetMinLastView();
            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                year: year,
                lastview: lastview,
                familyid: 0,
                hist: hist);

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
            progress?.Report($"importing {AppConsts.PathHp}...");
            var directoryInfo = new DirectoryInfo(AppConsts.PathHp);
            var fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                var p1 = Path.GetDirectoryName(orgfilename)?.Substring(AppConsts.PathHp.Length + 1);
                if (p1 != null && p1.Length == 2) {
                    var p2 = Path.GetFileNameWithoutExtension(orgfilename);
                    if (p2.Length == 8) {
                        var key = $"{p1}{p2}";
                        if (AppImgs.ContainsName(key)) {
                            continue;
                        }
                    }
                }

                ImportFile(orgfilename);
                progress?.Report($"importing {AppConsts.PathHp} (a:{_added})/f:{_found}/b:{_bad}...");
            }

            progress?.Report($"importing {AppConsts.PathRw}...");
            directoryInfo = new DirectoryInfo(AppConsts.PathRw);
            fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(e => e.Length).Take(1000).ToArray();
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                progress?.Report($"importing {AppConsts.PathRw} (a:{_added})/f:{_found}/b:{_bad}...");
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    ImportFile(orgfilename);
                    progress?.Report($"importing {AppConsts.PathHp} (a:{_added})/f:{_found}/b:{_bad}...");
                }
            }

            progress?.Report($"clean-up {AppConsts.PathHp}...");
            Helper.CleanupDirectories(AppConsts.PathHp, AppVars.Progress);
            progress?.Report($"clean-up {AppConsts.PathRw}...");
            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);
            progress?.Report($"Import done (a:{_added})/f:{_found}/b:{_bad})");
        }
    }
}
