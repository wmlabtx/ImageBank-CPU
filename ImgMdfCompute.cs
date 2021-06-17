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
                    if (img1.KazeMatch > 0) {
                        img1.KazeMatch = 0;
                    }
                }

                candidates = _imgList
                    .Where(e => !e.Value.FileName.Equals(img1.FileName, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (candidates.Length == 0) {
                backgroundworker.ReportProgress(0, "no candidates");
                return;
            }

            var nexthash = img1.NextHash;
            var kazematch = img1.KazeMatch;
            var lastchanged = img1.LastChanged;

            for (var i = 0; i < candidates.Length; i++) {
                var m = ImageHelper.ComputeKazeMatch(img1.KazeOne, candidates[i].KazeOne, candidates[i].KazeTwo);
                if (m > kazematch) {
                    lock (_imglock) {
                        if (_imgList.ContainsKey(img1.FileName) && _imgList.ContainsKey(candidates[i].FileName)) {
                            nexthash = candidates[i].Hash;
                            kazematch = m;
                            lastchanged = DateTime.Now;
                        }
                    }
                }
            }

            if (!nexthash.Equals(img1.NextHash)) {
                var sb = new StringBuilder();
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($"{img1.FileName}: ");
                sb.Append($"{img1.KazeMatch} ");
                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                sb.Append($"{kazematch} ");
                backgroundworker.ReportProgress(0, sb.ToString());
            }

            if (!nexthash.Equals(img1.NextHash)) {
                img1.NextHash = nexthash;
            }

            if (img1.KazeMatch != kazematch) {
                img1.KazeMatch = kazematch;
            }

            if (img1.LastChanged != lastchanged) {
                img1.LastChanged = lastchanged;
            }

            img1.LastCheck = DateTime.Now;
        }
    }
}