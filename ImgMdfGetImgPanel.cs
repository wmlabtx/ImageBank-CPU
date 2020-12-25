using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private ImgPanel GetImgPanel(string name)
        {
            Img img;
            int foldercounter;
            lock (_imglock) {
                if (!_imgList.TryGetValue(name, out img)) {
                    return null;
                }

                foldercounter = _imgList.Count(e => e.Value.Folder == img.Folder);
            }

            if (!ImageHelper.GetImageDataFromFile(img.FileName, 
                out byte[] imagedata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                out var bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                out _)) {
                return null;
            }

            var imgpanel = new ImgPanel(
                img: img,
                bitmap: bitmap, 
                length: imagedata.Length,
                foldercounter: foldercounter);

            return imgpanel;
        }
    }
}
