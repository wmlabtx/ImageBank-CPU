using System;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            var sb = new StringBuilder(GetPrompt());
            Img imgX;
            var dtn = DateTime.Now;
            int scopesize;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX))
                    {
                        var scope = _imgList
                            .Where(e =>
                                _imgList.ContainsKey(e.Value.NextName) &&
                                e.Value.GetDescriptors() != null &&
                                e.Value.GetDescriptors().Length > 0 &&
                                !e.Value.Name.Equals(e.Value.NextName))
                            .Select(e => e.Value)
                            .ToArray();

                        if (scope.Length < 2) {
                            progress.Report("No images to view");
                            return;
                        }

                        var mincounter = scope.Min(e => e.Counter);
                        scope = scope.Where(e => e.Counter == mincounter).ToArray();
                        var maxfolder = scope.Max(e => e.Folder);
                        scope = scope.Where(e => e.Folder == maxfolder).ToArray();
                        scopesize = scope.Length;
                        nameX = scope
                            .OrderByDescending(e => e.Sim)
                            .FirstOrDefault()
                            ?.Name;

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
            sb.Append($"({scopesize}) ");
            sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
            sb.Append($"{imgX.Sim:F4} ");
            sb.Append($"({secs:F4}s)");
            progress.Report(sb.ToString());
        }
    }
}
