namespace ImageBank
{
    public partial class ImgMdf
    {
        private void AddToMemory(Img img)
        {
            lock (_imglock) { 
                _hashList.Add(img.Hash, img);
                _imgList.Add(img.Name, img);
            }
        }

        private void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }
    }
}
