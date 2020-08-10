using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private int _find = 1;

        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Contract.Requires(progress != null);

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
                            .OrderBy(e => e.LastView)
                            .ToArray();

                        //var mincounter = imglist.Min(e => e.Counter);
                        //imglist = imglist.Where(e => e.Counter == mincounter).ToArray();

                        if (imglist.Length < 2) {
                            progress.Report("No images to view");
                            return;
                        }

                        /*
                        _find = 1 - _find;
                        if (_find == 0) {
                            var maxfolder = imglist.Max(e => e.Folder);
                            imglist = imglist.Where(e => e.Folder == maxfolder).ToArray();
                            nameX = imglist.FirstOrDefault().Name;
                        }
                        else {
                            var dts = new[] { "H", "S", "R" };
                            do {
                                 foreach (var dt in dts) {
                                    var dtscope = imglist.Where(e => e.Dt.Equals(dt, StringComparison.OrdinalIgnoreCase)).ToArray();
                                    if (dtscope.Length > 0) {
                                        nameX = dtscope.OrderBy(e => e.Dv).FirstOrDefault().Name;
                                        break;
                                    }
                                }
                            }
                            while (string.IsNullOrEmpty(nameX));
                        }
                        */

                        
                        var index = _find - 1;
                        if (index >= imglist.Length) {
                            _find = 1;
                            index = 0;

                            var mincounter = imglist.Min(e => e.Counter);
                            imglist = imglist.Where(e => e.Counter == mincounter).ToArray();
                            var dts = new[] { "H", "S", "R" };
                            do {
                                foreach (var dt in dts) {
                                    var dtscope = imglist.Where(e => e.Dt.Equals(dt, StringComparison.OrdinalIgnoreCase)).ToArray();
                                    if (dtscope.Length > 0) {
                                        nameX = dtscope.OrderBy(e => e.Dv).FirstOrDefault().Name;
                                        break;
                                    }
                                }
                            }
                            while (string.IsNullOrEmpty(nameX));
                        }
                        else {
                            nameX = imglist[index].Name;
                            _find *= 2;
                        }
                        

                        AppVars.ImgPanel[0] = GetImgPanel(nameX);
                        if (AppVars.ImgPanel[0] == null) {
                            Delete(nameX);
                            progress.Report($"{nameX} deleted");
                            nameX = string.Empty;
                            continue;
                        }

                        while (true) {
                            nameY = AppVars.ImgPanel[0].Img.NextName;
                            AppVars.ImgPanel[1] = GetImgPanel(nameY);
                            if (AppVars.ImgPanel[1] == null) {
                                Delete(nameY);
                                progress.Report($"{nameY} deleted");
                                nameY = string.Empty;
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
            var imgX = AppVars.ImgPanel[0].Img;
            sb.Append($" {imgX.Folder:D2}\\{imgX.Name}: ");
            sb.Append($"[{imgX.Dt}] {imgX.Dv:F4} ");
            sb.Append($"({secs:F2}s)");
            progress.Report(sb.ToString());
        }
    }
}
