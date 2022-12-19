using System;
using System.Collections.Generic;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Find(string hashX, IProgress<string> progress)
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

                if (!AppPanels.SetImgPanel(1, imgX.BestHash)) {
                    Delete(imgX.BestHash, progress);
                    progress?.Report($"{imgX.BestHash} deleted");
                    hashX = null;
                    continue;
                }

                var similar = new List<Tuple<string, float>> {
                    Tuple.Create(imgX.BestHash, imgX.Distance)
                };

                AppPanels.SetSimilars(similar, progress);
                //var similars = GetSimilars(imgX, progress);
                //AppPanels.SetSimilars(similars, progress);
                break;
            }
            while (true);
        }
    }
}
