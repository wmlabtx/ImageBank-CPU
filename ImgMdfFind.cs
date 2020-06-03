using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Find(string nameX, string nameY, IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            var dt = DateTime.Now;
            //var distance = 0;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }



                    if (string.IsNullOrEmpty(nameX)) {
                        var lastf = 99;
                        var fc = new int[100];
                        foreach (var img in _imgList) {
                            fc[img.Value.Folder]++;
                        }

                        while (fc[lastf] == 0 && lastf > 0) {
                            lastf--;
                        }

                        var docompress = false;
                        /*
                        if (lastf > 0) {
                            for (var i = 0; i < lastf; i++) {
                                if (fc[i] < AppConsts.MaxImagesInFolder) {
                                    docompress = true;
                                    break;
                                }
                            }
                        }
                        */ 

                        var df = lastf;
                        if (!docompress) {
                            var minct = DateTime.MaxValue;
                            string mindp = null;
                            for (var i = 0; i < 99; i++) {
                                var dp = $"{AppConsts.PathHp}{i:D2}";
                                if (Directory.Exists(dp)) {
                                    var ct = Directory.GetLastWriteTime(dp);
                                    if (ct < minct) {
                                        minct = ct;
                                        mindp = dp;
                                        df = i;
                                    }
                                }
                            }

                            try {
                                Directory.SetLastWriteTime(mindp, DateTime.Now);
                            }
                            catch (IOException) {
                            }
                        }

                        var minlv = _imgList.Where(e => e.Value.Folder == df).Min(e => e.Value.LastView);
                        nameX = _imgList.Where(e => e.Value.Folder == df).FirstOrDefault(e => e.Value.LastView == minlv).Key;
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

                        nameY = nameX;
                        var candidates = new List<Tuple<string, ulong[], float>>();
                        var descriptorsX = AppVars.ImgPanel[0].Img.GetDescriptors();
                        var scdX = AppVars.ImgPanel[0].Img.Scd;
                        var pathX = AppVars.ImgPanel[0].Img.Path;
                        int[] dmatch = null;

                        if (string.IsNullOrEmpty(pathX)) {
                            foreach (var e in _imgList) {
                                if (!e.Value.Name.Equals(nameX, StringComparison.OrdinalIgnoreCase)) {
                                    candidates.Add(new Tuple<string, ulong[], float>(e.Value.Name, e.Value.GetDescriptors(), scdX.GetDistance(e.Value.Scd)));
                                }
                            }
                        }
                        else {
                            var pX = pathX.Split('.');
                            foreach (var e in _imgList) {
                                if (!e.Value.Name.Equals(nameX, StringComparison.OrdinalIgnoreCase)) {
                                    if (!string.IsNullOrEmpty(e.Value.Path)) {
                                        var pY = e.Value.Path.Split('.');
                                        if (pY.Length >= pX.Length) {
                                            var issubpath = true;
                                            for (var i = 0; i < pX.Length; i++) {
                                                if (!pX[i].Equals(pY[i], StringComparison.OrdinalIgnoreCase)) {
                                                    issubpath = false;
                                                    break;
                                                }
                                            }

                                            if (issubpath) {
                                                candidates.Add(new Tuple<string, ulong[], float>(e.Value.Name, e.Value.GetDescriptors(), scdX.GetDistance(e.Value.Scd)));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        candidates = candidates.OrderBy(e => e.Item3).Take(AppConsts.MaxOrbScope).ToList();

                        var index = 0;
                        while (index < candidates.Count) {
                            var ematch = OrbHelper.GetMatches(descriptorsX, candidates[index].Item2);
                            if (dmatch == null) {
                                dmatch = ematch;
                                nameY = candidates[index].Item1;
                            }
                            else {
                                var len = Math.Min(dmatch.Length, ematch.Length);
                                for (var i = 0; i < len; i++) {
                                    if (ematch[i] < dmatch[i]) {
                                        dmatch = ematch;
                                        nameY = candidates[index].Item1;
                                        break;
                                    }
                                    else {
                                        if (ematch[i] > dmatch[i]) {
                                            break;
                                        }
                                    }
                                }
                            }

                            index++;
                        }

                        AppVars.ImgPanel[1] = GetImgPanel(nameY);
                        if (AppVars.ImgPanel[1] == null) {
                            Delete(nameY);
                            progress.Report($"{nameY} deleted");
                            nameY = string.Empty;
                            continue;
                        }

                        AppVars.ImgPanelDistance = Intrinsic.PopCnt(AppVars.ImgPanel[0].Img.PHash ^ AppVars.ImgPanel[1].Img.PHash);
                        if (AppVars.ImgPanelDistance <= AppConsts.MaxHamming) {
                            if (AppVars.ImgPanel[0].Bitmap.Width == AppVars.ImgPanel[1].Bitmap.Width && AppVars.ImgPanel[0].Bitmap.Height == AppVars.ImgPanel[1].Bitmap.Height) {
                                if (AppVars.ImgPanel[0].Img.Size > AppVars.ImgPanel[1].Img.Size) {
                                    Delete(nameY);
                                    progress.Report($"{Helper.GetShortName(AppVars.ImgPanel[1].Img)} deleted");
                                    nameY = string.Empty;
                                    continue;
                                }
                                else {
                                    Delete(nameX);
                                    progress.Report($"{Helper.GetShortName(AppVars.ImgPanel[0].Img)} deleted");
                                    nameX = string.Empty;
                                    continue;
                                }
                            }
                        }

                        break;
                    }

                    if (!string.IsNullOrEmpty(nameX)) {
                        break;
                    }
                }
            }

            var imgX = _imgList[nameX];

            var sb = new StringBuilder(GetPrompt());
            var secs = DateTime.Now.Subtract(dt).TotalSeconds;
            sb.Append($"{AppVars.MoveMessage} ");
            sb.Append($"D{AppVars.ImgPanelDistance} ");
            sb.Append($"({secs:F2}s)");
            progress.Report(sb.ToString());
        }
    }
}
