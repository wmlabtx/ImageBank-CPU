using System.IO;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private ImgPanel GetImgPanel(string id)
        {
            if (!_imgList.TryGetValue(id, out var imgX)) {
                return null;
            }

            if (!File.Exists(imgX.FileName)) {
                return null;
            }

            var imagedata = File.ReadAllBytes(imgX.FileName);
            if (imagedata == null || imagedata.Length == 0) {
                return null;
            }

            if (!Helper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                return null;
            }

            var imgpanel = new ImgPanel(
                id: id,
                folder: imgX.Folder,
                lastview: imgX.LastView,
                distance: imgX.Distance,
                bitmap: bitmap, 
                length: imagedata.Length,
                counter: imgX.Counter,
                year: imgX.LastModified.Year);

            return imgpanel;
        }
    }
}
