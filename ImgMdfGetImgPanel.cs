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

            if (!ImageHelper.GetImageDataFromFile(img.FileName, 
                out _,
#pragma warning disable CA2000 // Dispose objects before losing scope
                out var bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                out _)) {
                return null;
            }

            var imgpanel = new ImgPanel(
                img: img,
                bitmap: bitmap);

            return imgpanel;
        }
    }
}
