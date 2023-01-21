using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Delete(string hash, IProgress<string> progress)
        {
            if (AppImgs.TryGetValue(hash, out var img)) {
                var shortfilename = img.GetShortFileName();
                progress.Report($"Delete {shortfilename}");
                AppImgs.Delete(img);
                var filename = img.GetFileName();
                FileHelper.DeleteToRecycleBin(filename);
            }

            AppDatabase.DeleteImage(hash);
        }
    }
} 