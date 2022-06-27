namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static void AddToMemory(Img img)
        {
            _imgList.Add(img.Id, img);
            _nameList.Add(img.Name, img);
            _hashList.Add(img.Hash, img);
        }

        private static void Add(Img img)
        {
            AddToMemory(img);
            SqlAddImage(img);
        }
    }
}
