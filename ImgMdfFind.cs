using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static void FastFindNext(int idX, IProgress<string> progress, string prefix = "") 
        {
            if (!_imgList.TryGetValue(idX, out var img1)) {
                progress?.Report($"({idX}) not found");
                return;
            }

            var ni = img1.GetHistory();
            for (var i = 0; i < ni.Length; i++) {
                if (!_imgList.ContainsKey(ni[i])) {
                    img1.RemoveRank(ni[i]);
                }
            }

            Img img2 = null;
            var bestdistance = 2f;
            var bestmatch = 0;
            foreach (var img in _imgList) {
                if (img.Key != img1.Id && img.Value.GetPalette().Length > 0) {
                    var match = 1;
                    float distance;
                    if (img1.InHistory(img.Key)) {
                        match = 0;
                    }

                    if (img2 == null || match > bestmatch) {
                        distance = GetDistance(img1.GetPalette(), img.Value.GetPalette());
                        img2 = img.Value;
                        bestmatch = match;
                        bestdistance = distance;
                    }
                    else {
                        if (match == bestmatch) {
                            distance = GetDistance(img1.GetPalette(), img.Value.GetPalette());
                            if (distance < bestdistance) {
                                img2 = img.Value;
                                bestmatch = match;
                                bestdistance = distance;
                            }
                        }
                    }
                }
            }

            if (img1.BestId != img2.Id) {                
                img1.SetBestId(img2.Id);
            }

            progress?.Report($"{prefix}({img1.Id}) {img1.Distance:F2} -> {bestdistance:F2}");
            img1.SetDistance(bestdistance);

            ni = img2.GetHistory();
            for (var i = 0; i < ni.Length; i++) {
                if (!_imgList.ContainsKey(ni[i])) {
                    img2.RemoveRank(ni[i]);
                }
            }
        }

        private static void FindNext(int idX, IProgress<string> progress)
        {
            if (!_imgList.TryGetValue(idX, out var img1)) {
                progress?.Report($"({idX}) not found");
                return;
            }

            progress?.Report($"({idX}) getting palette...");
            var name = img1.Name;
            var filename = FileHelper.NameToFileName(name);
            var imagedata = FileHelper.ReadData(filename);
            if (imagedata == null) {
                progress?.Report($"({idX}) removed");
                Delete(img1.Id);
                return;
            }

            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
                    progress?.Report($"({idX}) removed");
                    Delete(img1.Id);
                    return;
                }

                var newpalette = ComputePalette(bitmap);
                img1.SetPalette(newpalette);
            }

            FastFindNext(img1.Id, progress);
        }

        public static void MoveBackward(IProgress<string> progress)
        {
            Img imgX = null;
            if (_imgList.Count < 2) {
                progress?.Report("No images to view");
                return;
            }

            var lv = _imgList.Max(e => e.Value.LastView);
            imgX = _imgList.First(e => e.Value.LastView == lv).Value;

            var idX = imgX.Id;
            AppVars.ImgPanel[0] = GetImgPanel(idX);
            if (AppVars.ImgPanel[0] == null) {
                Delete(idX);
                progress?.Report($"{idX} deleted");
                idX = 0;
                return;
            }

            imgX = AppVars.ImgPanel[0].Img;

            var idY = imgX.BestId;
            AppVars.ImgPanel[1] = GetImgPanel(idY);
            if (AppVars.ImgPanel[1] == null) {
                Delete(idY);
                progress?.Report($"{idY} deleted");
                FindNext(imgX.Id, progress);
                idY = imgX.BestId;
                AppVars.ImgPanel[1] = GetImgPanel(idY);
            }

            progress?.Report($"{imgX.Distance:F2}");
        }

        public static void FixErrors(IProgress<string> progress)
        {
            var errors = new List<Img>();
            foreach (var img in _imgList.Values) {
                if (img.Id == img.BestId || !_imgList.ContainsKey(img.BestId) || img.InHistory(img.BestId)) {
                    errors.Add(img);
                }
            }

            var counter = 1;
            foreach (var img in errors) {
                FastFindNext(img.Id, progress, $"{counter}/{errors.Count} ");
                counter++;
            }
        }

        private static void UpdatePalette(Img img1, IProgress<string> progress)
        {
            progress?.Report($"({img1.Id}) getting palette...");
            var name = img1.Name;
            var filename = FileHelper.NameToFileName(name);
            var imagedata = FileHelper.ReadData(filename);
            if (imagedata == null) {
                progress?.Report($"({img1.Id}) removed");
                Delete(img1.Id);
                return;
            }

            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
                    progress?.Report($"({img1.Id}) removed");
                    Delete(img1.Id);
                    return;
                }

                var newpalette = ComputePalette(bitmap);
                img1.SetPalette(newpalette);
            }
        }

        public static void Find(int idX, IProgress<string> progress)
        {
            FixErrors(progress);

            Img imgX = null;
            if (idX != 0) {
                if (!_imgList.TryGetValue(idX, out imgX)) {
                    imgX = null;
                    idX = 0;
                }
            }

            var bins = new SortedList<int, int>();
            foreach (var img in _imgList.Values) {
                var bin = img.LastView.Year <= 2020 ?
                    -1 :
                    img.GetHistorySize();

                if (bins.ContainsKey(bin)) {
                    bins[bin]++;
                }
                else {
                    bins.Add(bin, 1);
                }
            }

            var sb = new StringBuilder();
            for (var i = -1; i <= 10; i++) {
                if (sb.Length > 0) {
                    sb.Append('-');
                }

                var v = 0;
                if (bins.ContainsKey(i)) {
                    v = bins[i];
                }

                sb.Append($"{v}");
            }

            int totalcount;
            int luftcount;
            do {
                if (_imgList.Count < 2) {
                    progress?.Report("No images to view");
                    return;
                }

                totalcount = _imgList.Count;
                luftcount = totalcount - _importLimit;

                if (idX == 0) {                    
                    var mscope = _imgList.Values.OrderBy(e => e.LastView).Take(10000).ToArray();
                    var randpos = _random.IRandom(0, mscope.Length - 1);
                    imgX = mscope[randpos];
                }

                if (imgX == null) {
                    progress?.Report("No images to view");
                    return;
                }

                idX = imgX.Id;
                AppVars.ImgPanel[0] = GetImgPanel(idX);
                if (AppVars.ImgPanel[0] == null) {
                    Delete(idX);
                    progress?.Report($"{idX} deleted");
                    idX = 0;
                    continue;
                }

                imgX = AppVars.ImgPanel[0].Img;
                UpdatePalette(imgX, progress);
                _lastviewed.Add(imgX.LastView);
                while (_lastviewed.Count > SIMMAX) {
                    _lastviewed.RemoveAt(0);
                }

                var idY = imgX.BestId;
                AppVars.ImgPanel[1] = GetImgPanel(idY);
                if (AppVars.ImgPanel[1] == null) {
                    Delete(idY);
                    progress?.Report($"{idY} deleted");
                    idX = 0;
                    continue;
                }

                break;
            }
            while (true);

            progress?.Report($"{sb}/{totalcount} ({luftcount}) {imgX.Distance:F2}");
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);
    }
}
