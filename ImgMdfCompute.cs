using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static int _added; 
        private static int _bad;
        private static int _found;

        private static void ComputeInternal(BackgroundWorker backgroundworker)
        {
            Img img1 = null;
            SortedDictionary<int, Img> shadowcopy;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                shadowcopy = new SortedDictionary<int, Img>(_imgList);
            }

            foreach (var img in shadowcopy) {
                if (img.Value.GetHistogram().Length == 0 || !shadowcopy.ContainsKey(img.Value.BestId)) {
                    img1 = img.Value;
                    break;
                }

                if (img1 == null || img.Value.LastCheck < img1.LastCheck) {
                    img1 = img.Value;
                }
            }

            if (img1 == null) {
                return;
            }

            if (img1.GetHistogram().Length == 0) {
                var filename = FileHelper.NameToFileName(img1.Name);
                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    Delete(img1.Id);
                    return;
                }

                using (var matcolors = LabHelper.GetColors(imagedata)) {
                    if (matcolors == null) {
                        Delete(img1.Id);
                        return;
                    }

                    var histogram = LabHelper.GetLab(matcolors, GetCenters());
                    img1.SetHistogram(histogram);
                }
            }

            var history = img1.GetHistory();
            foreach (var id in history) {
                if (!shadowcopy.ContainsKey(id)) { 
                    img1.RemoveFromHistory(id);
                }
            }

            Img img2 = null;
            var bestdistance = 1f;
            foreach (var img in shadowcopy) {
                if (img1.Id == img.Key || img.Value.GetHistogram().Length == 0 || img1.IsInHistory(img.Key)) {
                    continue;
                }

                var distance = LabHelper.GetDistance(img1.GetHistogram(), img.Value.GetHistogram());
                if (img2 == null || distance < bestdistance) {
                    img2 = img.Value;
                    bestdistance = distance;
                }
            }

            if (img1.BestId != img2.Id) {
                var message = $"a:{_added}/f:{_found}/b:{_bad} [{img1.Id}] \u2192 {bestdistance:F2}";
                backgroundworker.ReportProgress(0, message);
                img1.BestId = img2.Id;
            }

            img1.SetLastCheck();
        }

        private static void ImportInternal()
        {
            FileInfo fileinfo;
            lock (_rwlock) {
                if (_rwList.Count == 0) {
                    return;
                }

                var rindex = _random.Next(0, _rwList.Count - 1);
                fileinfo = _rwList.ElementAt(rindex);
                _rwList.RemoveAt(rindex);
            }

            var orgfilename = fileinfo.FullName;
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
            Img imgfound;
            lock (_imglock) {
                found = _hashList.TryGetValue(hash, out imgfound);
            }

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

            float[] histogram;
            using (var matcolors = LabHelper.GetColors(imagedata)) {
                if (matcolors == null) {
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

                histogram = LabHelper.GetLab(matcolors, _centers);
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

            var lc = GetMinLastCheck();
            var id = AllocateId();

            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                histogram: histogram,
                year: year,
                bestid: 0,
                lastview: new DateTime(2020, 1, 1),
                lastcheck: lc,
                history: Array.Empty<int>());

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

        public static void Compute(BackgroundWorker backgroundworker)
        {
            ImportInternal();
            ComputeInternal(backgroundworker);
            ComputeInternal(backgroundworker);
        }   
    }
}