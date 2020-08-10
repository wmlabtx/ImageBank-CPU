using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Compute(BackgroundWorker backgroundworker)
        {
            Contract.Requires(backgroundworker != null);

            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            lock (_imglock) {
                if (_imgList.Count == 0) {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }
            }

            var idX = GetNextToCheck();
            Img imgX;
            lock (_imglock) {
                if (!_imgList.TryGetValue(idX, out imgX)) {
                    backgroundworker.ReportProgress(0, $"error getting {idX}");
                    return;
                }
            }

            if (!File.Exists(imgX.FileName)) {
                Delete(idX);
                backgroundworker.ReportProgress(0, $"{idX} deleted");
                return;
            }

            var pX = Array.Empty<string>();
            if (!string.IsNullOrEmpty(imgX.Path)) {
                pX = imgX.Path.Split('.');
            }

            var candidates = new List<Img>();

            bool issubpath;
            lock (_imglock) {
                foreach (var e in _imgList) {
                    if (e.Value.Name.Equals(imgX.Name, StringComparison.Ordinal)) {
                        continue;
                    }

                    issubpath = true;
                    if (pX.Length > 0 && !string.IsNullOrEmpty(e.Value.Path)) {
                        var pY = e.Value.Path.Split('.');
                        if (pY.Length >= pX.Length) {
                            for (var i = 0; i < pX.Length; i++) {
                                if (!pX[i].Equals(pY[i], StringComparison.OrdinalIgnoreCase)) {
                                    issubpath = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (!issubpath) {
                        continue;
                    }

                    candidates.Add(e.Value);
                }
            }

            var dt = imgX.Dt;
            var dv = 1f;
            var nextname = string.Empty;
            foreach (var e in candidates) {
                var h = Intrinsic.PopCnt(imgX.PHash ^ e.PHash) / 64f;
                if (h < AppConsts.MaxHamming && h < dv) {
                    dt = "H";
                    dv = h;
                    nextname = e.Name;

                    if (h < 0.0001 && imgX.Width == e.Width && imgX.Heigth == e.Heigth) {
                        if (imgX.Size > e.Size) {
                            Delete(e.Name);
                        }
                        else {
                            Delete(imgX.Name);
                        }

                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(nextname)) {
                var orbcandidates = new List<Tuple<Img, float>>();
                dv = float.MaxValue;
                var isbw = imgX.Scd.IsBw();
                foreach (var e in candidates) {
                    var s = imgX.Scd.GetDistance(e.Scd);
                    if (isbw < AppConsts.BwLimit && s < AppConsts.MaxScd && s < dv) {
                        dt = "S";
                        dv = s;
                        nextname = e.Name;
                    }
                    else { 
                        orbcandidates.Add(new Tuple<Img, float>(e, s));
                    }
                }

                if (string.IsNullOrEmpty(nextname)) {
                    orbcandidates = orbcandidates.OrderBy(e => e.Item2).Take(AppConsts.OrbHorizon).ToList();
                    int[] dmatch = null;
                    var descriptorsX = imgX.GetDescriptors();
                    foreach (var e in orbcandidates) {
                        var ematch = OrbHelper.GetMatches(descriptorsX, e.Item1.GetDescriptors());
                        if (dmatch == null) {
                            dmatch = ematch;
                            nextname = e.Item1.Name;
                        }
                        else {
                            var len = Math.Min(dmatch.Length, ematch.Length);
                            for (var i = 0; i < len; i++) {
                                if (ematch[i] < dmatch[i]) {
                                    dmatch = ematch;
                                    nextname = e.Item1.Name;
                                    break;
                                }
                                else {
                                    if (ematch[i] > dmatch[i]) {
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (dmatch != null) {
                        dt = "R";
                        dv = (float)dmatch.Sum() / dmatch.Length;
                    }
                }
            }

            if (string.IsNullOrEmpty(nextname)) {
                return;
            }

            Img imgY;
            lock (_imglock) {
                if (!_imgList.TryGetValue(nextname, out imgY)) {
                    backgroundworker.ReportProgress(0, $"error getting {nextname}");
                    return;
                }
            }

            if (!File.Exists(imgY.FileName)) {
                Delete(nextname);
                backgroundworker.ReportProgress(0, $"{nextname} deleted");
                return;
            }

            var sb = new StringBuilder();
            sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastCheck))} ago] ");
            sb.Append($"{imgX.Folder:D2}\\{imgX.Name}: ");
            if (
                (!dt.Equals(imgX.Dt, StringComparison.OrdinalIgnoreCase)) ||
                (Math.Abs(dv - imgX.Dv) > 0.0001) ||
                (!nextname.Equals(imgX.NextName, StringComparison.OrdinalIgnoreCase))
                ) {
                sb.Append($"[{dt}] {dv:F4} ");
                imgX.Dt = dt;
                imgX.Dv = dv;
                imgX.NextName = nextname;
            }
            else {
                sb.Append($"no changes");
            }

            imgX.LastCheck = DateTime.Now;

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}