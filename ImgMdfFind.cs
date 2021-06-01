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
                        var valid = _imgList
                            .Where(e => !e.Value.Hash.Equals(e.Value.NextHash))
                            .Select(e => e.Value)
                            .ToArray();

                        if (valid.Length == 0) {
                            progress.Report("No images to view");
                            return;
                        }

                        var scope = valid.Where(e => e.LastView.Year <= 2020).ToArray();
                        if (scope.Length > 0) {
                            imgX = scope.OrderByDescending(e => e.AkazePairs).FirstOrDefault();
                        }
                        else {
                            scope = valid.Where(e => e.LastChanged >= e.LastView).ToArray();
                            if (scope.Length > 0) {
                                imgX = scope.OrderByDescending(e => e.AkazePairs).FirstOrDefault();
                            }
                            else {
                                imgX = valid.OrderBy(e => e.LastView).FirstOrDefault();
                            }
                        }

                        if (!_hashList.TryGetValue(imgX.NextHash, out var imgY)) {
                            continue;
                        }

                        idX = imgX.Id;
                        idY = imgY.Id;
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
