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
                            .Where(e => !e.Value.Hash.Equals(e.Value.BestHash) && _hashList.ContainsKey(e.Value.BestHash))
                            .Select(e => e.Value)
                            .ToArray();

                        if (valid.Length == 0) {
                            progress.Report("No images to view");
                            return;
                        }

                        var recent = valid.Where(e => e.LastChanged >= e.LastView).ToArray();
                        imgX = recent.OrderBy(e => e.Distance).FirstOrDefault();
                        if (!_hashList.TryGetValue(imgX.BestHash, out var imgY)) {
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
                    .Where(e => !e.Value.Hash.Equals(e.Value.BestHash) && _hashList.ContainsKey(e.Value.BestHash))
                    .Count(e => e.Value.LastView <= e.Value.LastChanged);

                sb.Append($"{changed}/{_imgList.Count}: ");
                sb.Append($"{imgX.Name}: ");
                sb.Append($"{imgX.Distance:F2} ");
            }

            progress.Report(sb.ToString());
        }

        public static void Find(IProgress<string> progress)
        {
            Find(null, null, progress);
        }
    }
}
