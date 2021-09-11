using OpenCvSharp;
using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static int FindIdYx(Img imgX, Img[] collection, IProgress<string> progress)
        {
            if (collection.Length == 0) {
                return 0;
            }

            var mindistance = double.MaxValue;
            var minid = -1;

            foreach (var img in collection) {
                var distance = ImageHelper.GetDistance(imgX.ColorHistogram, img.ColorHistogram);
                if (distance < mindistance) {
                    mindistance = distance;
                    minid = img.Id;
                    progress.Report($"Searching candidates ({minid}, {mindistance})...");
                }
            }

            return minid;
        }

        private static int FindIdY0(Img imgX, IProgress<string> progress)
        {
            Img[] collection;
            lock (_imglock) {
                collection = _imgList
                    .Where(e => e.Key != imgX.Id && e.Value.Family > 0 && !imgX.History.ContainsKey(e.Value.Family))
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (collection.Length == 0) {
                return 0;
            }

            return FindIdYx(imgX, collection, progress);
        }

        private static int FindIdY1(Img imgX, IProgress<string> progress)
        {
            Img[] collection;
            lock (_imglock) {
                collection = _imgList
                    .Where(e => e.Key != imgX.Id && e.Value.Family == imgX.Family && !imgX.History.ContainsKey(e.Value.Id))
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (collection.Length == 0) {
                return 0;
            }

            return FindIdYx(imgX, collection, progress);
        }

        private static int FindIdY(Img imgX, IProgress<string> progress)
        {
            return imgX.Family == 0 ? FindIdY0(imgX, progress) : FindIdY1(imgX, progress);
        }

        private static int FindOutOfFamilyIdY(Img imgX, IProgress<string> progress)
        {
            Img[] collection;
            lock (_imglock) {
                collection = _imgList
                    .Where(e => e.Key != imgX.Id && e.Value.Family != imgX.Family)
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (collection.Length == 0) {
                return 0;
            }

            return FindIdYx(imgX, collection, progress);
        }

        public static void Find(int idX, IProgress<string> progress)
        {
            Img imgX = null;
            do {
                lock (_imglock) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }
                }

                if (idX == 0) {
                    imgX = null;
                    Img[] valid;
                    lock (_imglock) {
                        valid = _imgList
                            .Select(e => e.Value)
                            .ToArray();
                    }

                    if (valid.Length == 0) {
                        progress.Report("No images to view");
                        return;
                    }

                    var minlastview = valid.Min(e => e.LastView);
                    imgX = valid.FirstOrDefault(e => e.LastView == minlastview);
                    idX = imgX.Id;
                }

                AppVars.ImgPanel[0] = GetImgPanel(idX);
                if (AppVars.ImgPanel[0] == null) {
                    Delete(idX);
                    progress.Report($"{idX} deleted");
                    idX = 0;
                    continue;
                }

                imgX = AppVars.ImgPanel[0].Img;
                var idY = FindIdY(imgX, progress);
                if (idY == 0) {
                    if (imgX.Family == 0) {
                        imgX.Family = AllocateFamily();
                        imgX.History.Clear();
                        imgX.SaveHistory();
                        imgX.LastView = DateTime.Now;
                        idX = 0;
                        continue;
                    }
                    else {
                        idY = FindOutOfFamilyIdY(imgX, progress);
                    }
                }

                AppVars.ImgPanel[1] = GetImgPanel(idY);
                if (AppVars.ImgPanel[1] == null) {
                    Delete(idY);
                    progress.Report($"{idY} deleted");
                    idX = 0;
                    continue;
                }

                break;
            }
            while (true);

            lock (_imglock) {
                var imgtoview = _imgList.Count(e => e.Value.Family == 0);
                var families = _imgList.Select(e => e.Value.Family).Distinct().Count();
                var imgcount = _imgList.Count;
                progress.Report($"oof:{imgtoview}/fs:{families}/imgs:{imgcount}");
            }
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);
    }
}
