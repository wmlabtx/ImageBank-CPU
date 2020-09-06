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
                var minlc = DateTime.Now.AddYears(-10);
                _imgList
                    .Values
                    .Where(e => e.NextName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    .ForEach(e => e.LastCheck = minlc);
            }
        }
    }
}