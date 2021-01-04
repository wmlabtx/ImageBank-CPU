using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            int wc;
            Img imgX;
            var candidates = new List<Img>();
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                string nameX = null;
                while (string.IsNullOrEmpty(nameX)) {
                    imgX = _imgList
                        .FirstOrDefault(e =>
                            !_imgList.ContainsKey(e.Value.NextName) ||
                            e.Value.GetDescriptors() == null ||
                            e.Value.GetDescriptors().Length == 0 ||
                            /*!e.Value.Name.StartsWith("mzx-") ||*/
                            e.Value.Name.Equals(e.Value.NextName))
                        .Value;

                    if (imgX == null) {
                        nameX = _imgList
                            .OrderBy(e => e.Value.LastCheck)
                            .FirstOrDefault()
                            .Value
                            ?.Name;
                    }
                    else {
                        nameX = imgX.Name;
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

                if (
                    !imgX.Name.StartsWith("mzx-") || 
                    string.IsNullOrEmpty(imgX.Hash) || 
                    imgX.GetDescriptors() == null || 
                    imgX.GetDescriptors().Length == 0) {
                    if (!ImageHelper.GetImageDataFromFile(
                        imgX.FileName,
                        out var imagedata,
                        out var bitmap,
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

                    var lastmodified = File.GetLastWriteTime(imgX.FileName);
                    if (lastmodified > DateTime.Now) {
                        lastmodified = DateTime.Now;
                    }

                    Delete(nameX);

                    var hash = Helper.ComputeHash(imagedata);
                    nameX = $"mzx-{hash.Substring(0, 6)}";
                    var imgfilename = Helper.GetFileName(nameX, imgX.Folder);
                    Helper.WriteData(imgfilename, imagedata);
                    File.SetLastWriteTime(imgfilename, lastmodified);
                    var cloneX = new Img(
                        name: nameX,
                        hash: hash,
                        width: bitmap.Width,
                        heigth: bitmap.Height,
                        size: imagedata.Length,
                        descriptors: descriptors,
                        folder: imgX.Folder,
                        lastview: imgX.LastView,
                        lastcheck: imgX.LastCheck,
                        lastadded: imgX.LastAdded,
                        nextname: imgX.NextName,
                        sim: imgX.Sim,
                        family: string.Empty,
                        counter: imgX.Counter);

                    bitmap.Dispose();
                    if (_hashList.ContainsKey(hash)) {
                        var nameF = _hashList[hash];
                        var imgF = _imgList[nameF];
                        if (cloneX.LastAdded > imgF.LastAdded) {
                            Delete(nameF);
                            Add(cloneX);
                            imgX = cloneX;
                        }
                        else {
                            imgX = imgF;
                        }
                    }
                    else {
                        Add(cloneX);
                        imgX = cloneX;
                    }
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

            var nextname = imgX.NextName;
            var sim = 0f;

            if (candidates.Count == 0) {
                backgroundworker.ReportProgress(0, "no candidates");
                return;
            }

            var xd = imgX.GetDescriptors();
            foreach (var candidate in candidates) {
                var imgsim = ImageHelper.GetSim(xd, candidate.GetDescriptors());
                if (imgsim > sim) {
                    nextname = candidate.Name;
                    sim = imgsim;
                }
            }

            var sb = new StringBuilder();
            if (Math.Abs(sim - imgX.Sim) >= 0.0001) {
                if (wc > 0) {
                    sb.Append($"{wc}: ");
                }

                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
                sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
                sb.Append($"{imgX.Sim:F4} ");
                sb.Append($"{char.ConvertFromUtf32(sim > imgX.Sim ? 0x2192 : 0x2193)} ");
                sb.Append($"{sim:F4} ");
                imgX.Sim = sim;
                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    imgX.NextName = nextname;
                    imgX.Counter = 0;
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
                    imgX.Counter = 0;
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