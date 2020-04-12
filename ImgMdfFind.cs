using System;
using System.Diagnostics.Contracts;
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
                        var mincounter = int.MaxValue;
                        var mind = int.MaxValue;
                        var minlv = DateTime.MaxValue; 
                        foreach (var e in _imgList) {
                            if (e.Value.NextId <= 0 || !_imgList.TryGetValue(e.Value.NextId, out var imgy)) {
                                continue;
                            }

                            if (e.Value.Counter < mincounter) {
                                idX = e.Value.Id;
                                mincounter = e.Value.Counter;
                                mind = (int)e.Value.Distance;
                                minlv = e.Value.LastView;
                            }
                            else {
                                if (e.Value.Counter == mincounter) {
                                    if ((int)e.Value.Distance < mind) {
                                        idX = e.Value.Id;
                                        mind = (int)e.Value.Distance;
                                        minlv = e.Value.LastView;
                                    }
                                    else {
                                        if ((int)e.Value.Distance == mind) {
                                            if (e.Value.LastView < minlv) {
                                                idX = e.Value.Id;
                                                minlv = e.Value.LastView;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (idX <= 0) {
                            progress.Report("No images to view");
                            return;
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
                        idX = 0;
                        continue;
                    }

                    AppVars.ImgPanel[1] = GetImgPanel(idY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(idY);
                        imgX.NextId = 0;
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
