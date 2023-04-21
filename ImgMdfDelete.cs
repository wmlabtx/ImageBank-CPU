using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Delete(Img imgD, IProgress<string> progress)
        {
            progress.Report($"Delete {imgD.GetShortFileName()}");
            AppImgs.Delete(imgD);
            var filename = imgD.GetFileName();
            FileHelper.DeleteToRecycleBin(filename);
            AppDatabase.DeleteImage(imgD.Hash);
        }

        public static void Delete(int idpanel, IProgress<string> progress)
        {
            var imgD = AppPanels.GetImgPanel(idpanel).Img;
            Delete(imgD, progress);
        }
    }
} 