﻿using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppPanels.GetImgPanel(0).Img;
            imgX.SetLastView(DateTime.Now);
            imgX.SetReview((short)(imgX.Review + 1));
        }
    }
} 