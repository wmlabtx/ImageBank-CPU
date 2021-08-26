using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Img imgX;
            var hist = new int[AppConsts.MaxGeneration + 1];
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
                            .Where(e => !e.Value.Hash.Equals(e.Value.NextHash) && _hashList.ContainsKey(e.Value.NextHash))
                            .Select(e => e.Value)
                            .ToArray();

                        if (valid.Length == 0) {
                            progress.Report("No images to view");
                            return;
                        }

                        var mingeneration = valid.Min(e => e.Generation);
                        imgX = valid.Where(e => e.Generation == mingeneration).OrderByDescending(e => e.Sim).FirstOrDefault();
                        if (!_hashList.TryGetValue(imgX.NextHash, out var imgY)) {
                            continue;
                        }

                        nameX = imgX.Name;
                        nameY = imgY.Name;
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

                var changed = _imgList
                    .Where(e => !e.Value.Hash.Equals(e.Value.NextHash) && _hashList.ContainsKey(e.Value.NextHash))
                    .Count(e => e.Value.LastView <= e.Value.LastChanged);

                sb.Append($"{changed}/{_imgList.Count}: ");
                sb.Append($"{imgX.Name}: ");
                sb.Append($"{imgX.Sim:F1} ");
            }

            progress.Report(sb.ToString());
        }

        public static void Find(IProgress<string> progress)
        {
            Find(null, null, progress);
        }
    }
}
