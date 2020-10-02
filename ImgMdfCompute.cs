using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Compute(BackgroundWorker backgroundworker)
        {
            Contract.Requires(backgroundworker != null);

            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            Img imgX;
            List<Img> candidates;
            lock (_imglock) {
                if (_imgList.Count == 0) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                string nameX = null;
                while (string.IsNullOrEmpty(nameX)) {
                    var imglist = _imgList
                        .Select(e => e.Value)
                        .ToArray();

                    /*
                    var mincounter = imglist.Min(e => e.Counter);
                    imglist = imglist
                        .Where(e => e.Counter == mincounter)
                        .ToArray();
                    */
/*

                    var maxfolder = imglist.Max(e => e.Folder);
                    imglist = imglist
                        .Where(e => e.Folder == maxfolder)
                        .ToArray();
*/

                    nameX = imglist
                        .OrderBy(e => e.LastCheck)
                        .FirstOrDefault()
                        .Name;
                }

                if (!_imgList.TryGetValue(nameX, out imgX)) {
                    backgroundworker.ReportProgress(0, $"error getting {nameX}");
                    return;
                }

                if (!File.Exists(imgX.FileName)) {
                    Delete(nameX);
                    backgroundworker.ReportProgress(0, $"{nameX} deleted");
                    return;
                }

                if (imgX.Descriptors == null || imgX.Descriptors.Length == 0) {
                    if (!Helper.GetImageDataFromFile(
                        imgX.FileName,
                        out _,
#pragma warning disable CA2000 // Dispose objects before losing scope
                        out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                        out _,
                        out _)) {
                        Delete(nameX);
                        backgroundworker.ReportProgress(0, $"{nameX} deleted");
                        return;
                    }

                    if (!DescriptorHelper.Compute(bitmap, out var descriptors)) {
                        Delete(nameX);
                        backgroundworker.ReportProgress(0, $"{nameX} deleted");
                        return;
                    }

                    bitmap.Dispose();

                    imgX.Descriptors = descriptors;
                }

                var history = imgX.History;
                var offset = 0;
                while (offset + 10 <= history.Length) {
                    var prevname = history.Substring(offset, 10);
                    if (!_imgList.ContainsKey(prevname)) {
                        imgX.RemoveFromHistory(prevname);
                    }

                    offset += 10;
                }

                candidates = _imgList
                    .Where(e => e.Value.Descriptors != null && e.Value.Descriptors.Length > 0)
                    .Where(e => !imgX.IsInHistory(e.Value.Name))
                    .Where(e => !imgX.Name.Equals(e.Value.Name, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Value)
                    .ToList();
            }

            if (candidates.Count == 0) {
                backgroundworker.ReportProgress(0, "no candidates");
                return;
            }

            var distance = float.MaxValue;
            var nextname = string.Empty;
            foreach (var candidate in candidates) { 
                var d = DescriptorHelper.GetDistance(imgX.Descriptors, candidate.Descriptors);
                if (d < distance) {
                    distance = d;
                    nextname = candidate.Name;
                }
            }
            
            var sb = new StringBuilder();
            if (Math.Abs(distance - imgX.Distance) >= 0.0001) {
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
                sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
                sb.Append($"{imgX.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(distance < imgX.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{distance:F2} ");
                imgX.Distance = distance;
                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    imgX.NextName = nextname;
                }
            }
            else {
                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
                    sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
                    sb.Append($"{imgX.NextName} ");
                    sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                    sb.Append($"{nextname}");
                    imgX.NextName = nextname;
                }
            }

            imgX.LastCheck = DateTime.Now;
            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}