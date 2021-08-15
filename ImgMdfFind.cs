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
                            .Where(e => !e.Value.Hash.Equals(e.Value.NextHash) && _hashList.ContainsKey(e.Value.NextHash) && e.Value.Node[0] != 0 && e.Value.Node[1] != 0)
                            .Select(e => e.Value)
                            .ToArray();

                        if (valid.Length == 0) {
                            progress.Report("No images to view");
                            return;
                        }

                        /*
                        imgX = valid.OrderBy(e => e.LastView).FirstOrDefault();
                        */
                        
                        foreach (var e in valid) {
                            hist[e.Generation]++;
                        }

                        var ig = 0;
                        for (var i = hist.Length - 1; i >= 0; i--) {
                            if (hist[i] > hist[ig]) {
                                ig = i;
                            }
                        }

                        imgX = valid.Where(e => e.Generation == ig).OrderByDescending(e => e.Sim) .FirstOrDefault();

                        //imgX = valid.Where(e => e.Generation == ig).OrderByDescending(e => e.Sim).FirstOrDefault();

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

                for (var i = 0; i < hist.Length; i++) {
                    sb.Append($"{i}:{hist[i]}/");
                }

                var changed = _imgList
                    .Where(e => !e.Value.Hash.Equals(e.Value.NextHash) && _hashList.ContainsKey(e.Value.NextHash))
                    .Count( e => e.Value.LastView <= e.Value.LastChanged);

                sb.Append($"c:{changed}/{_imgList.Count}: ");
                sb.Append($"{imgX.Name}: ");
                sb.Append($"{imgX.Sim:F2} ");
            }

            progress.Report(sb.ToString());
        }

        public static void Find(IProgress<string> progress)
        {
            Find(null, null, progress);
        }
    }
}
