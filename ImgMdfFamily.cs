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
            var candidates = new List<string>();
            lock (_imglock)
            {
                if (!AppVars.ImgPanel[0].Img.Folder.Equals(AppConsts.FolderDefault)) {
                    foreach (var e in _imgList) {
                        if (!AppVars.ImgPanel[0].Img.Name.Equals(e.Key) && AppVars.ImgPanel[0].Img.Folder.Equals(e.Value.Folder)) {
                            candidates.Add(e.Value.Name);
                        }
                    }
                }

                if (candidates.Count == 0) {
                    foreach (var e in _imgList) {
                        if (!AppVars.ImgPanel[0].Img.Name.Equals(e.Key)) {
                            candidates.Add(e.Value.Name);
                        }
                    }
                }
            }

            if (candidates.Count > 0)
            {
                lock (_imglock)
                {
                    var name2 = candidates[0];
                    if (_imgList.TryGetValue(name2, out var img2))
                    {
                        var distance = ImageHelper.CompareBlob(
                            AppVars.ImgPanel[0].Img.GetMapDescriptors(), 
                            AppVars.ImgPanel[0].Img.GetDescriptors(),
                            img2.GetMapDescriptors(),
                            img2.GetDescriptors());

                        AppVars.ImgPanel[0].Img.NextHash = img2.Hash;
                        AppVars.ImgPanel[0].Img.Distance = distance;
                        AppVars.ImgPanel[1] = GetImgPanel(name2);
                    }
                }
            }
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
