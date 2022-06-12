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
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }
            }

            var img1Key = 0;
            var img1LastCheck = DateTime.Now;
            lock (_imglock) {
                foreach (var img in _imgList) {
                    if (img.Value.GetPalette().Length == 0 || !_imgList.ContainsKey(img.Value.BestId)) {
                        img1Key = img.Key;
                        img1LastCheck = img.Value.LastCheck;
                        break;
                    }

                    if (img1Key == 0 || img.Value.LastCheck < img1LastCheck) {
                        img1Key = img.Key;
                        img1LastCheck = img.Value.LastCheck;
                    }
                }
            }

            if (img1Key == 0) {
                return;
            }

            string name;
            lock (_imglock) {
                if (!_imgList.TryGetValue(img1Key, out var img1)) {
                    return;
                }

                name = img1.Name;
            }

            var filename = FileHelper.NameToFileName(name);
            var imagedata = FileHelper.ReadData(filename);
            if (imagedata == null) {
                Delete(img1Key);
                return;
            }

            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
                    Delete(img1Key);
                    return;
                }

                var newpalette = ComputePalette(bitmap);
                lock (_imglock) {
                    if (!_imgList.TryGetValue(img1Key, out var img1)) {
                        return;
                    }

                    img1.SetPalette(newpalette);
                }
            }

            int[] history;
            float[] palette;
            List<Tuple<int, float[]>> shadow = new List<Tuple<int, float[]>>();
            lock (_imglock) {
                if (!_imgList.TryGetValue(img1Key, out var img1)) {
                    return;
                }

                history = (int[])img1.GetHistory().Clone();
                foreach (var id in history) {
                    if (!_imgList.ContainsKey(id)) {
                        img1.RemoveFromHistory(id);
                    }
                }

                palette = (float[])img1.GetPalette().Clone();
                foreach (var img in _imgList) {
                    shadow.Add(new Tuple<int, float[]>(img.Key, (float[])img.Value.GetPalette().Clone()));
                }
            }

            var bestid = 0;
            var bestdistance = 2f;
            foreach (var e in shadow) {
                if (img1Key == e.Item1 || e.Item2.Length == 0 || history.Contains(e.Item1)) {
                    continue;
                }

                var distance = GetDistance(palette, e.Item2);
                if (bestid == 0 || distance < bestdistance) {
                    bestid = e.Item1;
                    bestdistance = distance;
                }
            }

            lock (_imglock) {
                if (!_imgList.TryGetValue(img1Key, out var img1)) {
                    return;
                }

                if (img1.BestId != bestid) {
                    var message = $"a:{_added}/f:{_found}/b:{_bad} [{img1.Id}] {img1.Distance:F2} \u2192 {bestdistance:F2}";
                    backgroundworker.ReportProgress(0, message);
                    img1.SetBestId(bestid);
                }

                img1.SetDistance(bestdistance);
                img1.SetLastCheck();
            }
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

            var lc = GetMinLastCheck();
            var id = AllocateId();

            var nimg = new Img(
                id: id,
                name: newname,
                hash: hash,
                palette: palette,
                distance: 2f,
                sceneid: 0,
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