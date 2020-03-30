using System.Drawing;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private ImgPanel GetImgPanel(int id)
        {
            if (!_imgList.TryGetValue(id, out var img)) {
                return null;
            }

            var imagedata = Helper.ReadData(img.FileName);
            if (imagedata == null) {
                return null;
            }

            if (!Helper.GetBitmapFromImageData(imagedata, out Bitmap bitmap)) {
                return null;
            }

            var magicformat = Helper.GetMagicFormat(imagedata);
            if (magicformat != img.Format) {
                img.Format = magicformat;
            }

            var name = $"{img.Folder}\\{img.Name}";
            var imgpanel = new ImgPanel(
                id: id,
                name: name,
                lastview: img.LastView,
                sim: img.Sim,
                bitmap: bitmap, 
                length: imagedata.Length,
                format: img.Format,
                counter: img.Counter);

            return imgpanel;
        }
    }
}
