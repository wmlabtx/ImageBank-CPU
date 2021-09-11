using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;
            if (imgX.Family == 0) {
                if (imgY.Family == 0) {
                    throw new Exception();
                }

                if (!imgX.History.ContainsKey(imgY.Family)) {
                    imgX.History.Add(imgY.Family, imgY.Family);
                    imgX.SaveHistory();
                }
            }
            else {
                if (imgX.Family == imgY.Family && !imgX.History.ContainsKey(imgY.Id)) {
                    imgX.History.Add(imgY.Id, imgY.Id);
                    imgX.SaveHistory();
                }

                imgX.LastView = DateTime.Now;
            }
        }

        public static void UpdateLastViewOnly()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.LastView = DateTime.Now;
        }
    }
}