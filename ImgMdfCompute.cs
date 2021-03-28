using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static string GetAvg()
        {
            var sum = _imgList.Sum(e => (float)e.Value.LastId);
            var avg = (sum / _imgList.Count) / 1024;
            var max = _id / 1024;
            var message = $"[{avg:F0}/{max:F0}] ";
            return message;
        }

        public void Compute(BackgroundWorker backgroundworker)
        {
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            Img img1 = null;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                img1 = _imgList
                    .OrderBy(e => e.Value.LastCheck)
                    .FirstOrDefault()
                    .Value;
            }

            var nexthash = img1.NextHash;
            var nc = img1.ColorDistance;
            var np = img1.PerceptiveDistance;
            var no = img1.OrbDistance;
            var lastid = img1.LastId;
            lock (_imglock) {
                if (img1.NextHash.Equals(img1.Hash) || !_hashList.ContainsKey(img1.NextHash)) {
                    nexthash = img1.Hash;
                    nc = 100f;
                    np = AppConsts.MaxPerceptiveDistance;
                    no = AppConsts.MaxOrbDistance;
                    lastid = 0;
                }
            }

            Img[] candidates = null;
            lock (_imglock) {
                candidates = _imgList
                    .Where(e => e.Value.Id != img1.Id && e.Value.Id > img1.LastId)
                    .OrderBy(e => e.Value.Id)
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (candidates.Length == 0) {
                img1.LastCheck = DateTime.Now;
                return;
            }

            foreach (var e in candidates) {
                var dip = ImageHelper.ComputePerceptiveDistance(img1.PerceptiveDescriptors, e.PerceptiveDescriptors);
                if (dip < AppConsts.MinPerceptiveDistance && dip < np) {
                    np = dip;
                    nexthash = e.Hash;
                    if (np == 0) {
                        break;
                    }
                }
            }

            if (np < img1.PerceptiveDistance) {
                lock (_imglock) {
                    if (_hashList.TryGetValue(nexthash, out var img2)) {
                        var sb = new StringBuilder();
                        sb.Append(GetAvg());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"{img1.PerceptiveDistance} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"{np} ");
                        img1.PerceptiveDistance = np;
                        img1.NextHash = nexthash;
                        img1.ColorDistance = ImageHelper.ComputeColorDistance(img1.ColorDescriptors, img2.ColorDescriptors);
                        img1.OrbDistance = ImageHelper.ComputeOrbDistance(img1.OrbDescriptors, img2.OrbDescriptors);
                        img1.LastChanged = DateTime.Now;
                        backgroundworker.ReportProgress(0, sb.ToString());
                    }
                }

                img1.LastCheck = DateTime.Now;
                return;
            }

            if (img1.PerceptiveDistance < AppConsts.MinPerceptiveDistance) {
                img1.LastCheck = DateTime.Now;
                return;
            }

            var sw = Stopwatch.StartNew();
            foreach (var e in candidates) {
                lastid = e.Id;
                var dio = ImageHelper.ComputeOrbDistance(img1.OrbDescriptors, e.OrbDescriptors);
                if (dio < AppConsts.MinOrbDistance && dio < no) {
                    no = dio;
                    nexthash = e.Hash;
                    if (no == 0) {
                        break;
                    }
                }

                if (sw.ElapsedMilliseconds >= 1000) {
                    break;
                }
            }

            sw.Stop();

            if (no < img1.OrbDistance) {
                lock (_imglock) {
                    if (_hashList.TryGetValue(nexthash, out var img2)) {
                        var sb = new StringBuilder();
                        sb.Append(GetAvg());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"[{img1.LastId}+{lastid - img1.LastId}] ");
                        sb.Append($"o:{img1.OrbDistance} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"o:{no} ");
                        img1.OrbDistance = no;
                        img1.NextHash = nexthash;
                        img1.ColorDistance = ImageHelper.ComputeColorDistance(img1.ColorDescriptors, img2.ColorDescriptors);
                        img1.PerceptiveDistance = ImageHelper.ComputePerceptiveDistance(img1.PerceptiveDescriptors, img2.PerceptiveDescriptors);
                        img1.LastChanged = DateTime.Now;
                        backgroundworker.ReportProgress(0, sb.ToString());
                    }
                }

                img1.LastCheck = DateTime.Now;
                return;
            }

            img1.LastId = lastid;
            if (img1.OrbDistance < AppConsts.MinOrbDistance) {
                img1.LastCheck = DateTime.Now;
                return;
            }

            foreach (var e in candidates) {
                var dic = ImageHelper.ComputeColorDistance(img1.ColorDescriptors, e.ColorDescriptors);
                if (dic < nc) {
                    nc = dic;
                    nexthash = e.Hash;
                }
            }

            if (nc < img1.ColorDistance) {
                lock (_imglock) {
                    if (_hashList.TryGetValue(nexthash, out var img2)) {
                        var sb = new StringBuilder();
                        sb.Append(GetAvg());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"c:{img1.ColorDistance:F2} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"c:{nc:F2} ");
                        img1.ColorDistance = nc;
                        img1.NextHash = nexthash;
                        img1.OrbDistance = ImageHelper.ComputeOrbDistance(img1.OrbDescriptors, img2.OrbDescriptors);
                        img1.PerceptiveDistance = ImageHelper.ComputePerceptiveDistance(img1.PerceptiveDescriptors, img2.PerceptiveDescriptors);
                        img1.LastChanged = DateTime.Now;
                        backgroundworker.ReportProgress(0, sb.ToString());
                    }
                }

                img1.LastCheck = DateTime.Now;
                return;
            }

            img1.LastCheck = DateTime.Now;
        }
    }
}