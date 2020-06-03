using System;
using System.Collections.Generic;
using System.IO;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Confirm(string name)
        {
            lock (_imglock) {
                AppVars.MoveMessage = string.Empty;
                if (_imgList.TryGetValue(name, out var img)) {
                    img.LastView = DateTime.Now;
                    img.Counter += 1;

                    if (img.Folder > 0) {
                        var cnt = new SortedDictionary<int, int>();
                        foreach (var e in _imgList) {
                            if (!cnt.ContainsKey(e.Value.Folder)) {
                                cnt.Add(e.Value.Folder, 1);
                            }
                            else {
                                cnt[e.Value.Folder]++;
                            }
                        }

                        var df = img.Folder - 1;
                        while (df >= 0) {
                            if (cnt[df] < AppConsts.MaxImagesInFolder) {
                                AppVars.MoveMessage =
                                    $"{img.Folder} [{cnt[img.Folder] - 1}] -> {df} [{cnt[df] + 1}]";

                                var oldfile = img.FileName;
                                img.Folder = df;
                                File.Move(oldfile, img.FileName);
                                break;
                            }

                            df--;
                        }
                    }
                }
            }
        }
    }
}
