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

            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap, out int format)) {
                return null;
            }

            if (img.Format != format) {
                img.Format = format;
            }

            var name = $"{img.Folder}\\{img.Name}";
            var personsize = GetPersonSize(img.Person);
            var imgpanel = new ImgPanel(
                id: id,
                name: name,
                person: img.Person,
                personsize: personsize,
                lastview: img.LastView,
                sim: img.Sim,
                bitmap: bitmap, 
                length: imgdata.Length,
                format: img.Format,
                counter: img.Counter);

            return imgpanel;
        }
    }
}
