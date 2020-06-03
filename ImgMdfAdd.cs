namespace ImageBank
{
    public partial class ImgMdf
    {
        private void AddToMemory(Img img)
        {
            lock (_imglock) {
                _imgList.Add(img.Name, img);
                _hashList.Add(img.Hash, img.Name);
            }
        }

        private void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }
    }
}
