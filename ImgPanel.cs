using System;
using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public Img Img { get; }
        public long Size { get; }
        public Bitmap Bitmap { get; }
        public DateTime DateTaken { get; }

        public ImgPanel(Img img, long size, Bitmap bitmap, DateTime datetaken) {
            Img = img;
            Size = size;
            Bitmap = bitmap;
            DateTaken = datetaken;
        }
    }
}
