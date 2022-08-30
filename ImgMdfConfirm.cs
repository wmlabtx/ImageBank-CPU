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
            imgX.AddHistory(imgY.Id);
        }
    }
}