using System.IO;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static ImgPanel GetImgPanel(string filename)
        {
            Img img;
            lock (_imglock) {
                if (!_imgList.TryGetValue(filename, out img)) {
                    return null;
                }
            }

            var imagedata = File.ReadAllBytes(filename);
            if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                return null;
            }

            var imgpanel = new ImgPanel(
                img: img,
                bitmap: bitmap);

            return imgpanel;
        }
    }
}
