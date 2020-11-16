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

            var wc = 0;
            Img imgX;
            List<Img> candidates = new List<Img>();
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                string nameX = null;
                while (string.IsNullOrEmpty(nameX)) {
                    var scoperecent = _imgList.Where(e =>
                        DateTime.Now.Subtract(e.Value.LastAdded).TotalDays < 1 &&
                        DateTime.Now.Subtract(e.Value.LastView).TotalDays > 3000
                        ).ToArray();

                    if (scoperecent.Length > 0) {
                        nameX = scoperecent
                                    .OrderBy(e => e.Value.LastCheck)
                                    .FirstOrDefault()
                                    .Value
                                    .Name;
                    }
                    else {
                        nameX = _imgList
                                    .OrderBy(e => e.Value.LastCheck)
                                    .FirstOrDefault()
                                    .Value
                                    .Name;
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

                if (imgX.GetDescriptors() == null || imgX.GetDescriptors().Length == 0) {
                    if (!ImageHelper.GetImageDataFromFile(
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

                    if (!ImageHelper.ComputeDescriptors(bitmap, out var descriptors)) {
                        Delete(nameX);
                        backgroundworker.ReportProgress(0, $"{nameX} deleted");
                        return;
                    }

                    bitmap.Dispose();
                    imgX.SetDescriptors(descriptors);
                }

                candidates.Clear();
                foreach (var img in _imgList) {
                    if (img.Value.GetDescriptors() == null || img.Value.GetDescriptors().Length == 0) {
                        continue;
                    }

                    if (imgX.Name.Equals(img.Value.Name, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(imgX.Family) && 
                        !imgX.Family.Equals(img.Value.Family, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    candidates.Add(img.Value);
                }

                if (candidates.Count == 0) {
                    foreach (var img in _imgList) {
                        if (img.Value.GetDescriptors() == null || img.Value.GetDescriptors().Length == 0) {
                            continue;
                        }

                        if (imgX.Name.Equals(img.Value.Name, StringComparison.OrdinalIgnoreCase)) {
                            continue;
                        }

                        candidates.Add(img.Value);
                    }
                }

                wc = candidates.Count;
            }

            if (candidates.Count == 0) {
                backgroundworker.ReportProgress(0, "no candidates");
                return;
            }

            var nextname = imgX.NextName;
            var distance = double.MaxValue;
            var xd = imgX.GetDescriptors();
            foreach (var candidate in candidates) {
                var yd = candidate.GetDescriptors();
                var imgdistance = ColorDescriptor.Distance(xd, yd);
                if (imgdistance < distance) {
                    nextname = candidate.Name;
                    distance = imgdistance;
                }
            }

            var sb = new StringBuilder();
            if (Math.Abs(distance - imgX.Distance) >= 0.0001) {
                if (wc > 0) {
                    sb.Append($"{wc}: ");
                }

                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
                sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
                sb.Append($"{imgX.Distance:F4} ");
                sb.Append($"{char.ConvertFromUtf32(distance < imgX.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{distance:F4} ");
                imgX.Distance = (float)distance;
                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    imgX.NextName = nextname;
                }
            }
            else {
                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    if (wc > 0) {
                        sb.Append($"{wc}: ");
                    }

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