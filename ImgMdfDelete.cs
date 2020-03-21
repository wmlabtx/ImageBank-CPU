using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Delete(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    Helper.DeleteToRecycleBin(img.FileName);
                    _imgList.Remove(id);
                }
            }

            SqlDelete(id);
            ResetRefers(id);
        }

        private void ResetRefers(int id)
        {
            var minlc = new DateTime(1997, 1, 1, 8, 0, 0);
            lock (_imglock) {
                _imgList
                    .Values
                    .Where(e => e.NextId == id)
                    .ToList()
                    .ForEach(e => e.LastCheck = minlc);
            }
        }
    }
}