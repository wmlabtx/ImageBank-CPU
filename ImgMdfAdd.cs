namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static void Add(Img img)
        {
            AppImgs.Add(img);
            AppDatabase.AddImage(img);
        }
    }
}
