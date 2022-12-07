﻿using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        static ImgMdf()
        {
        }

        public static void LoadImages(IProgress<string> progress)
        {
            AppImgs.Clear();
            VggHelper.LoadNet(progress);
            AppDatabase.LoadImages(progress);
        }
    }
}