using System;
using System.IO;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Move(string folder)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(AppVars.ImgPanel[0].Id, out var img)) {
                    var oldfilename = img.FileName;
                    img.Folder = folder;
                    var newfilename = img.FileName;
                    File.Move(oldfilename, newfilename);
                    img.Distance = 256f;
                    img.NextId = string.Empty;
                    if (FindNext(AppVars.ImgPanel[0].Id, out var nextid, out var distance)) {
                        img.NextId = nextid;
                        img.Distance = distance;
                        img.LastCheck = DateTime.Now;
                    }

                    ResetRefers(AppVars.ImgPanel[0].Id);
                    AppVars.ImgPanel[0] = GetImgPanel(AppVars.ImgPanel[0].Id);
                }
            }
        }
    }
}
