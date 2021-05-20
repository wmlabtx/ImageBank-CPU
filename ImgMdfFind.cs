using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Find(int idX, int idY, IProgress<string> progress)
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

                    if (idX == 0) {
                        imgX = null;
                        foreach (var e in _imgList) {
                            var eX = e.Value;
                            if (eX.Hash.Equals(eX.NextHash)) {
                                continue;
                            }

                            if (!_hashList.TryGetValue(eX.NextHash, out var eY)) {
                                continue;
                            }

                            if (eX.LastChanged < eX.LastView) {
                                continue;
                            }

                            if (imgX == null ||
                                imgX.Counter > eX.Counter ||
                                (imgX.Counter == eX.Counter && imgX.AkazePairs < eX.AkazePairs)) {
                                imgX = eX;
                                idX = eX.Id;
                                idY = eY.Id;
                            }
                        }
                    }

                    if (idX == 0) {
                        progress.Report("No images to view");
                        return;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(idX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(idX);
                        progress.Report($"{idX} deleted");
                        idX = 0;
                        continue;
                    }

                    imgX = AppVars.ImgPanel[0].Img;
                    AppVars.ImgPanel[1] = GetImgPanel(idY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(idY);
                        progress.Report($"{idY} deleted");
                        idX = 0;
                        continue;
                    }

                    break;
                }

                var zerocounter = _imgList.Count(e => e.Value.LastView.Year == 2020);
                if (zerocounter == 0) {
                    zerocounter = _imgList.Count(e => e.Value.LastView <= e.Value.LastChanged);
                }

                sb.Append($"{zerocounter}/{_imgList.Count}: ");
                sb.Append($"{imgX.Folder:D2}\\{imgX.Id:D6}: ");
                sb.Append($"{method} ");
                sb.Append($"a:{imgX.AkazePairs} ");
            }

            progress.Report(sb.ToString());
        }
    }
}
