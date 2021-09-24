using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Confirm(int index)
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;
            if (imgX.Id != imgY.Id && !imgX.History.ContainsKey(imgY.Id)) {
                imgX.History.Add(imgY.Id, imgY.Id);
                imgX.SaveHistory();
            }

            imgX.LastView = DateTime.Now;

            EloHelper.Compute(
                AppVars.ImgPanel[index].Img.Elo,
                AppVars.ImgPanel[1 - index].Img.Elo,
                1,
                0,
                out int newX, 
                out int newY);

            AppVars.ImgPanel[index].Img.Elo = newX;
            AppVars.ImgPanel[1 - index].Img.Elo = newY;
        }

        public static void UpdateLastViewOnly()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.LastView = DateTime.Now;
        }
    }
}