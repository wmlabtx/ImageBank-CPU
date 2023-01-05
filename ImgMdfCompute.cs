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
        private static int _moved;

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

        private static void ImportFile(string path, string orgfilename, IProgress<string> progress)
        {
            var fdir = Path.GetDirectoryName(orgfilename);
            var fpath = fdir.Length == path.Length ? string.Empty : fdir.Substring(path.Length + 1);
            var fname = Path.GetFileName(orgfilename);
            var name = $"{fpath}\\{fname}";
            if (AppImgs.ContainsName(name)) {
                return;
            }

            var lastview = new DateTime(1990, 1, 1);

            progress.Report($"importing {name} (a:{_added})/f:{_found}/m:{_moved}/b:{_bad}){AppConsts.CharEllipsis}");

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
                // we have a record with the same hash...
                lastview = imgfound.LastView;
                var filenamefound = FileHelper.NameToFileName(imgfound.Name);
                if (File.Exists(filenamefound)) {
                    // we have a file
                    var foundimagedata = FileHelper.ReadFile(filenamefound);
                    var foundhash = HashHelper.Compute(foundimagedata);
                    if (imgfound.Hash.Equals(foundhash)) {
                        // and file is okay
                        var foundlastmodified = File.GetLastWriteTime(orgfilename);
                        if (foundlastmodified > lastmodified) {
                            File.SetLastWriteTime(filenamefound, lastmodified);
                        }

                        if (path.Equals(AppConsts.PathRw) || name[0].Equals(AppConsts.CharLe)) {
                            // the org file is in RW or in a pool
                            FileHelper.DeleteToRecycleBin(orgfilename);
                            _found++;
                            return;
                        }

                        // the org file is in named folder
                        Delete(imgfound.Hash, progress);
                    }
                    else {
                        // but found file was changed or corrupted
                        FileHelper.WriteFile(filenamefound, imagedata);
                        File.SetLastWriteTime(filenamefound, lastmodified);
                        FileHelper.DeleteToRecycleBin(orgfilename);
                        _found++;
                        return;
                    }
                }
                else {
                    // ...but file is missing
                    AppImgs.SetName(imgfound, name);
                    _moved++;
                    return;
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

                using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, RotateFlipType.RotateNoneFlipNone)) {
                    vector = VggHelper.CalculateVector(bitmap);
                    extention = magickImage.Format.ToString().ToLowerInvariant();
                }
            }

            string newname;
            string newfilename;
            string newpath = path.Equals(AppConsts.PathRw) ?
                $"{AppConsts.CharLe}\\{hash.Substring(0, 1)}\\{hash.Substring(1, 1)}" :
                fpath;

            var iteration = 7;
            do {
                iteration++;
                newname = $"{newpath}\\{hash.Substring(0, iteration)}.{extention}";
                newfilename = $"{AppConsts.PathHp}\\{newname}";
            } while (File.Exists(newfilename));

            if (lastview.Year == 1990) {
                lastview = AppImgs.GetMinLastView();
            }

            var nimg = new Img(
                name: newname,
                hash: hash,
                orientation: RotateFlipType.RotateNoneFlipNone,
                lastview: lastview,
                vector: vector);

            if (!orgfilename.Equals(newfilename)) {
                FileHelper.WriteFile(newfilename, imagedata);
                File.SetLastWriteTime(newfilename, lastmodified);
            }

            var vimagedata = FileHelper.ReadFile(newfilename);
            if (vimagedata == null) {
                FileHelper.DeleteToRecycleBin(newfilename);
                return;
            }

            var vhash = HashHelper.Compute(vimagedata);
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
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    ImportFile(path, orgfilename, progress);
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
            _moved = 0;
            ImportFiles(AppConsts.PathHp, progress);
            ImportFiles(AppConsts.PathRw, progress);
            progress.Report($"Import done (a:{_added}/f:{_found}/m:{_moved}/b:{_bad})");
        }

        public static void Combine(IProgress<string> progress)
        {
            var imgX = AppPanels.GetImgPanel(0).Img;
            var imgY = AppPanels.GetImgPanel(1).Img;
            var folderX = FileHelper.NameToFolder(imgX.Name);
            var folderY = FileHelper.NameToFolder(imgY.Name);
            if (folderX.Equals(folderY)) {
                return;
            }

            if (folderX[0] == AppConsts.CharLe && folderY[0] != AppConsts.CharLe) {
                var filenameY = FileHelper.NameToFileName(imgY.Name);
                var pathY = Path.GetDirectoryName(filenameY);
                var filenameX = FileHelper.NameToFileName(imgX.Name);
                var nameX = Path.GetFileName(filenameX);
                var newfilenameX = Path.Combine(pathY, nameX);
                var newdirX = Path.GetDirectoryName(newfilenameX);
                var newpathX = newdirX.Substring(AppConsts.PathHp.Length + 1);
                var newnameX = $"{newpathX}\\{nameX}";
                File.Move(filenameX, newfilenameX);
                progress.Report($"{imgX.Name} {AppConsts.CharRightArrow} {newnameX}");
                AppImgs.SetName(imgX, newnameX);
                var similars = GetSimilars(imgX, progress);
                AppPanels.SetSimilars(similars, progress);
            }
            else {
                if (folderY[0] == AppConsts.CharLe && folderX[0] != AppConsts.CharLe) {
                    var filenameX = FileHelper.NameToFileName(imgX.Name);
                    var pathX = Path.GetDirectoryName(filenameX);
                    var filenameY = FileHelper.NameToFileName(imgY.Name);
                    var nameY = Path.GetFileName(filenameY);
                    var newfilenameY = Path.Combine(pathX, nameY);
                    var newdirY = Path.GetDirectoryName(newfilenameY);
                    var newpathY = newdirY.Substring(AppConsts.PathHp.Length + 1);
                    var newnameY = $"{newpathY}\\{nameY}";
                    File.Move(filenameY, newfilenameY);
                    progress.Report($"{imgY.Name} {AppConsts.CharRightArrow} {newnameY}");
                    AppImgs.SetName(imgY, newnameY);
                }

            }
        }
    }
}
