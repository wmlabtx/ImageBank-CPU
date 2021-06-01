namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Delete(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    Helper.DeleteToRecycleBin(img.FileName);
                    _imgList.Remove(id);

                    if (_hashList.ContainsKey(img.Hash)) {
                        _hashList.Remove(img.Hash);
                    }

                    foreach (var e in _imgList) {
                        if (e.Value.NextHash.Equals(img.Hash)) {
                            e.Value.NextHash = e.Value.Hash;
                            e.Value.AkazePairs = 0;
                        }
                    }
                }
            }

            SqlDelete(id);
        }
    }
}