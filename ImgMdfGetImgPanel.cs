namespace ImageBank
{
    public static partial class ImgMdf
    {
        private static ImgPanel GetImgPanel(int id)
        {
            if (!_imgList.TryGetValue(id, out Img img)) {
                return null;
            }

            var filename = FileHelper.NameToFileName(img.Name);
            var imagedata = FileHelper.ReadData(filename);
            if (imagedata == null) {
                return null;
            }

            var bitmap = BitmapHelper.ImageDataToBitmap(imagedata);
            if (bitmap == null) {
                return null;
            }

            var imgpanel = new ImgPanel(
                img: img,
                size: imagedata.LongLength,
                bitmap: bitmap);

            return imgpanel;
        }
    }
}
