using System;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Img imgX;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX))
                    {
                        imgX = null;
                        foreach (var e in _imgList) {
                            var eX = e.Value;
                            if (eX.Hash.Equals(eX.NextHash)) {
                                continue;
                            }

                            if (!_hashList.TryGetValue(eX.NextHash, out var eY)) {
                                continue;
                            }

                            if (eX.Counter == 0)
                            {
                                continue;
                            }

                            /*
                            if (imgX != null &&
                                imgX.Counter < eX.Counter) {
                                continue;
                            }

                            if (imgX != null &&
                                imgX.Counter == eX.Counter &&
                                imgX.LastView <= eX.LastView) {
                                continue;
                            }
                            */

                            if (imgX != null &&
                                imgX.LastView <= eX.LastView)
                            {
                                continue;
                            }

                            imgX = eX;
                            var imgY = eY;
                            nameX = imgX.Name;
                            nameY = imgY.Name;
                        }
                    }

                    if (string.IsNullOrEmpty(nameX))
                    {
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
            }

            var sb = new StringBuilder();
            sb.Append($"{_imgList.Count}: ");
            sb.Append($"{imgX.Folder}\\{imgX.Name}: ");
            sb.Append($"{imgX.Distance:F2} ");
            progress.Report(sb.ToString());
        }
    }
}
