using System;
using System.Collections.Generic;
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
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        var imglist = _imgList
                            .Where(e => e.Value.Descriptors != null && e.Value.Descriptors.Length > 0 && _imgList.ContainsKey(e.Value.NextName))
                            .Select(e => e.Value)
                            .ToArray();

                        if (imglist.Length < 2) {
                            progress.Report("No images to view");
                            return;
                        }

                        var mincounter = imglist.Min(e => e.Counter);
                        imglist = imglist
                            .Where(e => e.Counter == mincounter)
                            .OrderBy(e => e.LastView)
                            .ToArray();


                        /*
                        var families = new SortedDictionary<string, DateTime>();
                        foreach(var img in imglist) {
                            var family = img.Family;
                            if (families.ContainsKey(family)) {
                                if (img.LastView > families[family]) {
                                    families[family] = img.LastView;
                                }
                            }
                            else {
                                families.Add(family, img.LastView);
                            }
                        }

                        var minfamily = string.Empty;
                        var minlv = DateTime.MaxValue;
                        foreach (var familypair in families) {
                            if (familypair.Value < minlv) {
                                minlv = familypair.Value;
                                minfamily = familypair.Key;
                            }
                        }

                        Img[] familylist;
                        if (string.IsNullOrEmpty(minfamily)) {
                            familylist = imglist
                                .Where(e => string.IsNullOrEmpty(e.Family))
                                .OrderBy(e => e.LastView)
                                .ToArray();
                        }
                        else {
                            familylist = imglist
                                .Where(e => e.Family.Equals(minfamily, StringComparison.OrdinalIgnoreCase))
                                .OrderBy(e => e.LastView)
                                .ToArray();
                        }

                        nameX = familylist
                            .FirstOrDefault()
                            .Name;
                        */

                        nameX = imglist
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
            sb.Append($"{imgX.Distance:F2} ");
            sb.Append($"({secs:F2}s)");
            progress.Report(sb.ToString());
        }
    }
}
