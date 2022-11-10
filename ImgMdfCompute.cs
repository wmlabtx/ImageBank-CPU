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
            if (!File.Exists(filename)) {
                backgroundworker.ReportProgress(0, $"({img1.Id}) removed");
                Delete(img1.Id);
                return;
            }

            var vector = AppDatabase.ImageGetVector(img1.Id);
            if (vector == null || vector.Length != 4096) {
                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    backgroundworker.ReportProgress(0, $"({img1.Id}) removed");
                    Delete(img1.Id);
                    return;
                }

                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    if (bitmap == null) {
                        backgroundworker.ReportProgress(0, $"({img1.Id}) removed");
                        Delete(img1.Id);
                        return;
                    }

                    backgroundworker.ReportProgress(0, $"{img1.Id} calculating vector...");
                    vector = VggHelper.CalculateVector(bitmap);
                    AppDatabase.ImageSetVector(img1.Id, vector);
                    Thread.Sleep(1000);
                }
            }

            var oldclusterid = img1.ClusterId;
            var oldbestid = img1.BestId;
            float olddistance = img1.Distance;
            AppClusters.Compute(img1, vector, out int clusterid, out int bestid, out float distance);
            if (clusterid != oldclusterid) {
                img1.SetClusterId(clusterid);
                if (oldclusterid != 0) {
                    AppClusters.Update(oldclusterid);
                }

                if (clusterid != 0) {
                    AppClusters.Update(clusterid);
                }
            }

            if (distance != olddistance) {
                img1.SetDistance(distance);
            }

            if (bestid != oldbestid) {
                img1.SetBestId(bestid);
                var age = Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastView));
                backgroundworker.ReportProgress(0, $"[{age} ago] {img1.Id}: [{oldclusterid}] {olddistance:F2} \u2192 [{clusterid}] {distance:F2}");
                AppClusters.DeleteAged();
            }

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
                Thread.Sleep(1000);
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
                distance: 0f,
                year: year,
                bestid: 0,
                lastview: lastview,
                lastcheck: lastcheck,
                clusterid: 0);

            Add(nimg, vector);

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
