using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public static class AppImgs
    {
        private static readonly object _imglock = new object();
        private static readonly SortedList<int, Img> _imgList = new SortedList<int, Img>();
        private static readonly SortedList<string, Img> _nameList = new SortedList<string, Img>();
        private static readonly SortedList<string, Img> _hashList = new SortedList<string, Img>();

        public static void Clear()
        {
            lock (_imglock) {
                _imgList.Clear();
                _nameList.Clear();
                _hashList.Clear();
            }
        }

        public static int Count()
        {
            lock (_imglock) {
                return _imgList.Count;
            }
        }

        public static void Add(Img img)
        {
            lock (_imglock) {
                _imgList.Add(img.Id, img);
                _nameList.Add(img.Name, img);
                _hashList.Add(img.Hash, img);
            }
        }

        public static bool TryGetValue(int id, out Img img)
        {
            bool result;
            lock (_imglock) {
                result = _imgList.TryGetValue(id, out Img _img);
                img = _img;
            }

            return result;
        }

        public static bool ContainsId(int id)
        {
            bool result;
            lock (_imglock) {
                result = _imgList.ContainsKey(id);
            }

            return result;
        }

        public static bool ContainsHash(string hash)
        {
            bool result;
            lock (_imglock) {
                result = _hashList.ContainsKey(hash);
            }

            return result;
        }

        public static bool TryGetHash(string hash, out Img img)
        {
            bool result;
            lock (_imglock) {
                result = _hashList.TryGetValue(hash, out Img _img);
                img = _img;
            }

            return result;
        }

        public static bool ContainsName(string name)
        {
            bool result;
            lock (_imglock) {
                result = _nameList.ContainsKey(name);
            }

            return result;
        }

        public static void Delete(Img img)
        {
            lock (_imglock) {
                if (_imgList.ContainsKey(img.Id)) {
                    _nameList.Remove(img.Name);
                    _hashList.Remove(img.Hash);
                    _imgList.Remove(img.Id);
                }
            }
        }

        public static DateTime GetMinLastView()
        {
            DateTime lv;
            lock (_imglock) {
                lv = _imgList.Min(e => e.Value.LastView).AddSeconds(1);
            }

            return lv;
        }

        public static DateTime GetMinLastCheck()
        {
            DateTime lv;
            lock (_imglock) {
                lv = _imgList.Min(e => e.Value.LastCheck).AddSeconds(1);
            }

            return lv;
        }

        public static Img GetLastChecked()
        {
            Img result;
            lock (_imglock) {
                result = _imgList
                    .Values
                    .Where(e => DateTime.Now.Subtract(e.LastCheck).TotalHours > 1.0)
                    .OrderBy(e => e.GetHistorySize())
                    .ThenBy(e => e.Distance)
                    .FirstOrDefault();
            }

            return result;
        }

        public static Img GetLastViewed()
        {
            Img result;
            lock (_imglock) {
                result = _imgList
                    .Values
                    .Where(e => e.GetVector().Length == 4096 && _imgList.ContainsKey(e.BestId) && !e.InHistory(e.BestId))
                    .OrderBy(e => e.GetHistorySize())
                    .ThenBy(e => e.Distance)
                    .FirstOrDefault();
            }

            return result;
        }

        public static void ResetLastCheck()
        {
            lock (_imglock) {
                foreach (var img in _imgList) {
                    img.Value.LastCheck = img.Value.LastView;
                }
            }
        }

        public static List<Tuple<int, float[]>> GetShadow()
        {
            var result = new List<Tuple<int, float[]>>();
            lock (_imglock) {
                foreach (var img in _imgList) {
                    result.Add(new Tuple<int, float[]>(img.Key, img.Value.GetVector()));
                }
            }

            return result;
        }

        public static Img GetRandomImg()
        {
            Img result;
            lock (_imglock) {
                var keys = _imgList.Keys;
                var rpos = AppVars.IRandom(0, keys.Count - 1);
                result = _imgList[keys[rpos]];
            }

            return result;
        }

        public static int[] GetScope()
        {
            int[] result;
            lock (_imglock) {
                result = _imgList.Values.Where(e => _imgList.ContainsKey(e.BestId) && !e.InHistory(e.BestId) && e.GetVector().Length == 4096).OrderBy(e => e.LastView).Select(e => e.Id).ToArray();
            }

             return result;
        }

        public static Img GetFirstInvalid()
        {
            Img result = null;
            lock (_imglock) {
                var list = _imgList.OrderBy(e => e.Value.LastView);
                foreach (var img in list) {
                    if (!_imgList.ContainsKey(img.Value.BestId) || img.Value.InHistory(img.Value.BestId)) {
                        result = img.Value;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
