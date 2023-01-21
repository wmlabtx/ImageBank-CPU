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
            if (!AppImgs.ValidBestHash(imgX)) {
                imgX.SetBestHash(imgY.Hash);
                if (!AppImgs.ValidBestHash(imgY)) {
                    imgY.SetBestHash(imgX.Hash);
                }
            }
            else {
                imgX.SetBestHash(imgX.Hash);
            }
        }
    }
} 