namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Delete(string name)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(name, out var img)) {
                    Helper.DeleteToRecycleBin(img.FileName);
                    _imgList.Remove(name);

                    if (_hashList.ContainsKey(img.Hash)) {
                        _hashList.Remove(img.Hash);
                    }

                    foreach (var e in _imgList) {
                        if (e.Value.NextHash.Equals(img.Hash)) {
                            e.Value.NextHash = e.Value.Hash;
                            e.Value.Distance = AppConsts.MaxDistance;
                            e.Value.RemoveFromHistory(img.Hash);
                        }
                    }
                }
            }

            SqlDelete(name);
        }
    }
}