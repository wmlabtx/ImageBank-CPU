using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.LastView = DateTime.Now;
        }

        public static void UpdateLastViewOnly()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.LastView = DateTime.Now;
        }
    }
}