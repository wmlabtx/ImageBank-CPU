using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static int _added;
        private static int _found;
        private static int _bad;

        private static void ComputeInternal(BackgroundWorker backgroundworker)
        {
            Img img1 = null;
            Img[] shadowcopy;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                shadowcopy = _imgList.Select(e => e.Value).OrderBy(e => e.Id).ToArray();
            }

            foreach (var img in shadowcopy) {
                if (img.BestId == 0 || !_imgList.ContainsKey(img.BestId) || img.Fingerprints[0].Length == 0) {
                    img1 = img;
                    break;
                }

                if (img1 == null || img.LastCheck < img1.LastCheck) {
                    img1 = img;
                }
            }

            if (img1 == null) {
                return;
            }

            if (img1.Fingerprints[0].Length == 0) {
                var filename = FileHelper.NameToFileName(img1.Name);
                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    Delete(img1.Id);
                    return;
                }

                var matrix = BitmapHelper.GetMatrix(imagedata);
                if (matrix == null) {
                    Delete(img1.Id);
                    return;
                }

                RootSiftHelper.Compute(matrix, out var descriptors);
                img1.Fingerprints = RootSiftHelper.GetFingerprints(descriptors);
            }

            var candidates = shadowcopy.Where(e => e.Id != img1.Id && e.Fingerprints[0].Length != 0).ToArray();
            if (candidates.Length == 0) {
                img1.BestId = img1.Id;
                img1.BestVDistance = 100f;
                img1.ResetCounter();
            }
            else {
                var bestid = img1.Id;
                var bestvdistance = 100f;
                foreach (var img2 in candidates) {
                    var vdistance = RootSiftHelper.GetDistance(img1.Fingerprints, img2.Fingerprints);
                    if (vdistance < bestvdistance) {
                        bestid = img2.Id;
                        bestvdistance = vdistance;
                    }
                }

                if (bestid != img1.BestId || Math.Abs(img1.BestVDistance - bestvdistance) > 0.0001f) {
                    var message = $"a:{_added}/f:{_found}/b:{_bad} [{img1.Id}] {img1.BestVDistance:F1} \u2192 {bestvdistance:F1}";
                    backgroundworker.ReportProgress(0, message);
                    img1.BestId = bestid;
                    img1.BestVDistance = bestvdistance;
                    img1.ResetCounter();
                }
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

            var matrix = BitmapHelper.GetMatrix(imagedata);
            if (matrix == null) {
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
            
            RootSiftHelper.Compute(matrix, out var descriptors);
            var fingerprints = RootSiftHelper.GetFingerprints(descriptors);

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
                fingerprints: fingerprints,
                year: year,
                counter: 0,
                bestid: 0,
                bestvdistance: 100f,
                lastview: new DateTime(2020, 1, 1),
                lastcheck: lc);

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