using System;
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
                    _hashList.Remove(img.Hash);
                }
            }

            SqlDelete(name);
            ResetRefers(name);
        }

        private void ResetRefers(string name)
        {
            lock (_imglock) {
                var minlc = _imgList.Min(e => e.Value.LastCheck).AddSeconds(-1);
                foreach (var img in _imgList) {
                    if (img.Value.NextName.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                        img.Value.NextName = "0123456789";
                        img.Value.LastCheck = minlc;
                    }
                }
            }
        }
    }
}