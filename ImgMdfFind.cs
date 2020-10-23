using System;
using System.Collections.Generic;
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
                            .Where(e => e.Value.GetColors() != null && e.Value.GetColors().Length > 0)
                            .OrderBy(e => e.Value.LastView)
                            .Select(e => e.Value)
                            .ToArray();

                        if (imglist.Length < 2) {
                            progress.Report("No images to view");
                            return;
                        }

                        /*
                        var df = new SortedDictionary<int, DateTime>();
                        foreach(var img in imglist) {
                            if (df.ContainsKey(img.Folder)) {
                                if (df[img.Folder] < img.LastView) {
                                    df[img.Folder] = img.LastView;
                                }
                            }
                            else {
                                df.Add(img.Folder, img.LastView);
                            }
                        }

                        var mind = df.Min(e => e.Value);
                        var minf = df.First(e => e.Value == mind).Key;
                        imglist = imglist
                            .Where(e => e.Folder == minf)
                            .ToArray();

                        nameX = imglist
                            .OrderBy(e => e.LastView)
                            .FirstOrDefault()
                            .Name;
                            */

                        if (_offset - 1 >= imglist.Length) {
                            _offset = 1;
                        }

                        nameX = imglist[_offset - 1].Name;
                        _offset *= 10;

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

        public void FastFindNext(Img imgX)
        {
            Contract.Requires(imgX != null);

            var nextname = imgX.NextName;
            var distance = float.MaxValue;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    return;
                }

                foreach (var img in _imgList) {
                    if (img.Value.GetColors() == null || img.Value.GetColors().Length == 0) {
                        continue;
                    }

                    if (imgX.Name.Equals(img.Value.Name, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(imgX.Family) &&
                        !imgX.Family.Equals(img.Value.Family, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    var imgdistance = DescriptorHelper.GetDistance(imgX.GetColors(), img.Value.GetColors());
                    if (imgdistance < distance) {
                        nextname = img.Value.Name;
                        distance = imgdistance;
                    }
                }

                if (Math.Abs(distance - imgX.Distance) >= 0.0001) {
                    imgX.Distance = distance;
                }

                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    imgX.NextName = nextname;
                }

                imgX.LastCheck = DateTime.Now;
            }
        }
    }
}
