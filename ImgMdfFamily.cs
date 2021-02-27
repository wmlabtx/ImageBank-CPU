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
                var foldersize = folder.StartsWith(AppConsts.FolderDefault) ?
                    _imgList.Count(e => e.Value.Folder.StartsWith(AppConsts.FolderDefault)) : 
                    _imgList.Count(e => folder.Equals(e.Value.Folder));

                return foldersize;
            }
        }

        public void AssignFolder(string folder)
        {
            var img1 = AppVars.ImgPanel[0].Img;

            if (img1.Folder.Equals(folder)) {
                return;
            }

            img1.Folder = folder;
            FindCandidates(img1, out List<Tuple<string, ulong, ulong[]>> candidates);
            if (candidates.Count > 0)
            {
                var fast = img1.Folder.StartsWith(AppConsts.FolderDefault);
                FindCandidate(img1, candidates, fast, out var name2);
                lock (_imglock)
                {
                    if (_imgList.TryGetValue(name2, out var img2))
                    {
                        var distance = ImageHelper.CompareBlob(img1.GetDescriptors(), img2.GetDescriptors());
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
