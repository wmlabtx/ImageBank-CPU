namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Delete(string hash)
        {
            if (AppImgs.TryGetValue(hash, out var img)) {
                AppImgs.Delete(img);
                var filename = $"{AppConsts.PathRoot}\\{img.Name}";
                FileHelper.DeleteToRecycleBin(filename);
            }

            AppDatabase.DeleteImage(hash);
        }
    }
} 