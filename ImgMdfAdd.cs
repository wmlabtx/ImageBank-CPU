using OpenCvSharp;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void AddToMemory(Img img)
        {
            lock (_imglock) { 
                _hashList.Add(img.Hash, img);
                _imgList.Add(img.Id, img);
            }
        }

        private static void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }
    }
}
