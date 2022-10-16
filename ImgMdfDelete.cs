namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Delete(int id)
        {
            if (AppImgs.TryGetValue(id, out var img)) {
                AppImgs.Delete(img);
                var filename = FileHelper.NameToFileName(img.Name);
                FileHelper.DeleteToRecycleBin(filename);
            }

            AppDatabase.DeleteImage(id);
        }
    }
} 