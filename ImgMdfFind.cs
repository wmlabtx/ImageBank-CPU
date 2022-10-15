using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private const int POSEMAX = 10000;
        private static int _pose;

        public static void MoveBackward(IProgress<string> progress)
        {
            /*
            Img imgX = null;
            if (_imgList.Count < 2) {
                progress?.Report("No images to view");
                return;
            }

            var lv = _imgList.Max(e => e.Value.LastView);
            imgX = _imgList.First(e => e.Value.LastView == lv).Value;

            var idX = imgX.Id;
            AppVars.ImgPanel[0] = GetImgPanel(idX);
            if (AppVars.ImgPanel[0] == null) {
                Delete(idX);
                progress?.Report($"{idX} deleted");
                idX = 0;
                return;
            }

            imgX = AppVars.ImgPanel[0].Img;

            var idY = imgX.BestId;
            AppVars.ImgPanel[1] = GetImgPanel(idY);
            if (AppVars.ImgPanel[1] == null) {
                Delete(idY);
                progress?.Report($"{idY} deleted");
                FindNext(imgX.Id, progress);
                idY = imgX.BestId;
                AppVars.ImgPanel[1] = GetImgPanel(idY);
            }

            progress?.Report($"{imgX.Distance:F2}");
            */
        }

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
                    if (scope.Length < 100) {
                        progress?.Report($"scope.Length = {scope.Length}");
                        return;
                    }

                    /*
                    var id0 = scope[0];
                    if (!AppImgs.TryGetValue(id0, out var img0)) {
                        return;
                    }

                    var id99 = scope[99];
                    if (!AppImgs.TryGetValue(id99, out var img99)) {
                        return;
                    }

                    var days = img0.LastView.Subtract(img99.LastView).TotalDays;
                    if (days > 7.0) {
                        idX = id0;                        
                    }
                    else {
                        _pose = AppVars.IRandom(0, 99);
                        idX = scope[_pose];
                    }*/

                    idX = AppImgs.GetLastViewed().Id;
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
