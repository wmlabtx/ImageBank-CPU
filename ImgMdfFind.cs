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
        public void Find(int idX, IProgress<string> progress)
        {
            Contract.Requires(progress != null);
            var idY = 0;
            Img imgX;

            var dt = DateTime.Now;
            while (true) {
                lock (_imglock) {
                    if (_imgList.Count < 2) {
                        progress.Report("No images to view");
                        return;
                    }
                }

                if (idX <= 0) {
                    lock (_imglock) {
                        var sourcefolder = AppConsts.PathCollection;
                        var currentfolder = sourcefolder;
                        while (true) {
                            var directoryinfo = new DirectoryInfo(currentfolder);
                            var dirinfos = directoryinfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly).ToArray();
                            if (dirinfos.Length == 0) {
                                break;
                            }

                            var lastwritetime = dirinfos.Min(e => e.LastWriteTime);
                            currentfolder = dirinfos.Where(e => e.LastWriteTime == lastwritetime).Select(e => e.FullName).First();
                            try {
                                Directory.SetLastWriteTime(currentfolder, DateTime.Now);
                            }
                            catch (IOException) {
                            }
                        }

                        var prefix = currentfolder.Substring(AppConsts.PathCollection.Length).Replace('\\', '.');
                        var lv = new SortedDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                        foreach (var xp in _imgList) {
                            if (xp.Value.NextId <= 0 || xp.Value.LastId <= 0 || xp.Value.NextId == xp.Value.Id) {
                                continue;
                            }

                            if (!xp.Value.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                                continue;
                            }

                            if (lv.ContainsKey(xp.Value.Person)) {
                                if (xp.Value.LastView > lv[xp.Value.Person]) {
                                    lv[xp.Value.Person] = xp.Value.LastView;
                                }
                            }
                            else {
                                lv.Add(xp.Value.Person, xp.Value.LastView);
                            }
                        }

                        if (lv.Count == 0) {
                            continue;
                        }

                        var person = lv.Aggregate((m, e) => e.Value < m.Value ? e : m).Key;
                        var lvmin = long.MaxValue;
                        foreach (var xp in _imgList) {
                            if (!xp.Value.Person.Equals(person, StringComparison.OrdinalIgnoreCase)) {
                                continue;
                            }

                            if (xp.Value.NextId <= 0 || xp.Value.LastId <= 0 || xp.Value.NextId == xp.Value.Id) {
                                continue;
                            }

                            if (_imgList.TryGetValue(xp.Value.NextId, out var imgb)) {
                                var lvmax = Math.Max(xp.Value.LastView.Ticks, imgb.LastView.Ticks);
                                if (lvmax < lvmin) {
                                    lvmin = lvmax;
                                    idX = xp.Value.Id;
                                    idY = imgb.Id;
                                }
                            }
                        }

                        if (!_imgList.TryGetValue(idX, out imgX)) {
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
                    }
                }
                else {
                    lock (_imglock) {
                        if (!_imgList.TryGetValue(idX, out imgX)) {
                            Delete(idX);
                            idX = 0;
                            continue;
                        }
                    }

                    if (imgX.NextId <= 0) {
                        idX = 0;
                        continue;
                    }

                    AppVars.ImgPanel[0] = GetImgPanel(idX);
                    if (AppVars.ImgPanel[0] == null) {
                        Delete(idX);
                        idX = 0;
                        continue;
                    }

                    idY = imgX.NextId;
                }

                lock (_imglock) {
                    if (!_imgList.TryGetValue(idY, out Img imgY)) {
                        Delete(idY);
                        imgX.NextId = 0;
                        imgX.LastId = 0;
                        idX = 0;
                        continue;
                    }
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

            var sb = new StringBuilder(GetPrompt());
            var secs = DateTime.Now.Subtract(dt).TotalSeconds;
            sb.Append($"{secs:F2}s");
            progress.Report(sb.ToString());
        }
    }
}
