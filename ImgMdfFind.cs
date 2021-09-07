using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Forward(IProgress<string> progress)
        {
            var nameX = AppVars.ImgPanel[0].Img.Name;
            AppVars.BestNamesPosition += 10;
            if (AppVars.BestNamesPosition + 10 > AppVars.BestNames.Length) {
                AppVars.BestNamesPosition = 0;
            }

            var nameY = AppVars.BestNames.Substring(AppVars.BestNamesPosition, 10);
            Find(nameX, nameY, progress);
        }

        public static void Backward(IProgress<string> progress)
        {
            var nameX = AppVars.ImgPanel[0].Img.Name;
            AppVars.BestNamesPosition -= 10;
            if (AppVars.BestNamesPosition < 0) {
                AppVars.BestNamesPosition = AppVars.BestNames.Length - 10;
            }

            var nameY = AppVars.BestNames.Substring(AppVars.BestNamesPosition, 10);
            Find(nameX, nameY, progress);
        }

        public static void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Img imgX;
            var sb = new StringBuilder();
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (nameX == null) {
                        imgX = null;
                        var valid = _imgList
                            .Where(e => !string.IsNullOrEmpty(e.Value.BestNames))
                            .Select(e => e.Value)
                            .ToArray();

                        if (valid.Length == 0) {
                            progress.Report("No images to view");
                            return;
                        }

                        var recent = valid.Where(e => e.LastChanged >= e.LastView).ToArray();
                        if (recent.Length == 0) {
                            recent = valid;
                        }

                        imgX = recent.OrderBy(e => e.LastView).FirstOrDefault();
                        nameX = imgX.Name;
                        AppVars.BestNames = imgX.BestNames;
                        AppVars.BestNamesPosition = 0;
                        nameY = imgX.BestNames.Substring(0, 10);
                        if (!_imgList.TryGetValue(nameY, out var imgY)) {
                            continue;
                        }
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(nameX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(nameX);
                        progress.Report($"{nameX} deleted");
                        nameX = null;
                        continue;
                    }

                    imgX = AppVars.ImgPanel[0].Img;
                    AppVars.ImgPanel[1] = GetImgPanel(nameY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(nameY);
                        progress.Report($"{nameY} deleted");
                        nameX = null;
                        continue;
                    }

                    break;
                }

                var changed = _imgList.Count(e => e.Value.LastView <= e.Value.LastChanged);
                sb.Append($"{changed}/{_imgList.Count}: ");
                sb.Append($"{imgX.Name}");
            }

            progress.Report(sb.ToString());
        }

        public static void Find(IProgress<string> progress)
        {
            Find(null, null, progress);
        }
    }
}
