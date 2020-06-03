using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public Img Img { get; }
        public Bitmap Bitmap { get; }
        public long Length { get; }
        public int FolderCounter { get; }

        public ImgPanel(Img img, Bitmap bitmap, long length, int foldercounter)
        {
            Img = img;
            Bitmap = bitmap;
            Length = length;
            FolderCounter = foldercounter;
        }
    }
}
