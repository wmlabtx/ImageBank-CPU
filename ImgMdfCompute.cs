using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static string GetNew()
        {
            var newpics = _imgList.Count(e => e.Value.Hash.Equals(e.Value.NextHash));
            if (newpics == 0) {
                return string.Empty;
            }

            var message = $"{newpics} ";
            return message;
        }

        public static void Compute(BackgroundWorker backgroundworker)
        {
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            Img img1 = null;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                foreach (var e in _imgList) {
                    var eX = e.Value;
                    if (img1 != null &&
                        eX.Hash.Equals(eX.NextHash)) {
                        img1 = eX;
                        break;
                    }

                    if (img1 != null &&
                        !_hashList.TryGetValue(eX.NextHash, out var eY)) {
                        img1 = eX;
                        break;
                    }

                    if (eX.PerceptiveDistance == AppConsts.MaxPerceptiveDistance || eX.OrbDistance == AppConsts.MaxOrbDistance) {
                        img1 = eX;
                        break;
                    }

                    if (img1 != null &&
                        img1.LastCheck <= eX.LastCheck) {
                        continue;
                    }

                    img1 = eX;
                }
            }

            var nexthash = img1.NextHash;
            var nc = img1.ColorDistance;
            var np = img1.PerceptiveDistance;
            var no = img1.OrbDistance;
            lock (_imglock) {
                if (img1.NextHash.Equals(img1.Hash) || !_hashList.ContainsKey(img1.NextHash) || img1.PerceptiveDistance == AppConsts.MaxPerceptiveDistance || img1.OrbDistance == AppConsts.MaxOrbDistance) {
                    nexthash = img1.Hash;
                    img1.ColorDistance = 100f;
                    img1.PerceptiveDistance = AppConsts.MaxPerceptiveDistance;
                    img1.OrbDistance = AppConsts.MaxOrbDistance;
                    nc = img1.ColorDistance;
                    np = img1.PerceptiveDistance;
                    no = img1.OrbDistance;
                }
            }

            List<Img> candidates = null;
            lock (_imglock) {
                candidates = _imgList
                    .Where(e => e.Value.Id != img1.Id)
                    .Select(e => e.Value)
                    .ToList();
            }

            if (candidates.Count == 0) {
                img1.LastCheck = DateTime.Now;
                return;
            }

            var sw = new Stopwatch();
            sw.Restart();
            while (candidates.Count > 0) {
                var index = _random.Next(candidates.Count);
                var e = candidates[index];
                candidates.RemoveAt(index);
                var dip = ImageHelper.ComputePerceptiveDistance(img1.PerceptiveDescriptors, e.PerceptiveDescriptors);
                if (dip < AppConsts.MinPerceptiveDistance && dip < np) {
                    np = dip;
                    nexthash = e.Hash;
                    if (np == 0) {
                        break;
                    }
                }

                if (sw.ElapsedMilliseconds >= 1000) {
                    break;
                }
            }

            sw.Stop();
            if (np < img1.PerceptiveDistance) {
                lock (_imglock) {
                    if (_hashList.TryGetValue(nexthash, out var img2)) {
                        var sb = new StringBuilder();
                        sb.Append(GetNew());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"{img1.PerceptiveDistance} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"{np} ");
                        img1.PerceptiveDistance = np;
                        img1.NextHash = nexthash;
                        img1.ColorDistance = ImageHelper.ComputeColorDistance(img1.ColorDescriptors, img2.ColorDescriptors);
                        img1.OrbDistance = ImageHelper.ComputeOrbDistance_v2(img1.OrbDescriptors, img1.OrbKeyPoints, img2.OrbDescriptors, img2.OrbKeyPoints);
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

            lock (_imglock) {
                candidates = _imgList
                    .Where(e => e.Value.Id != img1.Id)
                    .Select(e => e.Value)
                    .ToList();
            }

            sw.Restart();
            while (candidates.Count > 0) {
                var index = _random.Next(candidates.Count);
                var e = candidates[index];
                candidates.RemoveAt(index);
                var dio = ImageHelper.ComputeOrbDistance_v2(img1.OrbDescriptors, img1.OrbKeyPoints, e.OrbDescriptors, e.OrbKeyPoints);
                if (dio < AppConsts.MinOrbDistance && dio < no) {
                    no = (int)dio;
                    nexthash = e.Hash;
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
                        sb.Append(GetNew());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"o:{img1.OrbDistance:F2} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"o:{no:F2} ");
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

            if (img1.OrbDistance < AppConsts.MinOrbDistance) {
                img1.LastCheck = DateTime.Now;
                return;
            }

            lock (_imglock) {
                candidates = _imgList
                    .Where(e => e.Value.Id != img1.Id)
                    .Select(e => e.Value)
                    .ToList();
            }

            sw.Restart();
            while (candidates.Count > 0) {
                var index = _random.Next(candidates.Count);
                var e = candidates[index];
                candidates.RemoveAt(index);
                var dic = ImageHelper.ComputeColorDistance(img1.ColorDescriptors, e.ColorDescriptors);
                if (dic < nc) {
                    nc = dic;
                    nexthash = e.Hash;
                }

                if (sw.ElapsedMilliseconds >= 1000) {
                    break;
                }
            }

            sw.Stop();

            if (nc < img1.ColorDistance) {
                lock (_imglock) {
                    if (_hashList.TryGetValue(nexthash, out var img2)) {
                        var sb = new StringBuilder();
                        sb.Append(GetNew());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"c:{img1.ColorDistance:F2} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"c:{nc:F2} ");
                        img1.ColorDistance = nc;
                        img1.NextHash = nexthash;
                        img1.OrbDistance = ImageHelper.ComputeOrbDistance_v2(img1.OrbDescriptors, img1.OrbKeyPoints, img2.OrbDescriptors, img2.OrbKeyPoints);
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