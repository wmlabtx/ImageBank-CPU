namespace ImageBank
{
    public partial class ImgMdf
    {
        private static ImgPanel GetImgPanel(int id)
        {
            Img img;
            lock (_imglock) {
                if (!_imgList.TryGetValue(id, out img)) {
                    return null;
                }
            }

            var filename = FileHelper.NameToFileName(img.Name);
            var imagedata = FileHelper.ReadData(filename);
            if (imagedata == null) {
                return null;
            }

            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
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
