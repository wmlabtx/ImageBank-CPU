using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Delete(string hash, IProgress<string> progress)
        {
            if (AppImgs.TryGetValue(hash, out var img)) {
                progress.Report($"Delete {img.Name}");
                AppImgs.Delete(img);
                var filename = FileHelper.NameToFileName(hash:img.Hash, name:img.Name);
                FileHelper.DeleteToRecycleBin(filename);
            }

            AppDatabase.DeleteImage(hash);
        }
    }
} 