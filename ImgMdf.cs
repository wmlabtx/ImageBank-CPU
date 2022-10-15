using System;
using System.Collections.Generic;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static readonly SortedList<int, string> BinsList = new SortedList<int, string>();

        static ImgMdf()
        {
            VggHelper.LoadNetwork();
        }

        public static void LoadImages(IProgress<string> progress)
        {
            AppImgs.Clear();
            AppDatabase.LoadImages(progress);
        }
    }
}