namespace ImageBank
{
    public partial class ImgMdf
    {
        private void AddToMemory(Img img)
        {
            lock (_imglock) {
                _imgList.Add(img.Id, img);
                _nameList.Add(img.Name, img);
                _checksumList.Add(img.Checksum, img);
            }
        }

        private void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }
    }
}
