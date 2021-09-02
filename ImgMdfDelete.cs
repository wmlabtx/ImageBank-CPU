namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Delete(string name)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(name, out var img)) {
                    if (_hashList.ContainsKey(img.Hash)) {
                        _hashList.Remove(img.Hash);
                    }

                    foreach (var e in _imgList) {
                        if (e.Value.BestHash.Equals(img.Hash)) {
                            e.Value.BestHash = e.Value.Hash;
                            e.Value.Distance = 486f;
                            e.Value.LastCheck = GetMinLastCheck();
                        }
                    }

                    _imgList.Remove(name);

                    var filename = Helper.GetFileName(img.Name);
                    Helper.DeleteToRecycleBin(filename);
                }
            }

            SqlDelete(name);
        }
    }
}