using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private int _flagfind;

        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            var sb = new StringBuilder(GetPrompt());
            Img imgX;
            var dtn = DateTime.Now;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX))
                    {
                        var imglist = _imgList
                            .Where(e => _imgList.ContainsKey(e.Value.NextName))
                            .Where(e => e.Value.GetDescriptors() != null && e.Value.GetDescriptors().Length > 0)
                            .Select(e => e.Value)
                            .ToArray();

                        if (imglist.Length < 2)
                        {
                            progress.Report("No images to view");
                            return;
                        }

                        if (_flagfind == 0) {
                            nameX = imglist
                                .OrderByDescending(e => e.LastAdded)
                                .FirstOrDefault(e => e.Counter == 0 && !e.NextName.Equals(e.Name))
                                ?.Name;

                        }
                        else {
                            nameX = imglist
                                .OrderByDescending(e => e.Sim)
                                .FirstOrDefault(e => e.Counter == 0 && !e.NextName.Equals(e.Name))
                                ?.Name;
                        }

                        _flagfind = 1 - _flagfind;

                        AppVars.ImgPanel[0] = GetImgPanel(nameX);
                        if (AppVars.ImgPanel[0] == null) {
                            Delete(nameX);
                            progress.Report($"{nameX} deleted");
                            nameX = string.Empty;
                            continue;
                        }

                        imgX = AppVars.ImgPanel[0].Img;
                        if (string.IsNullOrEmpty(nameY)) {
                            nameY = imgX.NextName;
                        }

                        AppVars.ImgPanel[1] = GetImgPanel(nameY);
                        if (AppVars.ImgPanel[1] == null) {
                            Delete(nameY);
                            nameY = string.Empty;
                            progress.Report($"{nameY} deleted");
                            nameX = string.Empty;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(nameX)) {
                            break;
                        }
                    }
                }
            }

            var secs = DateTime.Now.Subtract(dtn).TotalSeconds;
            sb.Append($"{AppVars.MoveMessage} ");
            imgX = AppVars.ImgPanel[0].Img;
            sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
            sb.Append($"{imgX.Sim:F4} ");
            sb.Append($"({secs:F4}s)");
            progress.Report(sb.ToString());
        }
    }
}
