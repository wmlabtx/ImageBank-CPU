using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private readonly Random _random = new Random();

        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Img imgX;
            var sb = new StringBuilder();
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        var r = _random.Next(20);
                        imgX = null;
                        foreach (var e in _imgList) {
                            var eX = e.Value;
                            if (eX.Hash.Equals(eX.NextHash)) {
                                continue;
                            }

                            if (!_hashList.TryGetValue(eX.NextHash, out var eY)) {
                                continue;
                            }

                            if (imgX != null &&
                                imgX.Counter < eX.Counter) {
                                continue;
                            }

                            if (r == 0 &&
                                imgX != null &&
                                imgX.Counter == eX.Counter &&
                                imgX.ColorDistance <= eX.ColorDistance) {
                                continue;
                            }

                            if (r == 1 &&
                                imgX != null &&
                                imgX.Counter == eX.Counter &&
                                imgX.OrbDistance <= eX.OrbDistance) {
                                continue;
                            }

                            if (r == 2 &&
                                imgX != null &&
                                imgX.Counter == eX.Counter &&
                                imgX.PerceptiveDistance <= eX.PerceptiveDistance) {
                                continue;
                            }

                            if (r >= 3 &&
                                imgX != null &&
                                imgX.Counter == eX.Counter &&
                                imgX.LastView <= eX.LastView) {
                                continue;
                            }

                            imgX = eX;
                            var imgY = eY;
                            nameX = imgX.Name;
                            nameY = imgY.Name;
                        }
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        progress.Report("No images to view");
                        return;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(nameX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(nameX);
                        progress.Report($"{nameX} deleted");
                        nameX = string.Empty;
                        continue;
                    }

                    imgX = AppVars.ImgPanel[0].Img;
                    AppVars.ImgPanel[1] = GetImgPanel(nameY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(nameY);
                        progress.Report($"{nameY} deleted");
                        nameX = string.Empty;
                        continue;
                    }

                    break;
                }

                var mincounter = _imgList.Min(e => e.Value.Counter);
                var newpics = _imgList.Count(e => e.Value.Counter == mincounter);
                sb.Append($"{newpics}/{_imgList.Count}: ");
                sb.Append($"{imgX.Folder}\\{imgX.Name}: ");
                sb.Append($"p:{imgX.PerceptiveDistance}/o:{imgX.OrbDistance}/c:{imgX.ColorDistance:F2} ");
            }

            progress.Report(sb.ToString());
        }
    }
}
