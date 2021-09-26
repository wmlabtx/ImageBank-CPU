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
                                _imgList.ContainsKey(e.Value.BestId))
                            .Select(e => e.Value)
                            .ToArray();
                    }

                    if (valid.Length == 0) {
                        progress.Report("No images to view");
                        return;
                    }

                    var hcmin = valid.Min(e => e.History.Count);
                    valid = valid.Where(e => e.History.Count == hcmin).ToArray();

                    var realyear = valid.Where(e => e.Year != 0).ToArray();
                    if (realyear.Length > 0) {
                        var lv = realyear.Min(e => e.LastView);
                        imgX = realyear.First(e => e.LastView == lv);
                    }
                    else {
                        var bestdistancemin = valid.Min(e => e.BestDistance);
                        imgX = valid.First(e => e.BestDistance == bestdistancemin);
                    }
                    
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
                var g0 = _imgList.Count(e => e.Value.History.Count == 0);
                progress.Report($"0:{g0} images:{imgcount}({diff}) distance:{imgX.BestDistance}");
            }
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);
    }
}
