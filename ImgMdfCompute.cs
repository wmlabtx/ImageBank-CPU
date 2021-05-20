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
        private static readonly Tuple<Img, Mat, Mat>[] _candidates = new Tuple<Img, Mat, Mat>[MAXCANDIDATES];
        private static int _candidatescounter = 0;

        private static void UpdateCandidates()
        {
            for (var i = 0; i < MAXCANDIDATES; i++) {
                if (_candidates[i] != null) {
                    if (_candidatescounter <= 0 || !_imgList.ContainsKey(_candidates[i].Item1.Id)) {
                        _candidates[i].Item3.Dispose();
                        _candidates[i] = null;
                    }
                }

                if (_candidates[i] == null) {
                    var index = _random.Next(_imgList.Count);
                    var img = _imgList.ElementAt(index).Value;
                    var akazedescriptors = LoadAkazeDescriptors(img.Id);
                    var akazemirrordescriptors = LoadAkazeMirrorDescriptors(img.Id);
                    _candidates[i] = new Tuple<Img, Mat, Mat>(img, akazedescriptors, akazemirrordescriptors);
                }
            }

            if (_candidatescounter <= 0) {
                _candidatescounter = MAXCANDIDATES;
            }
            else {
                _candidatescounter--;
            }
        }

        private static string GetNew()
        {
            var newpics = _imgList.Count(e => e.Value.Hash.Equals(e.Value.NextHash));
            if (newpics == 0) {
                return string.Empty;
            }

            var message = $"{newpics} ";
            return message;
        }

        private static void Compute(Img img1, Mat ad1, Img pimg1, Mat pad1, Mat pamd1, BackgroundWorker backgroundworker)
        {
            var ap2 = ImageHelper.ComputeAkazePairs(ad1, pad1);
            var apm2 = ImageHelper.ComputeAkazePairs(ad1, pamd1);
            var ap = Math.Max(ap2, apm2);
            if (ap > img1.AkazePairs) {
                lock (_imglock) {
                    if (_imgList.ContainsKey(img1.Id) && _imgList.ContainsKey(pimg1.Id)) {
                        var sb = new StringBuilder();
                        sb.Append(GetNew());
                        sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                        sb.Append($"{img1.Id}: ");
                        sb.Append($"{img1.AkazePairs} ");
                        sb.Append($"{char.ConvertFromUtf32(0x2192)} ");
                        sb.Append($"{ap} ");
                        backgroundworker.ReportProgress(0, sb.ToString());

                        img1.NextHash = pimg1.Hash;
                        img1.AkazePairs = ap;
                        img1.LastChanged = DateTime.Now;
                    }
                }
            }
        }

        private static Img GetRecommendedToCompute()
        {
            Img img1 = null;
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
                }
                else {
                    if (eX.LastCheck < img1.LastCheck) {
                        img1 = eX;
                    }
                }
            }

            if (!_hashList.ContainsKey(img1.NextHash)) {
                img1.NextHash = img1.Hash;
                if (img1.AkazePairs > 0) {
                    img1.AkazePairs = 0;
                }
            }

            return img1;
        }

        public static void Compute(BackgroundWorker backgroundworker)
        {
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            Img img1 = null;
            Img pimg1 = null;
            lock (_imglock) {
                if (_imgList.Count < 2) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                UpdateCandidates();
                img1 = GetRecommendedToCompute();
            }

            var ncd = ulong.MaxValue;
            lock (_imglock) {
                foreach (var img in _imgList) {
                    if (img.Value.Id == img1.Id) {
                        continue;
                    }

                    var c1 = ImageHelper.ComputeCentoidDistance(img1.AkazeCentroid, img.Value.AkazeCentroid);
                    var c2 = ImageHelper.ComputeCentoidDistance(img1.AkazeCentroid, img.Value.AkazeMirrorCentroid);
                    var cd = Math.Min(c1, c2);
                    if (cd < ncd) {
                        pimg1 = img.Value;
                        ncd = cd;
                    }
                }
            }

            using (var ad1 = LoadAkazeDescriptors(img1.Id)) {
                if (!pimg1.Hash.Equals(img1.NextHash)) {
                    using (var pad1 = LoadAkazeDescriptors(pimg1.Id))
                    using (var pamd1 = LoadAkazeMirrorDescriptors(pimg1.Id)) {
                        Compute(img1, ad1, pimg1, pad1, pamd1, backgroundworker);
                    }
                }

                for (var i = 0; i < _candidates.Length; i++) {
                    if (_candidates[i].Item1.Id == img1.Id) {
                        continue;
                    }

                    Compute(img1, ad1, _candidates[i].Item1, _candidates[i].Item2, _candidates[i].Item3, backgroundworker);
                }
            }

            img1.LastCheck = DateTime.Now;
        }
    }
}