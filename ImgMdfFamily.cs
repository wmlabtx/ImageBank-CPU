using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public int FolderSize(string folder)
        {
            lock (_imglock) {
                var foldersize = _imgList.Count(e => folder.Equals(e.Value.Folder));
                return foldersize;
            }
        }

        public void AssignFolder(string folder)
        {
            if (AppVars.ImgPanel[0].Img.Folder.Equals(folder)) {
                return;
            }

            AppVars.ImgPanel[0].Img.Folder = folder;
            var candidates = new List<Img>();
            lock (_imglock)
            {
                if (!AppVars.ImgPanel[0].Img.Folder.Equals(AppConsts.FolderDefault)) {
                    foreach (var e in _imgList) {
                        if (!AppVars.ImgPanel[0].Img.Name.Equals(e.Key) && AppVars.ImgPanel[0].Img.Folder.Equals(e.Value.Folder)) {
                            candidates.Add(e.Value);
                        }
                    }
                }

                if (candidates.Count == 0) {
                    foreach (var e in _imgList) {
                        if (!AppVars.ImgPanel[0].Img.Name.Equals(e.Key)) {
                            candidates.Add(e.Value);
                        }
                    }
                }
            }

            var index = Random.Next(candidates.Count);
            var imgY = candidates[index];

            AppVars.ImgPanel[0].Img.NextHash = imgY.Hash;
            AppVars.ImgPanel[0].Img.Counter = 1;
            AppVars.ImgPanel[0].Img.Distance = ImageHelper.GetDistance(
                AppVars.ImgPanel[0].Img.GetDescriptors(), 
                imgY.GetDescriptors(), 
                AppVars.ImgPanel[0].Img.Counter);

            AppVars.ImgPanel[0].Img.LastCheck = GetMinLastCheck();
            AppVars.ImgPanel[1] = GetImgPanel(imgY.Name);
        }

        public static void AssignFolderLeft()
        {
            if (AppVars.ImgPanel[0].Img.Folder.Equals(AppVars.ImgPanel[1].Img.Folder)) {
                return;
            }

            AppVars.ImgPanel[0].Img.Folder = AppVars.ImgPanel[1].Img.Folder;
        }
    }
}
