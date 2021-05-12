using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        const int MAXCANDIDATES = 100;
        private static readonly Tuple<string, string, Mat, Mat>[] _candidates = new Tuple<string, string, Mat, Mat>[MAXCANDIDATES];
        private static int _candidatescounter = 0;

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
                if (_imgList.Count < MAXCANDIDATES) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                for (var i = 0; i < MAXCANDIDATES; i++) {
                    if (_candidates[i] != null) {
                        if (_candidatescounter <= 0 || !_imgList.ContainsKey(_candidates[i].Item1)) {
                            _candidates[i].Item3.Dispose();
                            _candidates[i] = null;
                        }
                    }

                    if (_candidates[i] == null) {
                        var index = _random.Next(_imgList.Count);
                        var name = _imgList.ElementAt(i).Value.Name;
                        var hash = _imgList.ElementAt(i).Value.Hash;
                        var akazedescriptors = LoadAkazeDescriptors(name);
                        var akazemirrordescriptors = LoadAkazeMirrorDescriptors(name);
                        _candidates[i] = new Tuple<string, string, Mat, Mat>(name, hash, akazedescriptors, akazemirrordescriptors);
                    }
                }

                if (_candidatescounter <= 0) {
                    _candidatescounter = MAXCANDIDATES;
                }
                else {
                    _candidatescounter--;
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

                    if (img1 != null) {
                        if (img1.LastCheck <= eX.LastCheck) {
                            continue;
                        }
                    }

                    img1 = eX;
                }
            }

            using (var a1 = LoadAkazeDescriptors(img1.Name)) {
                var nexthash = img1.NextHash;
                var ncd = ulong.MaxValue;
                lock (_imglock) {
                    if (img1.NextHash.Equals(img1.Hash) || !_hashList.ContainsKey(img1.NextHash)) {
                        nexthash = img1.Hash;
                        if (img1.AkazePairs != 0) {
                            img1.AkazePairs = 0;
                        }

                        if (img1.Counter != 0) {
                            img1.Counter = 0;
                        }
                    }

                    foreach (var img in _imgList) {
                        if (img.Value.Name.Equals(img1.Name)) {
                            continue;
                        }

                        var c1 = ImageHelper.ComputeCentoidDistance(img1.AkazeCentroid, img.Value.AkazeCentroid);
                        var c2 = ImageHelper.ComputeCentoidDistance(img1.AkazeCentroid, img.Value.AkazeMirrorCentroid);
                        var cd = Math.Min(c1, c2);
                        if (cd < ncd) {
                            nexthash = img.Value.Hash;
                            ncd = cd;
                        }
                    }
                }

                if (ncd < ulong.MaxValue) {
                    lock (_imglock) {
                        if (_hashList.TryGetValue(nexthash, out var img2)) {
                            using (var a2 = LoadAkazeDescriptors(img2.Name))
                            using (var am2 = LoadAkazeMirrorDescriptors(img2.Name)) {
                                var nap2 = Math.Max(ImageHelper.ComputeAkazePairs(a1, a2), ImageHelper.ComputeAkazePairs(a1, am2));
                                if (nap2 > img1.AkazePairs) {
                                    var sb = new StringBuilder();
                                    sb.Append(GetNew());
                                    sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                                    sb.Append($"{img1.Id}: ");
                                    sb.Append($"[{img1.Counter}] ");
                                    sb.Append($"{img1.AkazePairs} ");
                                    sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                                    sb.Append($"[{img1.Counter + 1}] ");
                                    sb.Append($"{nap2} ");
                                    img1.AkazePairs = nap2;
                                    img1.NextHash = nexthash;
                                    img1.LastChanged = DateTime.Now;
                                    backgroundworker.ReportProgress(0, sb.ToString());
                                    img1.Counter += 1;
                                    img1.LastCheck = DateTime.Now;
                                    return;
                                }
                            }
                        }
                    }
                }

                var nap = img1.AkazePairs;
                for (var i = 0; i < _candidates.Length; i++) {
                    if (_candidates[i].Item1.Equals(img1.Name)) {
                        continue;
                    }

                    var p1 = ImageHelper.ComputeAkazePairs(a1, _candidates[i].Item3);
                    var p2 = ImageHelper.ComputeAkazePairs(a1, _candidates[i].Item4);
                    var nap2 = Math.Max(p1, p2);
                    if (nap2 > nap) {
                        nap = nap2;
                        nexthash = _candidates[i].Item2;
                    }
                }

                if (nap > img1.AkazePairs) {
                    lock (_imglock) {
                        if (_hashList.TryGetValue(nexthash, out var img2)) {
                            var sb = new StringBuilder();
                            sb.Append(GetNew());
                            sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                            sb.Append($"{img1.Id}: ");
                            sb.Append($"[{img1.Counter}] ");
                            sb.Append($"{img1.AkazePairs} ");
                            sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                            sb.Append($"[{img1.Counter + 1}] ");
                            sb.Append($"{nap} ");
                            img1.AkazePairs = nap;
                            img1.NextHash = nexthash;
                            img1.LastChanged = DateTime.Now;
                            backgroundworker.ReportProgress(0, sb.ToString());
                        }
                    }
                }
            }

            img1.Counter += 1;
            img1.LastCheck = DateTime.Now;
        }
    }
}