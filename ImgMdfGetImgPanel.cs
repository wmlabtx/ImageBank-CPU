using System.Drawing;
using System.IO;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private ImgPanel GetImgPanel(int id)
        {
            if (!_imgList.TryGetValue(id, out var imgX)) {
                return null;
            }

            /*
            if (!Helper.GetImageDataFromFile(
                imgX.FileName,
                out var imgdata,
                out var magicformat,
#pragma warning disable CA2000 // Dispose objects before losing scope
                        out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                        out var checksum,
                out var message,
                out var bitmapchanged)) {
                return null;
            }

            if (bitmapchanged) {
                Helper.WriteData(imgX.FileName, imgdata);
                imgX.Format = magicformat;
                imgX.Checksum = checksum;
                var scd = ScdHelper.Compute(bitmap);
                imgX.Vector = scd;
            }

            if (magicformat != imgX.Format) {
                imgX.Format = magicformat;
            }
            */

            if (!File.Exists(imgX.FileName)) {
                return null;
            }

            var imagedata = Helper.ReadData(imgX.FileName);
            if (imagedata == null || imagedata.Length == 0) {
                return null;
            }

            if (!Helper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                return null;
            }

            var name = $"{imgX.Folder}\\{imgX.Name}";
            var imgpanel = new ImgPanel(
                id: id,
                name: name,
                lastview: imgX.LastView,
                distance: imgX.Distance,
                bitmap: bitmap, 
                length: imagedata.Length,
                format: imgX.Format,
                counter: imgX.Counter,
                lastadded: imgX.LastAdded);

            return imgpanel;
        }
    }
}
