using OpenCvSharp;
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
        private static readonly Tuple<string, string, Mat>[] _candidates = new Tuple<string, string, Mat>[MAXCANDIDATES];
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
                        _candidates[i] = new Tuple<string, string, Mat>(name, hash, akazedescriptors);
                    }
                }

                if (_candidatescounter <= 0) {
                    _candidatescounter = 100;
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
                var npd = img1.PerceptiveDistance;
                var nap = img1.AkazePairs;
                var candidates = new List<Tuple<Img, int>>();
                lock (_imglock) {
                    if (img1.NextHash.Equals(img1.Hash) ||
                        !_hashList.ContainsKey(img1.NextHash) ||
                        img1.PerceptiveDistance == AppConsts.MaxPerceptiveDistance) {
                        nexthash = img1.Hash;
                        img1.PerceptiveDistance = AppConsts.MaxPerceptiveDistance;
                        img1.AkazePairs = 0;
                        npd = img1.PerceptiveDistance;
                        nap = img1.AkazePairs;
                    }

                    foreach (var img in _imgList) {
                        if (img.Value.Name.Equals(img1.Name)) {
                            continue;
                        }

                        var dip = ImageHelper.ComputePerceptiveDistance(img1.PerceptiveDescriptors, img.Value.PerceptiveDescriptors);
                        candidates.Add(new Tuple<Img, int>(img.Value, dip));
                    }

                    candidates = candidates.OrderBy(e => e.Item2).ToList();
                }

                if ((img1.PerceptiveDistance == AppConsts.MaxPerceptiveDistance || candidates[0].Item2 < AppConsts.MinPerceptiveDistance) && 
                    candidates[0].Item2 < img1.PerceptiveDistance) {
                    nexthash = candidates[0].Item1.Hash;
                    lock (_imglock) {
                        if (_hashList.TryGetValue(nexthash, out var img2)) {
                            using (var a2 = LoadAkazeDescriptors(img2.Name)) {
                                nap = ImageHelper.ComputeAkazePairs(a1, a2);
                                npd = candidates[0].Item2;
                                var sb = new StringBuilder();
                                sb.Append(GetNew());
                                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                                sb.Append($"{img1.Id}: ");
                                sb.Append($"{img1.PerceptiveDistance} ");
                                sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                                sb.Append($"{npd} ");
                                img1.PerceptiveDistance = npd;
                                img1.AkazePairs = nap;
                                img1.NextHash = nexthash;
                                img1.LastChanged = DateTime.Now;
                                img1.Counter = 0;
                                backgroundworker.ReportProgress(0, sb.ToString());
                            }
                        }
                    }

                    img1.LastCheck = DateTime.Now;
                    return;
                }

                for (var i = 0; i < _candidates.Length; i++) {
                    if (_candidates[i].Item1.Equals(img1.Name)) {
                        continue;
                    }

                    var nap2 = ImageHelper.ComputeAkazePairs(a1, _candidates[i].Item3);
                    if (nap2 > nap) {
                        nap = nap2;
                        nexthash = _candidates[i].Item2;
                    }
                }

                if (nap > img1.AkazePairs) {
                    lock (_imglock) {
                        if (_hashList.TryGetValue(nexthash, out var img2)) {
                            npd = ImageHelper.ComputePerceptiveDistance(img1.PerceptiveDescriptors, img2.PerceptiveDescriptors);
                            var sb = new StringBuilder();
                            sb.Append(GetNew());
                            sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                            sb.Append($"{img1.Id}: ");
                            sb.Append($"{img1.AkazePairs} ");
                            sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                            sb.Append($"{nap} ");
                            img1.PerceptiveDistance = npd;
                            img1.AkazePairs = nap;
                            img1.NextHash = nexthash;
                            img1.LastChanged = DateTime.Now;
                            if (img1.AkazePairs > AppConsts.MinAkazePairs) {
                                img1.Counter = 0;
                            }

                            backgroundworker.ReportProgress(0, sb.ToString());
                        }
                    }
                }
            }

            img1.LastCheck = DateTime.Now;
        }
    }
}