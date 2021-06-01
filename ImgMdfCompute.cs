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
            Img[] candidates;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                foreach (var e in _imgList) {
                    var eX = e.Value;
                    if (eX.Hash.Equals(eX.NextHash)) {
                        img1 = eX;
                        break;
                    }

                    if (!_hashList.TryGetValue(eX.NextHash, out var eY)) {
                        img1 = eX;
                        break;
                    }

                    if (img1 == null || eX.LastCheck < img1.LastCheck) {
                        img1 = eX;
                    }
                }

                if (!_hashList.ContainsKey(img1.NextHash)) {
                    img1.NextHash = img1.Hash;
                    if (img1.AkazePairs > 0) {
                        img1.AkazePairs = 0;
                    }
                }

                candidates = _imgList
                    .Where(e => e.Value.Id != img1.Id)
                    .OrderBy(e => e.Value.Id)
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (candidates.Length == 0) {
                backgroundworker.ReportProgress(0, "no candidates");
                return;
            }

            var nexthash = img1.NextHash;
            var akazepairs = img1.AkazePairs;
            var lastchanged = img1.LastChanged;

            for (var i = 0; i < candidates.Length; i++) {
                var m = ImageHelper.ComputeKazeMatch(img1.AkazeCentroid, candidates[i].AkazeCentroid, candidates[i].AkazeMirrorCentroid);
                if (m > akazepairs) {
                    lock (_imglock) {
                        if (_imgList.ContainsKey(img1.Id) && _imgList.ContainsKey(candidates[i].Id)) {
                            nexthash = candidates[i].Hash;
                            akazepairs = m;
                            lastchanged = DateTime.Now;
                        }
                    }
                }
            }

            if (!nexthash.Equals(img1.NextHash)) {
                var sb = new StringBuilder();
                lock (_imglock) {
                    var scope = _imgList.Where(e => e.Value.Hash.Equals(e.Value.NextHash) || !_hashList.ContainsKey(e.Value.NextHash)).ToArray();
                    if (scope.Length > 0) {
                        sb.Append($"{scope.Length}");
                    }
                    else {
                        scope = _imgList.Where(e => e.Value.LastView <= e.Value.LastChanged).ToArray();
                        if (scope.Length == 0) {
                            sb.Append($"nc");
                        }
                        else {
                            var maxap = scope.Max(e => e.Value.AkazePairs);
                            if (maxap > AppVars.ImgPanel[0].Img.AkazePairs) {
                                sb.Append($"{maxap}");
                            }
                            else {
                                sb.Append("=");
                            }

                            sb.Append($"/{scope.Length}");
                        }
                    }
                }

                sb.Append(": ");
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img1.Id}: ");
                sb.Append($"{img1.AkazePairs} ");
                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                sb.Append($"{akazepairs} ");
                backgroundworker.ReportProgress(0, sb.ToString());
            }

            if (!nexthash.Equals(img1.NextHash)) {
                img1.NextHash = nexthash;
            }

            if (img1.AkazePairs != akazepairs) {
                img1.AkazePairs = akazepairs;
            }

            if (img1.LastChanged != lastchanged) {
                img1.LastChanged = lastchanged;
            }

            img1.LastCheck = DateTime.Now;
        }
    }
}