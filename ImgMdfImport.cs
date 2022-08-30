using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static int _added;
        private static int _bad;
        private static int _found;

        private static void Import(string orgfilename, IProgress<string> progress)
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
            found = _hashList.TryGetValue(hash, out var imgfound);
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

            float[] palette;
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

                palette = ComputePalette(bitmap);
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

            var id = AllocateId();
            var lastview = new DateTime(2020, 1, 1).AddSeconds(_random.Next(0, 60 * 60 * 24 * 365));
            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                palette: palette,
                distance: 2f,
                year: year,
                bestid: 0,
                lastview: lastview,
                ni: Array.Empty<int>());

            Add(nimg);

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

        public static void Import(int max, IProgress<string> progress)
        {
            var imgcount = _imgList.Count;
            var diff = imgcount - _importLimit;
            if (diff > 0) {
                return;
            }

            DecreaseImportLimit();

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
                        if (_nameList.ContainsKey(key)) {
                            continue;
                        }
                    }
                }

                Import(orgfilename, progress);
                progress?.Report($"importing {AppConsts.PathHp} (a:{_added})/f:{_found}/b:{_bad}...");
            }

            progress?.Report($"importing {AppConsts.PathRw}...");
            directoryInfo = new DirectoryInfo(AppConsts.PathRw);
            fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(e => e.Length).Take(max).ToArray();
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                progress?.Report($"importing {AppConsts.PathRw} (a:{_added})/f:{_found}/b:{_bad}...");
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    Import(orgfilename, progress);
                    progress?.Report($"importing {AppConsts.PathHp} (a:{_added})/f:{_found}/b:{_bad}...");
                }
            }

            progress?.Report($"clean-up {AppConsts.PathHp}...");
            Helper.CleanupDirectories(AppConsts.PathHp, AppVars.Progress);
            progress?.Report($"clean-up {AppConsts.PathRw}...");
            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);
        }
    }
}
