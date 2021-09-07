using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public Img Img { get; }
        public long Size { get; }
        public Bitmap Bitmap { get; }

        public ImgPanel(Img img, long size, Bitmap bitmap)
        {
            Img = img;
            Size = size;
            Bitmap = bitmap;
        }
    }
}
