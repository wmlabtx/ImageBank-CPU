using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            Img imgX;
            var dtn = DateTime.Now;
            int bigdescriptors;
            lock (_imglock) {
                bigdescriptors = _imgList.Count(e => e.Value.GetDescriptors() != null && e.Value.GetDescriptors().Height > AppConsts.MaxDescriptorsInImage);
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        var imglist = _imgList
                            .Where(e => e.Value.GetDescriptors() != null && _imgList.ContainsKey(e.Value.NextName))
                            .Select(e => e.Value)
                            .ToArray();

                        if (imglist.Length < 2) {
                            progress.Report("No images to view");
                            return;
                        }

                        var mincounter = imglist.Min(e => e.Counter);
                        imglist = imglist.Where(e => e.Counter == mincounter).ToArray();

                        nameX = imglist
                            .OrderBy(e => e.LastView)
                            .FirstOrDefault()
                            .Name;
                        
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

            var sb = new StringBuilder(GetPrompt());
            var secs = DateTime.Now.Subtract(dtn).TotalSeconds;
            sb.Append($"{AppVars.MoveMessage} ");
            imgX = AppVars.ImgPanel[0].Img;
            sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
            sb.Append($"({secs:F2}s)");
            sb.Append($" bigs:{bigdescriptors}");
            progress.Report(sb.ToString());
        }
    }
}
