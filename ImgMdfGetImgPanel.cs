using System.Drawing;
using System.IO;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private ImgPanel GetImgPanel(int id)
        {
            if (!_imgList.TryGetValue(id, out var img)) {
                return null;
            }

            Bitmap bitmap;
            long length;
            try {
                var imgdata = Helper.ReadData(img.File);
                using (var ms = new MemoryStream(imgdata)) {
                    length = imgdata.Length;
                    bitmap = (Bitmap)Image.FromStream(ms);
                }
            }
            catch {
                return null;
            }

            if (bitmap == null) {
                return null;
            }

            var imgpanel = new ImgPanel(
                id: id,
                name: img.Name,
                path: img.Path,
                lastview: img.LastView,
                generation: img.Generation,
                distance: img.Distance,
                lastchange: img.LastChange,
                bitmap: bitmap, 
                length: length);

            return imgpanel;
        }
    }
}
