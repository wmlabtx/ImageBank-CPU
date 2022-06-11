using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Find(int idX, IProgress<string> progress)
        {
            Img imgX = null;
            int totalcount;
            int luftcount;
            int zerocount;
            do {
                lock (_imglock) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    totalcount = _imgList.Count;
                    luftcount = totalcount - _importLimit;
                    zerocount = _imgList.Count(e => e.Value.Counter == 0);
                    var validscope = _imgList.Where(e => e.Key != e.Value.BestId && _imgList.ContainsKey(e.Value.BestId)).Select(e => e.Value).ToArray();
                    var minc = validscope.Min(e => e.Counter);
                    validscope = validscope.Where(e => e.Counter == minc).ToArray();
                    var scope = new List<Img>();
                    var newscope = validscope.Where(e => e.LastView.Year == 2020).Take(1000).ToList();
                    scope.AddRange(newscope);
                    var more = newscope.Count > 0 ? 100 : 500;
                    var similarscope = validscope.OrderBy(e => e.Distance).Take(more).ToList();
                    scope.AddRange(similarscope);
                    var oldestscope = validscope.Where(e => e.LastView.Year > 2020).OrderBy(e => e.LastView).Take(more).ToList();
                    scope.AddRange(oldestscope);

                    if (idX == 0) {
                        imgX = null;
                        if (_lastviewed.Count == 0) {
                            var minlv = scope.Min(e => e.LastView);
                            imgX = scope.FirstOrDefault(e => e.LastView == minlv);
                        }
                        else {
                            var maxd = 0f;
                            foreach (var img in scope) {
                                var mind = float.MaxValue;
                                foreach (var limg in _lastviewed) {
                                    var distance = GetDistance(img.GetPalette(), limg);
                                    if (distance < mind) {
                                        mind = distance;
                                    }
                                }

                                if (mind > maxd) {
                                    maxd = mind;
                                    imgX = img;
                                }
                            }
                        }
                    }

                    if (imgX == null) {
                        progress.Report("No images to view");
                        return;
                    }

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

            progress.Report($"0:{zerocount}/{totalcount} ({luftcount}) {imgX.Distance:F2}");
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);
    }
}
