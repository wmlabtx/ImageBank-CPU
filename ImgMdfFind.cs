using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
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
                            .Where(
                                e => e.Value.BestId != 0 &&
                                e.Value.BestId != e.Value.Id &&
                                _imgList.ContainsKey(e.Value.BestId) &&
                                e.Value.Vector.Length > 0 &&
                                e.Value.Vector[0] != 0)
                            .Select(e => e.Value)
                            .ToArray();
                    }

                    if (valid.Length == 0) {
                        progress.Report("No images to view");
                        return;
                    }

                    /*
                    var cmin = valid.Min(e => e.Counter);
                    valid = valid.Where(e => e.Counter == cmin).ToArray();

                    var realyear = valid.Where(e => e.Year != 0).ToArray();
                    if (realyear.Length > 0) {
                        var lv = realyear.Min(e => e.LastView);
                        imgX = realyear.First(e => e.LastView == lv);
                    }
                    else {
                        var bestpdistancemin = valid.Min(e => e.BestPDistance);
                        if (bestpdistancemin < 80) {
                            imgX = valid.First(e => e.BestPDistance == bestpdistancemin);
                        }
                        else {
                            var bestvdistancemin = valid.Min(e => e.BestVDistance);
                            imgX = valid.First(e => e.BestVDistance == bestvdistancemin);
                        }
                    }
                    */

                    var lvmin = valid.Min(e => e.LastView);
                    imgX = valid.First(e => e.LastView == lvmin);

                    idX = imgX.Id;

                    if (!_imgList.TryGetValue(imgX.BestId, out var imgBest)) {
                        Delete(imgX.BestId);
                        progress.Report($"{imgX.BestId} deleted");
                        idX = 0;
                        continue;
                    }
                }

                AppVars.ImgPanel[0] = GetImgPanel(idX);
                if (AppVars.ImgPanel[0] == null) {
                    Delete(idX);
                    progress.Report($"{idX} deleted");
                    idX = 0;
                    continue;
                }

                imgX = AppVars.ImgPanel[0].Img;
                var idY = imgX.BestId;

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
                var imgcount = _imgList.Count;
                var diff = imgcount - _importLimit;
                var g0 = _imgList.Count(e => e.Value.Counter == 0);
                var g1 = _imgList.Count(e => e.Value.Counter == 1);
                progress.Report($"0:{g0} 1:{g1} images:{imgcount}({diff}) distance:{imgX.BestPDistance} ({imgX.BestVDistance:F2})");
            }
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);
    }
}
