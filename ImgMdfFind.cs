using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Find(string idX, string idY, IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            var dt = DateTime.Now;
            lock (_imglock) {
                while (true) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }

                    if (string.IsNullOrEmpty(idX)) {
                        
                        /*
                        var mincounter = int.MaxValue;
                        var minlv = DateTime.MaxValue;
                        foreach (var e in _imgList) {
                            if (string.IsNullOrEmpty(e.Value.NextId) || 
                                e.Value.Id.Equals(e.Value.NextId, StringComparison.OrdinalIgnoreCase) ||
                                !_imgList.TryGetValue(e.Value.NextId, out var imgy)) {
                                continue;
                            }

                            if (e.Value.Counter < mincounter) {
                                idX = e.Value.Id;
                                mincounter = e.Value.Counter;
                                minlv = e.Value.LastView;
                            }
                            else {
                                if (e.Value.Counter == mincounter) {
                                    if (!_imgList.TryGetValue(e.Value.NextId, out Img imgnext)) {
                                        continue;
                                    }

                                    if (imgnext.LastView < minlv) {
                                        idX = e.Value.Id;
                                        minlv = imgnext.LastView;
                                    }
                                }
                            }
                        }
                        */
                        
                        /*
                        var mincounter = int.MaxValue;
                        var mind = float.MaxValue;
                        var minlv = DateTime.MaxValue; 
                        foreach (var e in _imgList) {
                            if (string.IsNullOrEmpty(e.Value.NextId) || !_imgList.TryGetValue(e.Value.NextId, out var imgy)) {
                                continue;
                            }

                            if (e.Value.Counter < mincounter) {
                                idX = e.Value.Id;
                                mincounter = e.Value.Counter;
                                mind = e.Value.Distance;
                                minlv = e.Value.LastView;
                            }
                            else {
                                if (e.Value.Counter == mincounter) {
                                    if (e.Value.Distance < mind) {
                                        idX = e.Value.Id;
                                        mind = e.Value.Distance;
                                        minlv = e.Value.LastView;
                                    }
                                    else {
                                        if (e.Value.Distance == mind) {
                                            if (e.Value.LastView < minlv) {
                                                idX = e.Value.Id;
                                                minlv = e.Value.LastView;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        */

                        var minlv = DateTime.MaxValue;
                        foreach (var e in _imgList) {
                            if (string.IsNullOrEmpty(e.Value.NextId) || 
                                e.Value.NextId.Equals(e.Value.Id, StringComparison.OrdinalIgnoreCase) || 
                                !_imgList.TryGetValue(e.Value.NextId, out var imgy)) {
                                continue;
                            }

                            if (e.Value.LastView < minlv) {
                                idX = e.Value.Id;
                                minlv = e.Value.LastView;
                            }
                        }

                        if (string.IsNullOrEmpty(idX)) {
                            progress.Report("No images to view");
                            return;
                        }
                    }

                    if (!_imgList.TryGetValue(idX, out Img imgX)) {
                        Delete(idX);
                        progress.Report($"{idX} deleted");
                        idX = string.Empty;
                        idY = string.Empty;
                        continue;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(idX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(idX);
                        progress.Report($"{idX} deleted");
                        idX = string.Empty;
                        idY = string.Empty;
                        continue;
                    }

                    if (string.IsNullOrEmpty(idY)) {
                        idY = imgX.NextId;
                    }

                    if (!_imgList.TryGetValue(idY, out Img imgY)) {
                        Delete(idY);
                        progress.Report($"{idY} deleted");
                        imgX.NextId = string.Empty;
                        idX = string.Empty;
                        idY = string.Empty;
                        continue;
                    }

                    AppVars.ImgPanel[1] = GetImgPanel(idY);
                    if (AppVars.ImgPanel[1] == null) {
                        Delete(idY);
                        progress.Report($"{idY} deleted");
                        imgX.NextId = string.Empty;
                        idX = string.Empty;
                        idY = string.Empty;
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
