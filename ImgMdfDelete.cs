using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Delete(string filename)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(filename, out var img)) {
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

                    _imgList.Remove(filename);

                    if (File.Exists(filename)) {
                        Helper.DeleteToRecycleBin(img.FileName);
                    }
                }
            }

            SqlDelete(filename);
        }
    }
}