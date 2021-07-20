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
                        if (e.Value.NextHash.Equals(img.Hash)) {
                            e.Value.NextHash = e.Value.Hash;
                            e.Value.KazeMatch = 0;
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