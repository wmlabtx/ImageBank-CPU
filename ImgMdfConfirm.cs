using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;
            if (imgX.Id != imgY.Id && !imgX.History.ContainsKey(imgY.Id)) {
                imgX.History.Add(imgY.Id, imgY.Id);
                imgX.SaveHistory();
            }

            imgX.LastView = DateTime.Now;
        }

        public static void UpdateLastViewOnly()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.LastView = DateTime.Now;
        }
    }
}