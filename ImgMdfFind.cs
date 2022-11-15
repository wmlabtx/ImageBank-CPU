using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Find(int idX, IProgress<string> progress)
        {
            Img imgX;
            int totalcount;
            do {
                totalcount = AppImgs.Count();
                if (totalcount < 2) {
                    progress?.Report($"totalcount = {totalcount}");
                    return;
                }

                if (idX == 0) {
                    idX = AppImgs.GetNextView().Id;
                }

                if (!AppPanels.SetImgPanel(0, idX)) {
                    Delete(idX);
                    progress?.Report($"{idX} deleted");
                    idX = 0;
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
