using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            AppVars.ImgPanel[0].Img.Counter++;
            UpdateLastView(1);
            UpdateLastView(0);
        }

        public static void UpdateLastView(int index)
        {
            var dtx = index == 0 ? 0 : -10;
            AppVars.ImgPanel[index].Img.LastView = DateTime.Now.AddSeconds(dtx);
        }
    }
}