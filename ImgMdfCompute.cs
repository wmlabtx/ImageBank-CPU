using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static readonly Random Random = new Random();

        public void Compute(BackgroundWorker backgroundworker)
        {
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);
            
            Img imgX = null;
            var candidates = new List<Img>();
            var nexthash = string.Empty;
            var distance = 256f;
            lock (_imglock)
            {
                if (_imgList.Count < 2)
                {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                foreach (var e in _imgList)
                {
                    var eX = e.Value;
                    if (eX.Hash.Equals(eX.NextHash))
                    {
                        imgX = eX;
                        distance = 256f;
                        break;
                    }

                    if (!_hashList.TryGetValue(eX.NextHash, out var eY)) {
                        imgX = eX;
                        nexthash = eX.Hash;
                        distance = 256f;
                        break;
                    }

                    if (!eX.Folder.StartsWith(AppConsts.FolderDefault) && !eX.Folder.Equals(eY.Folder) && FolderSize(eX.Folder) > 1) {
                        imgX = eX;
                        distance = 256f;
                        break;
                    }

                    if (imgX != null && imgX.Counter == 0 && eX.Counter > 0) {
                        continue;
                    }

                    if (imgX != null && imgX.Counter == eX.Counter && imgX.LastView <= eX.LastView) {
                        continue;
                    }

                    imgX = eX;
                    nexthash = imgX.NextHash;
                    distance = imgX.Distance;
                }

                if (imgX == null) {
                    throw new ArgumentException(nameof(imgX));
                }

                if (!File.Exists(imgX.FileName)) {
                    backgroundworker.ReportProgress(0, $"{imgX.Name} deleted");
                    Delete(imgX.Name);
                    return;
                }

                if (!imgX.Folder.StartsWith(AppConsts.FolderDefault)) {
                    foreach (var e in _imgList) {
                        if (!imgX.Name.Equals(e.Key) && imgX.Folder.Equals(e.Value.Folder)) {
                            candidates.Add(e.Value);
                        }
                    }
                }

                if (candidates.Count == 0) {
                    foreach (var e in _imgList) {
                        if (!imgX.Name.Equals(e.Key)) {
                            candidates.Add(e.Value);
                        }
                    }
                }
            }

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 1000 && candidates.Count > 0) {
                var index = Random.Next(candidates.Count);
                var d = OrbDescriptor.Distance(imgX.GetDescriptors(), candidates[index].GetDescriptors());
                if (d < distance) {
                    nexthash = candidates[index].Hash;
                    distance = d;
                }

                candidates.RemoveAt(index);
            }

            sw.Stop();

            var sb = new StringBuilder();
            if (Math.Abs(distance - imgX.Distance) >= 0.01) {
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
                sb.Append($"{imgX.Folder}\\{imgX.Name}: ");
                sb.Append($"{imgX.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(distance < imgX.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{distance:F2} ");
                if (!nexthash.Equals(imgX.NextHash)) {
                    imgX.NextHash = nexthash;
                }

                imgX.Distance = distance;
                imgX.Counter = 0;
            }
            else {
                if (!nexthash.Equals(imgX.NextHash, StringComparison.OrdinalIgnoreCase)) {
                    sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
                    sb.Append($"{imgX.Folder}\\{imgX.Name}: ");
                    sb.Append($"{nexthash.Substring(0, 8)}...");
                    imgX.NextHash = nexthash;
                    imgX.Counter = 0;
                }
                else {
                    imgX.LastCheck = DateTime.Now;
                    imgX.Counter = 1;
                }
            }

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}