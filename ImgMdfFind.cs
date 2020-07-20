using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private bool _findtrigger;

        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            var dt = DateTime.Now;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(nameX)) {
                        DateTime minlv;
                        if (_findtrigger) {
                            minlv = _imgList.Min(e => e.Value.LastView);
                        }
                        else {
                            minlv = _imgList.Where(e => DateTime.Now.Subtract(e.Value.LastView).TotalDays > 365).Max(e => e.Value.LastView);
                        }

                        nameX = _imgList.FirstOrDefault(e => e.Value.LastView == minlv).Key;
                        _findtrigger = !_findtrigger;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(nameX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(nameX);
                        progress.Report($"{nameX} deleted");
                        nameX = string.Empty;
                        continue;
                    }

                    while (true) {
                        if (string.IsNullOrEmpty(nameX)) {
                            break;
                        }

                        nameY = string.Empty;
                        var imgX = AppVars.ImgPanel[0].Img;
                        var pX = Array.Empty<string>();
                        if (!string.IsNullOrEmpty(imgX.Path)) {
                            pX = imgX.Path.Split('.');
                        }

                        var candidates = new List<Img>();

                        bool issubpath;
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

                        var descriptorsX = AppVars.ImgPanel[0].Img.GetDescriptors();
                        AppVars.ImgPanelHammingDistance = int.MaxValue;
                        foreach (var e in candidates) {
                            var d = Intrinsic.PopCnt(imgX.PHash ^ e.PHash);
                            if (d < AppConsts.MaxHamming && d < AppVars.ImgPanelHammingDistance) {
                                if (d == 0 && imgX.Width == e.Width && imgX.Heigth == e.Heigth) {
                                    if (imgX.Size > e.Size) {
                                        Delete(e.Name);
                                        progress.Report($"{e.Name} deleted");
                                        nameY = string.Empty;
                                        continue;
                                    }
                                    else {
                                        Delete(nameX);
                                        progress.Report($"{nameX} deleted");
                                        nameX = string.Empty;
                                        break;
                                    }
                                }

                                nameY = e.Name;
                                AppVars.ImgPanelHammingDistance = d;
                                AppVars.ImgPanelScdDistance = imgX.Scd.GetDistance(e.Scd);
                            }
                        }

                        if (string.IsNullOrEmpty(nameX)) {
                            break;
                        }

                        if (string.IsNullOrEmpty(nameY)) {
                            var orbcandidates = new List<Tuple<Img, float>>();
                            AppVars.ImgPanelScdDistance = float.MaxValue;
                            foreach (var e in candidates) {
                                var s = imgX.Scd.GetDistance(e.Scd);
                                if (s < AppConsts.MaxScd && s < AppVars.ImgPanelScdDistance) {
                                    nameY = e.Name;
                                    AppVars.ImgPanelHammingDistance = Intrinsic.PopCnt(imgX.PHash ^ e.PHash);
                                    AppVars.ImgPanelScdDistance = s;
                                }
                                else {
                                    orbcandidates.Add(new Tuple<Img, float>(e, s));
                                }
                            }

                            if (string.IsNullOrEmpty(nameY)) {
                                orbcandidates = orbcandidates.OrderBy(e => e.Item2).Take(AppConsts.OrbHorizon).ToList();
                                int[] dmatch = null;
                                foreach (var e in orbcandidates) {
                                    var ematch = OrbHelper.GetMatches(descriptorsX, e.Item1.GetDescriptors());
                                    if (dmatch == null) {
                                        nameY = e.Item1.Name;
                                        AppVars.ImgPanelHammingDistance = Intrinsic.PopCnt(imgX.PHash ^ e.Item1.PHash);
                                        AppVars.ImgPanelScdDistance = imgX.Scd.GetDistance(e.Item1.Scd); ;
                                        dmatch = ematch;
                                    }
                                    else {
                                        var len = Math.Min(dmatch.Length, ematch.Length);
                                        for (var i = 0; i < len; i++) {
                                            if (ematch[i] < dmatch[i]) {
                                                nameY = e.Item1.Name;
                                                AppVars.ImgPanelHammingDistance = Intrinsic.PopCnt(imgX.PHash ^ e.Item1.PHash);
                                                AppVars.ImgPanelScdDistance = imgX.Scd.GetDistance(e.Item1.Scd); ; ;
                                                dmatch = ematch;
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
                            }
                        }

                        AppVars.ImgPanel[1] = GetImgPanel(nameY);
                        if (AppVars.ImgPanel[1] == null) {
                            Delete(nameY);
                            progress.Report($"{nameY} deleted");
                            nameY = string.Empty;
                            continue;
                        }

                        break;
                    }

                    if (!string.IsNullOrEmpty(nameX)) {
                        break;
                    }
                }
            }

            var sb = new StringBuilder(GetPrompt());
            var secs = DateTime.Now.Subtract(dt).TotalSeconds;
            sb.Append($"{AppVars.MoveMessage} ");
            sb.Append($"D:{AppVars.ImgPanelHammingDistance} ");
            sb.Append($"S:{AppVars.ImgPanelScdDistance:F2} ");
            sb.Append($"({secs:F2}s)");
            progress.Report(sb.ToString());
        }
    }
}
