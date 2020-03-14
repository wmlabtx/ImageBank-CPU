﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Find(int idX, int idY, IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            var dt = DateTime.Now;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (idX <= 0) {
                        var scope = _imgList
                            .Values
                            .Where(e => e.NextId > 0 && e.LastId > 0 && e.NextId != e.Id)
                            .ToArray();

                        if (scope.Length == 0) {
                            progress.Report("No images to view");
                            return;
                        }

                        var mingeneration = scope.Min(e => e.Generation);
                        scope = scope
                            .Where(e => e.Generation == mingeneration)
                            .OrderBy(e => e.Distance)
                            .ToArray();

                        var mingen = int.MaxValue;
                        var mindst = 256f;
                        foreach (var imgx in scope) {
                            if (!_imgList.TryGetValue(imgx.NextId, out var imgy)) {
                                continue;
                            }

                            if (imgy.Generation < mingen) {
                                mingen = imgy.Generation;
                                mindst = imgx.Distance;
                                idX = imgx.Id;
                            }
                            else {
                                if (imgy.Generation == mingen && imgx.Distance < mindst) {
                                    mindst = imgy.Distance;
                                    idX = imgx.Id;
                                }
                            }
                        }
                    }

                    if (!_imgList.TryGetValue(idX, out Img imgX)) {
                        Delete(idX);
                        idX = 0;
                        continue;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(idX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(idX);
                        idX = 0;
                        continue;
                    }

                    if (idY <= 0) {
                        idY = imgX.NextId;
                    }

                    if (!_imgList.TryGetValue(idY, out Img imgY)) {
                        Delete(idY);
                        imgX.NextId = 0;
                        imgX.LastId = 0;
                        idX = 0;
                        continue;
                    }

                    AppVars.ImgPanel[1] = GetImgPanel(idY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(idY);
                        imgX.NextId = 0;
                        imgX.LastId = 0;
                        idX = 0;
                        continue;
                    }

                    break;
                }
            }

            var sb = new StringBuilder(GetPrompt());
            var secs = DateTime.Now.Subtract(dt).TotalSeconds;
            sb.Append($"{secs:F2}s");
            progress.Report(sb.ToString());
        }
    }
}
