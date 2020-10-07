using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private int _offset = 1;

        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            var sb = new StringBuilder(GetPrompt());
            Img imgX;
            var dtn = DateTime.Now;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        var imglist = _imgList
                            .Where(e => _imgList.ContainsKey(e.Value.NextName))
                            .Where(e => e.Value.Descriptors != null && e.Value.Descriptors.Length > 0)
                            .OrderBy(e => e.Value.LastView)
                            .Select(e => e.Value)
                            .ToArray();

                        if (imglist.Length < 2) {
                            progress.Report("No images to view");
                            return;
                        }

                        var mincounter = imglist.Min(e => e.Counter);
                        imglist = imglist
                            .Where(e => e.Counter == mincounter)
                            .ToArray();

                        if (_offset - 1 >= imglist.Length) {
                            _offset = 1;
                            nameX = imglist
                                .OrderBy(e => e.Distance)
                                .FirstOrDefault()
                                .Name;
                        }

                        if (string.IsNullOrEmpty(nameX)) {
                            nameX = imglist[_offset - 1].Name;
                            _offset *= 10;
                        }

                        AppVars.ImgPanel[0] = GetImgPanel(nameX);
                        if (AppVars.ImgPanel[0] == null) {
                            Delete(nameX);
                            progress.Report($"{nameX} deleted");
                            nameX = string.Empty;
                            continue;
                        }

                        while (true) {
                            imgX = AppVars.ImgPanel[0].Img;
                            nameY = imgX.NextName;
                            AppVars.ImgPanel[1] = GetImgPanel(nameY);
                            if (AppVars.ImgPanel[1] == null) {
                                Delete(nameY);
                                progress.Report($"{nameY} deleted");
                                continue;
                            }

                            break;
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
            sb.Append($"{imgX.Distance:F4} ");
            sb.Append($"({secs:F4}s)");
            progress.Report(sb.ToString());
        }
    }
}
