using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Confirm(int index)
        {
            lock (_imglock) {
                AppVars.ImgPanel[index].Img.LastView = DateTime.Now;
                AppVars.ImgPanel[index].Img.AddToHistory(AppVars.ImgPanel[1 - index].Img.Hash);
            }
        }
    }
}