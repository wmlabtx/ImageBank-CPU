using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Find(string hashX, IProgress<string> progress)
        {
            for (var i = 0; i < 10; i++) {
                ComputeInternal(progress);
            }

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
                var similars = GetSimilars(imgX, progress);
                AppPanels.SetSimilars(similars, progress);
                break;
            }
            while (true);
        }
    }
}
