namespace ImageBank
{
    public partial class ImgMdf
    {
        private void AddToMemory(Img img)
        {
            lock (_imglock) {
                if (!string.IsNullOrEmpty(img.Hash)) {
                    _hashList.Add(img.Hash, img.Name);
                }

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
