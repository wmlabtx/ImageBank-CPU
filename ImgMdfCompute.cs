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

            var wrongcount = 0;
            Img imgX;
            var candidates = new List<Img>();
            lock (_imglock)
            {
                if (_imgList.Count < 2)
                {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                string nameX = null;
                while (string.IsNullOrEmpty(nameX))
                {
                    var scopewrong = _imgList
                        .Where(e =>
                            !_imgList.ContainsKey(e.Value.NextName) ||
                            e.Value.GetDescriptors() == null ||
                            e.Value.GetDescriptors().Length == 0 ||
                            e.Value.Name.Equals(e.Value.NextName))
                        .ToArray();
                    wrongcount = scopewrong.Length;
                    if (wrongcount > 0) {
                        nameX = scopewrong.First().Value.Name;
                    }
                    else {
                        nameX = _imgList
                            .OrderBy(e => e.Value.LastCheck)
                            .FirstOrDefault()
                            .Value
                            ?.Name;
                    }
                }

                if (!_imgList.TryGetValue(nameX, out imgX))
                {
                    backgroundworker.ReportProgress(0, $"error getting {nameX}");
                    return;
                }

                if (!File.Exists(imgX.FileName))
                {
                    Delete(nameX);
                    backgroundworker.ReportProgress(0, $"{nameX} deleted");
                    return;
                }

                if (
                    string.IsNullOrEmpty(imgX.Hash) ||
                    imgX.GetDescriptors() == null ||
                    imgX.GetDescriptors().Length == 0)
                {
                    if (!ImageHelper.GetImageDataFromFile(
                        imgX.FileName,
                        out var imagedata,
                        out var bitmap,
                        out _))
                    {
                        Delete(nameX);
                        backgroundworker.ReportProgress(0, $"{nameX} deleted");
                        return;
                    }

                    if (!ImageHelper.ComputeDescriptors(bitmap, out var descriptors))
                    {
                        Delete(nameX);
                        backgroundworker.ReportProgress(0, $"{nameX} deleted");
                        return;
                    }

                    var lastmodified = File.GetLastWriteTime(imgX.FileName);
                    if (lastmodified > DateTime.Now)
                    {
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
                    if (_hashList.ContainsKey(hash))
                    {
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

                foreach (var e in _imgList) {
                    if (e.Value.GetDescriptors() == null && e.Value.GetDescriptors().Length == 0) {
                        continue;
                    }

                    if (imgX.Name.Equals(e.Value.Name)) {
                        continue;
                    }

                    e.Value.SimFast = 0f;
                    candidates.Add(e.Value);
                }
            }

            foreach (var candidate in candidates) {
                candidate.SimFast = ImageHelper.GetSimFast(imgX.GetDescriptors(), candidate.GetDescriptors());
            }

            candidates = candidates
                .ToList()
                .OrderByDescending(e => e.SimFast)
                .Take(10000)
                .ToList();

            var sim = 0f;
            var nextname = imgX.Name;
            lock (_imglock) {
                foreach (var candidate in candidates) {
                    var fsim = ImageHelper.GetSim(imgX.GetDescriptors(), candidate.GetDescriptors());
                    if (fsim > sim) {
                        nextname = candidate.Name;
                        sim = fsim;
                    }
                }
            }

            var sb = new StringBuilder();
            if (Math.Abs(sim - imgX.Sim) >= 0.0001) {
                if (wrongcount > 0) {
                    sb.Append($"{wrongcount}: ");
                }

                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
                sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
                sb.Append($"{imgX.Sim:F4} ");
                sb.Append($"{char.ConvertFromUtf32(sim > imgX.Sim ? 0x2192 : 0x2193)} ");
                sb.Append($"{sim:F4} ");
                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    imgX.NextName = nextname;
                    if (sim > imgX.Sim) {
                        imgX.Counter = 0;
                    }
                }

                imgX.Sim = sim;
            }
            else {
                if (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase)) {
                    if (wrongcount > 0) {
                        sb.Append($"{wrongcount}: ");
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