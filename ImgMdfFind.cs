using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
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

                            if (imgX != null &&
                                imgX.Counter < eX.Counter)
                            {
                                continue;
                            }

                            if (imgX != null &&
                                imgX.Counter == eX.Counter &&
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

                    /*
                    if (imgX.Distance < 0.1)
                    {
                        if (AppVars.ImgPanel[0].Length <= AppVars.ImgPanel[1].Length)
                        {
                            Delete(nameY);
                            progress.Report($"{nameY} deleted");
                            nameX = string.Empty;
                            continue;
                        }
                        else
                        {
                            Delete(nameX);
                            progress.Report($"{nameX} deleted");
                            nameX = string.Empty;
                            continue;
                        }
                    }
                    */

                    var folderX = AppVars.ImgPanel[0].Img.Folder;
                    var folderY = AppVars.ImgPanel[1].Img.Folder;
                    if (folderX != folderY) {
                        var dimX = AppVars.ImgPanel[0].Bitmap.Width * AppVars.ImgPanel[0].Bitmap.Height;
                        var dimY = AppVars.ImgPanel[1].Bitmap.Width * AppVars.ImgPanel[1].Bitmap.Height;
                        if ((folderX < folderY && dimX > dimY) ||
                            (folderX > folderY && dimX < dimY))
                        {
                            AppVars.ImgPanel[0].Img.Folder = folderY;
                            AppVars.ImgPanel[1].Img.Folder = folderX;
                        }
                    }

                    break;
                }

                var mincounter = _imgList.Min(e => e.Value.Counter);
                var z = _imgList.Count(e => e.Value.Width == 0);
                var scope = _imgList.Where(e => e.Value.Counter == mincounter).ToArray();
                var mindistance = scope.Min(e => (int)(e.Value.Distance * 10f));
                scope = scope.Where(e => (int)(e.Value.Distance * 10f) == mindistance).ToArray();
                sb.Append($"{mindistance / 10f:F1}:{scope.Length}/{z}/{_imgList.Count}: ");
                sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
                sb.Append($"{imgX.Distance:F1} ");

                var moves = 0;
                var movemessage = string.Empty;
                var c = new int[100];
                foreach (var e in _imgList)
                {
                    c[e.Value.Folder]++;
                }

                for (var df = 2; df <= 99; df++)
                {
                    if (c[df - 1] < AppConsts.MaxImagesInFolder && c[df] > 0)
                    {
                        var img = _imgList
                            .Where(e => e.Value.Folder == df)
                            .OrderBy(e => e.Value.LastAdded)
                            .FirstOrDefault()
                            .Value;

                        c[df - 1]++;
                        c[df]--;
                        img.Folder = df - 1;
                        moves++;
                        movemessage = $"{df}[{c[df]}] {char.ConvertFromUtf32(0x2192)} {df - 1}[{c[df - 1]}] ";
                        
                    }
                }

                if (moves > 0) {
                    sb.Append($"{movemessage} ({moves} moves)");
                }
            }

            progress.Report(sb.ToString());
        }
    }
}
