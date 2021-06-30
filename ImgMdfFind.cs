using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Find(string filenameX, string filenameY, IProgress<string> progress)
        {
            Img imgX;
            var sb = new StringBuilder();
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (filenameX == null) {
                         imgX = null;
                        var valid = _imgList
                            .Where(e => !e.Value.Hash.Equals(e.Value.NextHash) && _hashList.ContainsKey(e.Value.NextHash))
                            .Select(e => e.Value)
                            .ToArray();

                        if (valid.Length == 0) {
                            progress.Report("No images to view");
                            return;
                        }

                        var scope = valid
                            .Where(e => e.LastView < e.LastChanged)
                            .ToArray();

                        if (scope.Length > 0) {
                            var mincounter = valid.Min(e => e.Counter);
                            scope = scope
                                .Where(e => e.Counter == mincounter)
                                .ToArray();
                        }

                        imgX = scope.Length > 0 ?
                            scope.OrderBy(e => e.Size).FirstOrDefault() :
                            valid.OrderBy(e => e.LastView).FirstOrDefault();


                        /*
                        var mincounter = valid.Min(e => e.Counter);
                        var scope = valid
                            .Where(e => e.Counter == mincounter)
                            .ToArray();

                        imgX = mincounter == 0 ?
                            scope.OrderBy(e => e.Size).FirstOrDefault() :
                            scope.OrderBy(e => e.LastView).FirstOrDefault();
                        */

                        if (!_hashList.TryGetValue(imgX.NextHash, out var imgY)) {
                            continue;
                        }

                        filenameX = imgX.FileName;
                        filenameY = imgY.FileName;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(filenameX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(filenameX);
                        progress.Report($"{filenameX} deleted");
                        filenameX = null;
                        continue;
                    }

                    imgX = AppVars.ImgPanel[0].Img;
                    AppVars.ImgPanel[1] = GetImgPanel(filenameY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(filenameY);
                        progress.Report($"{filenameY} deleted");
                        filenameX = null;
                        continue;
                    }

                    break;
                }

                var zerocounter = _imgList.Count(e => e.Value.Counter == 0);
                if (zerocounter == 0) {
                    zerocounter = _imgList.Count(e => e.Value.LastView <= e.Value.LastChanged);
                }

                sb.Append($"{zerocounter}/{_imgList.Count}: ");
                sb.Append($"{imgX.FileName}: ");
                sb.Append($"{imgX.KazeMatch} ");
            }

            progress.Report(sb.ToString());
        }

        public static void Find(IProgress<string> progress)
        {
            Find(null, null, progress);
        }
    }
}
