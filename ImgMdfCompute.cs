using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Compute(BackgroundWorker backgroundworker)
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
            var nextdiff = img1.GetDiff();
            var nextdistance = img1.Distance;
            var lastid = img1.LastId;
            Img[] candidates = null;
            lock (_imglock) {
                if (img1.NextHash.Equals(img1.Hash) || !_hashList.ContainsKey(img1.NextHash)) {
                    nexthash = img1.Hash;
                    nextdiff = new byte[1] { 0xFF };
                    nextdistance = 256;
                    lastid = 0;
                }
            }

            lock (_imglock) {
                candidates = _imgList
                    .Select(e => e.Value)
                    .ToArray();
            }

            foreach (var e in candidates) {
                if (e.Id == img1.Id) {
                    continue;
                }

                var distancet = ImageHelper.ComputeDistance(img1.GetHashes(), e.GetHashes());
                if (distancet < AppConsts.PDistance && distancet < nextdistance) {
                    nextdistance = distancet;
                    nexthash = e.Hash;
                    nextdiff = ImageHelper.ComputeDiff(img1.GetDescriptors(), e.GetDescriptors());
                }
            }

            var sb = new StringBuilder();
            if (nextdistance < AppConsts.PDistance) {
                lock (_imglock) {
                    if (nextdistance < img1.Distance) {
                        var avg = (_imgList.Sum(e => (float)e.Value.LastId) * 100f) / ((float)_imgList.Count * _imgList.Count);
                        sb.Append($"[{avg:F1}%] ");
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Folder}\\{img1.Name}: ");
                        sb.Append($"{img1.Distance} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"{nextdistance} ");
                        img1.Distance = nextdistance;
                        img1.NextHash = nexthash;
                        img1.Diff = nextdiff;
                        img1.LastChanged = DateTime.Now;
                        img1.Counter = 0;
                    }
                }
            }
            else {
                lock (_imglock) {
                    candidates = _imgList
                        .Where(e => e.Value.Id > img1.LastId)
                        .OrderBy(e => e.Value.Id)
                        .Take(1000)
                        .Select(e => e.Value)
                        .ToArray();
                }

                foreach (var e in candidates) {
                    if (e.Id == img1.Id) {
                        continue;
                    }

                    lastid = e.Id;
                    var difft = ImageHelper.ComputeDiff(img1.GetDescriptors(), e.GetDescriptors());
                    if (ImageHelper.CompareDiff(difft, nextdiff) < 0) {
                        nextdiff = difft;
                        nexthash = e.Hash;
                        nextdistance = ImageHelper.ComputeDistance(img1.GetHashes(), e.GetHashes());
                    }
                }

                lock (_imglock) {
                    var cmp = ImageHelper.CompareDiff(nextdiff, img1.GetDiff());
                    if (cmp < 0) {
                        var avg = (_imgList.Sum(e => (float)e.Value.LastId) * 100f) / ((float)_imgList.Count * _imgList.Count);
                        sb.Append($"[{avg:F1}%] ");
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        //sb.Append($"{img1.Folder}\\{img1.Name}: ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"[{img1.LastId}+{lastid - img1.LastId}] ");
                        sb.Append($"{ImageHelper.ShowDiff(img1.GetDiff())} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"{ImageHelper.ShowDiff(nextdiff)} ");
                        img1.Distance = nextdistance;
                        img1.NextHash = nexthash;
                        img1.Diff = nextdiff;
                        img1.LastChanged = DateTime.Now;
                        img1.Counter = 0;
                    }

                    if (img1.LastId != lastid) {
                        img1.LastId = lastid;
                    }
                }
            }

            img1.LastCheck = DateTime.Now;

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}