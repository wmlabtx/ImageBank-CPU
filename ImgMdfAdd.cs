namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static void Add(Img img, float[] vector)
        {
            AppImgs.Add(img);
            AppDatabase.AddImage(img, vector);
        }
    }
}
