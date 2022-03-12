using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.Counter++;
            UpdateLastView(0);
            UpdateLastView(1);
        }

        public static void UpdateLastView(int index)
        {
            var imgX = AppVars.ImgPanel[index].Img;
            imgX.LastView = DateTime.Now;
        }
    }
}