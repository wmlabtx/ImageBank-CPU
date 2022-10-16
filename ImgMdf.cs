using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
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