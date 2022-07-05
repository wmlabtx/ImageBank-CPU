using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static int _newcount;
        private static int _oldcount;

        private static void FastFindNext(int idX, IProgress<string> progress) 
        {
            if (!_imgList.TryGetValue(idX, out var img1)) {
                progress?.Report($"({idX}) not found");
                return;
            }

            var ni = img1.GetNi();
            for (var i = 0; i < ni.Length; i++) {
                if (ni[i] != 0) {
                    if (!_imgList.ContainsKey(ni[i])) {
                        img1.RemoveRank(ni[i]);
                    }
                }
            }

            Img img2 = null;
            var bestdistance = 2f;
            var bestmatch = 0;
            foreach (var img in _imgList) {
                if (img.Key != img1.Id && img.Value.GetPalette().Length > 0) {
                    var match = 1;
                    float distance;
                    if (img1.IsRank(img.Key)) {
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

            progress?.Report($"({img1.Id}) {img1.Distance:F2} -> {bestdistance:F2}");
            img1.SetDistance(bestdistance);
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

        public static void Find(int idX, IProgress<string> progress)
        {
            Img imgX = null;
            if (idX != 0) {
                if (!_imgList.TryGetValue(idX, out imgX)) {
                    imgX = null;
                    idX = 0;
                }
            }

            var hist = new SortedList<int, int>();
            foreach (var img in _imgList) {
                var nexts = img.Value.GetNexts();
                if (hist.ContainsKey(nexts)) {
                    hist[nexts]++;
                }
                else {
                    hist.Add(nexts, 1);
                }
            }

            var sb = new StringBuilder();
            for (var i = 0; i <= 10; i++) {
                if (sb.Length > 0) {
                    sb.Append('-');
                }

                var v = 0;
                if (hist.ContainsKey(i)) {
                    v = hist[i];
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
                    var validscope = _imgList.Select(e => e.Value).ToArray();
                    var scope = new List<Img>();
                    var newscope = validscope.Where(e => e.LastView.Year == 2020).ToArray();
                    _newcount = newscope.Length;
                    scope.AddRange(newscope);
                     var take = _newcount == 0 ? 10000 : _newcount;
                    var oldscope = validscope.Where(e => e.LastView.Year > 2020 && e.GetNexts() == 0).OrderBy(e => e.LastView).Take(take).ToArray();
                    var md = oldscope.Min(e => e.LastView);
                    md = md.AddDays(7);
                    oldscope = oldscope.Where(e => e.LastView < md).ToArray();
                    scope.AddRange(oldscope);
                    _oldcount = oldscope.Length;
                    take = _oldcount == 0 ? 10000 : _oldcount;
                    var ratedscope = validscope.Where(e => e.LastView.Year > 2020 && e.GetNexts() != 0).OrderBy(e => e.LastView).Take(take).ToArray();
                    if (ratedscope.Length > 0) {
                        ratedscope = ratedscope.OrderBy(e => e.LastView).Take(oldscope.Length).ToArray();
                        scope.AddRange(ratedscope);
                    }

                    if (imgX == null) {
                        var rindex = _random.Next(0, scope.Count - 1);
                        imgX = scope[rindex];                        
                    }
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

                FindNext(imgX.Id, progress);
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

            progress?.Report($"n:{_newcount}/o:{_oldcount}/{sb}/{totalcount} ({luftcount}) {imgX.Distance:F2}");
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);
    }
}
