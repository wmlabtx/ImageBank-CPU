using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private Random _random = new Random();

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
                            .Select(e => e.Value)
                            .ToArray();

                        if (imglist.Length < 2) {
                            progress.Report("No images to view");
                            return;
                        }

                        var dtmin = imglist.Min(e => e.LastView.Date);
                        imglist = imglist
                            .Where(e => e.LastView.Date == dtmin)
                            .OrderBy(e => e.LastView)
                            .Take(100)
                            .ToArray();

                        var index = _random.Next(imglist.Length);
                        nameX = imglist[index].Name;

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
                                break;
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

        public void FastFindNext(Img imgX)
        {
            Contract.Requires(imgX != null);

            var nextname = imgX.NextName;
            var distance = double.MaxValue;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    return;
                }

                var xcolors = imgX.GetHistogram();
                foreach (var img in _imgList) {
                    if (img.Value.GetHistogram() == null || img.Value.GetHistogram().Length == 0) {
                        continue;
                    }

                    if (imgX.Name.Equals(img.Value.Name, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(imgX.Family) &&
                        imgX.Family.Equals(img.Value.Family, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    var ycolors = img.Value.GetHistogram();
                    var imgdistance = ImageHelper.Distance(xcolors, ycolors);
                    if (imgdistance < distance) {
                        nextname = img.Value.Name;
                        distance = imgdistance;
                    }
                }

                if (Math.Abs(distance - imgX.Distance) >= 0.0001) {
                    imgX.Distance = (float)distance;
                }

                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    imgX.NextName = nextname;
                }

                imgX.LastCheck = DateTime.Now;
            }
        }
    }
}
