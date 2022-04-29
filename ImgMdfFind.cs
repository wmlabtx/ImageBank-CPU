using System;
using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Find(int idX, IProgress<string> progress)
        {
            Img imgX;
            do {
                lock (_imglock) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }
                }

                if (idX == 0) {
                    imgX = null;
                    var maxdays = 0.0;
                    var now = DateTime.Now;
                    lock (_imglock) {
                        foreach (var img in _imgList.Values) {
                            if (img.BestId == 0 || img.BestId == img.Id) {
                                continue;
                            }

                            if (!_imgList.TryGetValue(img.BestId, out var imgnext)) {
                                continue;
                            }

                            if (imgX != null && imgX.LastView.Year == 2020 && img.LastView.Year != 2020) {
                                continue;
                            }

                            var a = now.Subtract(img.LastView).TotalDays;
                            var b = now.Subtract(imgnext.LastView).TotalDays;
                            var days = (a * a) + (b * b);
                            if ((imgX != null && imgX.LastView.Year != 2020 && img.LastView.Year == 2020) || (days > maxdays)) {
                                imgX = img;
                                maxdays = days;
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

            lock (_imglock) {
                var imgcount = _imgList.Count;
                var diff = imgcount - _importLimit;
                var now = DateTime.Now;
                var array = _imgList.Select(e => now.Subtract(e.Value.LastView).TotalDays).OrderByDescending(e => e).ToArray();
                var pindex = array.Length / 100;
                var th = array[pindex];
                var countmaxdays = array.Count(e => e > th);
                progress.Report($"{th:F1}d:{countmaxdays} images:{imgcount}({diff}) distance: {imgX.BestVDistance:F1}");
            }
        }

        public static void Find(IProgress<string> progress) => Find(0, progress);
    }
}
