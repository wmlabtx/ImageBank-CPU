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

            var imgdata = Helper.ReadData(img.FileName);
            if (imgdata == null) {
                return null;
            }

            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap)) {
                return null;
            }

            var familysize = GetFamilySize(img.Family);
            var name = $"{img.Folder}\\{img.Name}";
            var done = img.LastId * 100f / _id;

            var imgpanel = new ImgPanel(
                id: id,
                name: name,
                family: img.Family,
                familysize: familysize,
                lastview: img.LastView,
                distance: img.Distance,
                generation: img.Generation,
                bitmap: bitmap, 
                length: imgdata.Length,
                done: done);

            return imgpanel;
        }
    }
}
