using System.Linq;

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

                    var minlc = _imgList.Min(e => e.Value.LastCheck).AddSeconds(-1);
                    foreach (var e in _imgList) {
                        if (e.Value.NextHash.Equals(img.Hash)) {
                            e.Value.LastCheck = minlc;
                            e.Value.NextHash = e.Value.Hash;
                        }
                    }
                }
            }

            SqlDelete(name);
        }
    }
}