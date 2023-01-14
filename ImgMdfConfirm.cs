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
            var compare = string.Compare(imgX.Family, imgY.Family, StringComparison.OrdinalIgnoreCase);
            if (compare == 0) {
                imgX.SetFamily(imgX.Name);
                imgY.SetFamily(imgY.Name);
            }
            else {
                var sizeX = AppImgs.Count(imgX.Family);
                var sizeY = AppImgs.Count(imgY.Family);
                if (sizeX == sizeY) {
                    if (compare < 0) {
                        imgY.SetFamily(imgX.Family);
                    }
                    else {
                        imgX.SetFamily(imgY.Family);
                    }
                }
                else {
                    if (sizeX > sizeY) {
                        imgY.SetFamily(imgX.Family);
                    }
                    else {
                        imgX.SetFamily(imgY.Family);
                    }
                }
            }
        }
    }
} 