using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;
            imgX.SetLastView(DateTime.Now);
            AppImgs.AddPair(imgX.Id, imgY.Id, AppVars.ImgPanel[0].Img.Distance, true);
            var lastcheck = AppImgs.GetMinLastCheck();
            imgX.SetLastCheck(lastcheck);
        }
    }
} 