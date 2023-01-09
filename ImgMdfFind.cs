using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Find(string hashX, bool findfamilies, IProgress<string> progress)
        {
            Img imgX;
            int totalcount;
            do {
                totalcount = AppImgs.Count();
                if (totalcount < 2) {
                    progress?.Report($"totalcount = {totalcount}");
                    return;
                }

                if (hashX == null) {
                    imgX = AppImgs.GetNextView();
                    if (imgX == null) {
                        return;
                    }

                    hashX = imgX.Hash;
                }

                if (!AppPanels.SetImgPanel(0, hashX)) {
                    Delete(hashX, progress);
                    progress?.Report($"{hashX} deleted");
                    hashX = null;
                    continue;
                }

                imgX = AppPanels.GetImgPanel(0).Img;
                var similars = GetSimilars(imgX, findfamilies, progress);
                AppPanels.SetSimilars(similars, progress);
                break;
            }
            while (true);
        }
    }
}
