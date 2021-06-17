using OpenCvSharp;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void AddToMemory(Img img)
        {
            lock (_imglock) {
                _imgList.Add(img.FileName, img);
                _hashList.Add(img.Hash, img);
            }
        }

        private static void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }
    }
}
