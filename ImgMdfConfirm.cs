using System;
using System.IO;
using System.Linq;

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
                }
            }
        }

        public void Pack()
        {
            /*
            lock (_imglock) {
                var c = new int[100];
                foreach (var e in _imgList) {
                    c[e.Value.Folder]++;
                }

                AppVars.MoveMessage = string.Empty;
                var moved = 0;
                for (var df = 0; df < 98; df++) {
                    if (c[df] < AppConsts.MaxImagesInFolder && c[df + 1] > 0) {
                        var minla = _imgList.Where(e => e.Value.Folder == df + 1).Min(e => e.Value.LastAdded);
                        var img = _imgList.FirstOrDefault(e => e.Value.Folder == df + 1 && e.Value.LastAdded == minla).Value;
                        c[df]++;
                        c[df + 1]--;
                        moved++;
                        AppVars.MoveMessage = $"{img.Folder} [{c[df + 1]}] -> {df} [{c[df]}] ({moved}) ";
                        var oldfile = img.FileName;
                        img.Folder = df;
                        File.Move(oldfile, img.FileName);
                    }
                }
            }
            */
            lock (_imglock) {
                var c = new int[100];
                foreach (var e in _imgList) {
                    c[e.Value.Folder]++;
                }

                AppVars.MoveMessage = string.Empty;
                for (var df = 0; df <= 98; df++) {
                    if (c[df] < AppConsts.MaxImagesInFolder) {
                        for (var sf = 99; sf > df; sf--) {
                            if (c[sf] > 0) {
                                var minla = _imgList.Where(e => e.Value.Folder == sf).Min(e => e.Value.LastView);
                                var img = _imgList.FirstOrDefault(e => e.Value.Folder == sf && e.Value.LastView == minla).Value;
                                c[df]++;
                                c[sf]--;
                                AppVars.MoveMessage = $"{sf} [{c[sf]}] -> {df} [{c[df]}] ";
                                var oldfile = img.FileName;
                                img.Folder = df;
                                File.Move(oldfile, img.FileName);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}