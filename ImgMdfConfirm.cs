using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Confirm(int index)
        {
            lock (_imglock) {
                AppVars.ImgPanel[index].Img.LastView = DateTime.Now;
                AppVars.ImgPanel[1 - index].Img.LastView = DateTime.Now;
            }
        }
    }
}