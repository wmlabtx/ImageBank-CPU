using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppPanels.GetImgPanel(0).Img;
            imgX.SetLastView(DateTime.Now);
        }

        public static void Combine()
        {
            var imgX = AppPanels.GetImgPanel(0).Img;
            var imgY = AppPanels.GetImgPanel(1).Img;
            if (imgX.NextHash.Equals(imgY.Hash)) {
                imgX.SetNextHash(imgX.Hash);
            }
            else {
                imgX.SetNextHash(imgY.Hash);
                if (imgY.Hash.Equals(imgY.NextHash) || !AppImgs.ContainsHash(imgY.NextHash)) {
                    imgY.SetNextHash(imgX.Hash);
                }
            }
        }
    }
} 