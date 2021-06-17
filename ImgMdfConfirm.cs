using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void UpdateCounter(int index)
        {
            lock (_imglock) {
                AppVars.ImgPanel[index].Img.Counter += 1;
            }
        }

        public static void UpdateLastView(int index)
        {
            lock (_imglock) {
                AppVars.ImgPanel[index].Img.LastView = DateTime.Now;
            }
        }
    }
}