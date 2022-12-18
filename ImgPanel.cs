using System;
using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public Img Img { get; }
        public long Size { get; }
        public Bitmap Bitmap { get; }
        public string Format { get; }
        public DateTime DateTaken { get; }

        public ImgPanel(Img img, long size, Bitmap bitmap, string format, DateTime datetaken)
        {
            Img = img;
            Size = size;
            Bitmap = bitmap;
            Format = format;
            DateTaken = datetaken;
        }
    }
}
