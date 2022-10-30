using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static int _added;
        private static int _bad;
        private static int _found;

        private static void ComputeInternal(BackgroundWorker backgroundworker)
        {
            var img1 = AppImgs.GetNextCheck();
            var name = img1.Name;
            var filename = FileHelper.NameToFileName(name);
            var imagedata = FileHelper.ReadData(filename);
            if (imagedata == null) {
                backgroundworker.ReportProgress(0, $"({img1.Id}) removed");
                Delete(img1.Id);
                return;
            }

            if (img1.GetVector().Length != 4096) {
                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    if (bitmap == null) {
                        backgroundworker.ReportProgress(0, $"({img1.Id}) removed");
                        Delete(img1.Id);
                        return;
                    }

                    var vector = VggHelper.CalculateVector(bitmap);
                    img1.SetVector(vector);
                    Thread.Sleep(2000);
                }
            }

            /*
            var ni = img1.GetHistory();
            for (var i = 0; i < ni.Length; i++) {
                if (!AppImgs.TryGetValue(ni[i], out Img imgN)) {
                    img1.RemoveRank(ni[i]);
                }
                else {
                    if (img1.FamilyId > 0 && imgN.FamilyId == img1.FamilyId) {
                        img1.RemoveRank(ni[i]);
                    }
                }
            }
            */

            int idY = 0;
            var shadow = AppImgs.GetShadow();
            var bestdistance = 2f;
            foreach (var e in shadow) {
                if (e.Item1 == img1.Id || AppImgs.InHistory(img1.Id, e.Item1)) {
                    continue;
                }

                var distance = VggHelper.GetDistance(img1.GetVector(), e.Item2);
                if (distance < bestdistance) {
                    bestdistance = distance;
                    idY = e.Item1;
                }
            }

            AppImgs.TryGetValue(idY, out Img img2);
            if (img2 == null) {
                img2 = AppImgs.GetRandomImg();
            }

            var age = Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastView));
            if (img1.BestId != img2.Id) {
                img1.SetBestId(img2.Id);
                backgroundworker.ReportProgress(0, $"[{age} ago] {img1.Id}: {img1.Distance:F2} \u2192 {bestdistance:F2}");
            }
            else {
                backgroundworker.ReportProgress(0, $"[{age} ago] {img1.Id}: {img1.Distance:F2} = {bestdistance:F2}");
            }

            img1.SetDistance(bestdistance);
            img1.SetLastCheck(DateTime.Now);
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

            float[] vector;
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

                vector = VggHelper.CalculateVector(bitmap);
                //Thread.Sleep(1000);
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
            var lastcheck = AppImgs.GetMinLastCheck();
            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                vector: vector,
                distance: 0f,
                year: year,
                bestid: 0,
                lastview: lastview,
                lastcheck: lastcheck);

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

        private static void ImportInternal(BackgroundWorker backgroundworker)
        {
            _added = 0;
            _found = 0;
            _bad = 0;
            backgroundworker.ReportProgress(0, $"importing {AppConsts.PathHp}...");
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
                backgroundworker.ReportProgress(0, $"importing {AppConsts.PathHp} (a:{_added})/f:{_found}/b:{_bad}...");
            }

            backgroundworker.ReportProgress(0, $"importing {AppConsts.PathRw}...");
            directoryInfo = new DirectoryInfo(AppConsts.PathRw);
            fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(e => e.Length).Take(1000).ToArray();
            foreach (var e in fs) {
                var orgfilename = e.FullName;
                backgroundworker.ReportProgress(0, $"importing {AppConsts.PathRw} (a:{_added})/f:{_found}/b:{_bad}...");
                if (!Path.GetExtension(orgfilename).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    ImportFile(orgfilename);
                    backgroundworker.ReportProgress(0, $"importing {AppConsts.PathHp} (a:{_added})/f:{_found}/b:{_bad}...");
                }
            }

            backgroundworker.ReportProgress(0, $"clean-up {AppConsts.PathHp}...");
            Helper.CleanupDirectories(AppConsts.PathHp, AppVars.Progress);
            backgroundworker.ReportProgress(0, $"clean-up {AppConsts.PathRw}...");
            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);

            AppVars.ImportMode = false;
        }

        public static void Compute(BackgroundWorker backgroundworker)
        {
            if (AppVars.ImportMode) {
                ImportInternal(backgroundworker);
            }
            
            ComputeInternal(backgroundworker);
        }
    }
}
