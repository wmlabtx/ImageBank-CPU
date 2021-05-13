using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public Img Img { get; }
        public Bitmap Bitmap { get; }

        public ImgPanel(Img img, Bitmap bitmap)
        {
            Img = img;
            Bitmap = bitmap;
        }
    }
}
