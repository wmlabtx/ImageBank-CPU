namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Delete(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    _hashList.Remove(img.Hash);
                    _imgList.Remove(id);
                    var filename = Helper.GetFileName(img.Name);
                    Helper.DeleteToRecycleBin(filename);
                }
            }

            SqlDelete(id);
        }
    }
}