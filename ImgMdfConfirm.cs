using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Confirm(int index)
        {
            lock (_imglock) {
                AppVars.MoveMessage = string.Empty;
                AppVars.ImgPanel[index].Img.LastView = DateTime.Now;
                AppVars.ImgPanel[index].Img.Counter = 1;
            }
        }

        public void Pack()
        {
            lock (_imglock) {
                var c = new int[100];
                foreach (var e in _imgList) {
                    c[e.Value.Folder]++;
                }

                AppVars.MoveMessage = string.Empty;
                var moved = 0;
                for (var df = 1; df <= 99; df++) {
                    if (c[df - 1] < AppConsts.MaxImagesInFolder && c[df] > 0)
                    {
                        var img = _imgList
                            .Where(e => e.Value.Folder == df)
                            .OrderBy(e => e.Value.LastAdded)
                            .FirstOrDefault()
                            .Value;
                        
                        c[df - 1]++;
                        c[df]--;
                        moved++;
                        AppVars.MoveMessage = $"{df} [{c[df]}] -> {df - 1} [{c[df - 1]}] ({moved}) ";
                        var oldfile = img.FileName;
                        img.Folder = df - 1;
                        File.Move(oldfile, img.FileName);
                    }
                }
            }
            
            /*
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
            */
        }
    }
}