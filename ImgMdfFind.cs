using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static readonly Random _random = new Random();

        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Img imgX;
            var sb = new StringBuilder();
            var method = string.Empty;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
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
                                eX.LastView > eX.LastChanged) {
                                continue;
                            }

                            if (imgX != null &&
                                imgX.PerceptiveDistance < eX.PerceptiveDistance) {
                                continue;
                            }

                            if (imgX != null &&
                                imgX.PerceptiveDistance == eX.PerceptiveDistance &&
                                imgX.OrbDistance < eX.OrbDistance) {
                                continue;
                            }

                            if (imgX != null &&
                                imgX.PerceptiveDistance == eX.PerceptiveDistance &&
                                imgX.OrbDistance == eX.OrbDistance &&
                                imgX.ColorDistance <= eX.ColorDistance) {
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

                var zerocounter = _imgList.Count(e => e.Value.LastView <= e.Value.LastChanged);
                sb.Append($"{zerocounter}/{_imgList.Count}: ");
                sb.Append($"{imgX.Folder}\\{imgX.Name}: ");
                sb.Append($"{method} ");
                sb.Append($"p:{imgX.PerceptiveDistance}/o:{imgX.OrbDistance:F2}/c:{imgX.ColorDistance:F2} ");
            }

            progress.Report(sb.ToString());
        }
    }
}
