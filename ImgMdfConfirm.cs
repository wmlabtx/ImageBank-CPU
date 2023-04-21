using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppPanels.GetImgPanel(0).Img;
            imgX.SetLastView(DateTime.Now);
            
            var imgY = AppPanels.GetImgPanel(1).Img;
            imgY.SetLastView(DateTime.Now);

            if (imgX.Review < imgY.Review) {
                imgX.IncrementReview();
            }
            else {
                if (imgX.Review == imgY.Review) {
                    imgX.IncrementReview();
                    imgY.IncrementReview();
                }
                else {
                    imgY.IncrementReview();
                }
             }
        }
    }
} 