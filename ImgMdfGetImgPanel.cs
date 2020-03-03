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

            var imgdata = Helper.ReadData(img.File);
            if (imgdata == null) {
                return null;
            }

            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap)) {
                return null;
            }

            var imgpanel = new ImgPanel(
                id: id,
                name: img.Name,
                lastview: img.LastView,
                generation: img.Generation,
                distance: img.Distance,
                lastchange: img.LastChange,
                bitmap: bitmap, 
                length: imgdata.Length);

            return imgpanel;
        }
    }
}
