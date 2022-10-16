using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Find(int idX, IProgress<string> progress)
        {
            int scopecount = 0;
            Img imgX;
            int totalcount;
            do {
                totalcount = AppImgs.Count();
                if (totalcount < 2) {
                    progress?.Report($"totalcount = {totalcount}");
                    return;
                }

                if (idX == 0) {
                    var scope = AppImgs.GetScope();
                    scopecount = scope.Length;
                    if (scope.Length < 10) {
                        progress?.Report($"scope.Length = {scope.Length}");
                        return;
                    }

                    idX = AppImgs.GetNextView().Id;
                }

                if (!AppImgs.TryGetValue(idX, out imgX)) {
                    idX = 0;
                    continue;
                }

                AppVars.ImgPanel[0] = GetImgPanel(idX);
                if (AppVars.ImgPanel[0] == null) {
                    Delete(idX);
                    progress?.Report($"{idX} deleted");
                    idX = 0;
                    continue;
                }

                var idY = imgX.BestId;
                AppVars.ImgPanel[1] = GetImgPanel(idY);
                if (AppVars.ImgPanel[1] == null) {
                    Delete(idY);
                    progress?.Report($"{idY} deleted");
                    idX = 0;
                    continue;
                }

                break;
            }
            while (true);

            progress?.Report($"{scopecount}/{totalcount}: {imgX.Distance:F2}");
        }
    }
}
