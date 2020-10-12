using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Confirm()
        {
            lock (_imglock) {
                AppVars.MoveMessage = string.Empty;
                AppVars.ImgPanel[0].Img.LastView = DateTime.Now;
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
                        var maxlv = _imgList.Where(e => e.Value.Folder == df + 1).Max(e => e.Value.LastView);
                        var img = _imgList.FirstOrDefault(e => e.Value.Folder == df + 1 && e.Value.LastView == maxlv).Value;
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
                for (var df = 98; df >= 0; df--) {
                    if (c[df] < AppConsts.MaxImagesInFolder) {
                        var sf = df + 1;
                        if (c[sf] > 0) {
                            var minla = _imgList.Where(e => e.Value.Folder == sf).Min(e => e.Value.LastAdded);
                            var img = _imgList.FirstOrDefault(e => e.Value.Folder == sf && e.Value.LastAdded == minla).Value;
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