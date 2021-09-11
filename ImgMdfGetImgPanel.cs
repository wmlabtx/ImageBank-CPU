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

            var filename = Helper.GetFileName(img.Name);
            var imagedata = Helper.ReadData(filename);
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
