using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        const int MAXCANDIDATES = 100;

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
            var cmp1 = 0;
            var candidates = new List<Tuple<ulong, Img, int>>();
            var c1 = 0;
            var c2 = 0;
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

                    if (img1 == null) {
                        img1 = eX;
                        if (_resultList.TryGetValue(img1.Id, out var nx)) {
                            cmp1 = nx.Count;
                        }
                    }
                    else {
                        var cmp2 = 0;
                        if (_resultList.TryGetValue(eX.Id, out var ny)) {
                            cmp2 = ny.Count;
                        }

                        if (cmp2 < cmp1) {
                            img1 = eX;
                            cmp1 = cmp2;
                        }
                    }
                }

                foreach (var e in _imgList) {
                    if (img1.Id == e.Value.Id) {
                        continue;
                    }

                    var token = img1.Token ^ e.Value.Token;
                    var ac = -1;
                    if (_resultList.TryGetValue(img1.Id, out var nx)) {
                        c1 = nx.Count;
                        if (nx.TryGetValue(e.Value.Id, out var ny)) {
                            ac = ny;
                        }
                    }

                    candidates.Add(new Tuple<ulong, Img, int>(token, e.Value, ac));
                }
            }

            candidates = candidates.OrderBy(e => e.Item1).ToList();
            var nap = -1;
            var nexthash = img1.Hash;
            var counter = 0;
            for (var i = 0; i < candidates.Count; i++) {
                var nap2 = candidates[i].Item3;
                if (counter < MAXCANDIDATES && nap2 < 0) {
                    var p1 = ImageHelper.ComputeAkazePairs(img1.AkazeDescriptors, candidates[i].Item2.AkazeDescriptors);
                    var p2 = ImageHelper.ComputeAkazePairs(img1.AkazeMirrorDescriptors, candidates[i].Item2.AkazeMirrorDescriptors);
                    nap2 = Math.Max(p1, p2);
                    AddResult(img1.Id, candidates[i].Item2.Id, nap2);
                    counter++;
                }

                if (nap2 > nap) {
                    nap = nap2;
                    nexthash = candidates[i].Item2.Hash;
                }
            }

            if (!nexthash.Equals(img1.NextHash)) {
                lock (_imglock) {
                    if (_hashList.TryGetValue(nexthash, out var img2)) {
                        if (_resultList.TryGetValue(img1.Id, out var nx)) {
                            c2 = nx.Count;
                        }

                        var dc = c2 - c1;
                        var sb = new StringBuilder();
                        sb.Append(GetNew());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"({c1}+{dc}) ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"{img1.AkazePairs} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"{nap} ");
                        img1.AkazePairs = nap;
                        img1.NextHash = nexthash;
                        img1.LastChanged = DateTime.Now;
                        backgroundworker.ReportProgress(0, sb.ToString());
                    }
                }
            }

            img1.LastCheck = DateTime.Now;
        }
    }
}