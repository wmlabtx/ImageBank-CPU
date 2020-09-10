using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
                    Img[] xlist;
                    xlist = _imgList
                        .Where(e => e.Value.Descriptors == null || e.Value.Descriptors.Length == 0)
                        .Select(e => e.Value)
                        .OrderBy(e => e.LastCheck)
                        .ToArray();

                    if (xlist.Length > 0) {
                        nameX = xlist.First().Name;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        xlist = _imgList
                            .Where(e => e.Value.Descriptors != null && e.Value.Descriptors.Length > 0)
                            .Select(e => e.Value)
                            .OrderBy(e => e.LastCheck)
                            .ToArray();

                        if (xlist.Length > 0) {
                            nameX = xlist.First().Name;
                        }
                    }
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

                    if (!_imgList.ContainsKey(imgX.NextName)) {
                        imgX.Distance = 256f;
                    }
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
            var random = new Random();
            var sw = Stopwatch.StartNew();
            while (candidates.Count > 0) {
                var index = random.Next(candidates.Count);
                var candidate = candidates[index];
                candidates.RemoveAt(index);

                var d = DescriptorHelper.GetDistance(imgX.Descriptors, candidate.Descriptors);
                if (d < distance) {
                    distance = d;
                    nextname = candidate.Name;
                }

                if (sw.ElapsedMilliseconds > 1000) {
                    break;
                }
            }

            sw.Stop();
            
            var sb = new StringBuilder();
            if (distance < imgX.Distance) {
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
                if (
                    Math.Abs(distance - imgX.Distance) < 0.0001 &&
                    !nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)
                    ) {
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