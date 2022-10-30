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
        private static readonly SortedList<int, SortedList<int, float>> _pairsList = new SortedList<int, SortedList<int, float>>();

        public static void Clear()
        {
            lock (_imglock) {
                _imgList.Clear();
                _nameList.Clear();
                _hashList.Clear();
                _pairsList.Clear();
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
                    AppDatabase.DeletePair(img.Id);
                    _pairsList.Remove(img.Id);
                    foreach (var e in _pairsList) {
                        e.Value.Remove(img.Id);
                    }

                    _nameList.Remove(img.Name);
                    _hashList.Remove(img.Hash);
                    _imgList.Remove(img.Id);
                }
            }
        }

        public static void AddPair(int idx, int idy, float distance, bool savetodb)
        {
            lock (_imglock) {
                if (!_pairsList.ContainsKey(idx)) {
                    _pairsList.Add(idx, new SortedList<int, float>());
                }

                if (!_pairsList[idx].ContainsKey(idy)) {
                    _pairsList[idx].Add(idy, distance);
                    if (savetodb) {
                        AppDatabase.AddPair(idx, idy, distance);
                    }
                }

                if (!_pairsList.ContainsKey(idy)) {
                    _pairsList.Add(idy, new SortedList<int, float>());
                }

                if (!_pairsList[idy].ContainsKey(idx)) {
                    _pairsList[idy].Add(idx, distance);
                    if (savetodb) {
                        AppDatabase.AddPair(idy, idx, distance);
                    }
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

        public static Img GetNextCheck()
        {
            Img result = null;
            lock (_imglock) {
                var scope = _imgList.OrderBy(e => e.Value.Distance);
                foreach (var img in scope) {
                    if (img.Value.GetVector().Length != 4096 || !_imgList.ContainsKey(img.Value.BestId) || InHistory(img.Key, img.Value.BestId)) {
                        result = img.Value;
                        break;
                    }
                }

                if (result == null) {
                    var minlc = _imgList.Min(e => e.Value.LastCheck);
                    result = _imgList.FirstOrDefault(e => e.Value.LastCheck == minlc).Value;
                }
            }

            return result;
        }

        public static Img GetNextView()
        {
            Img result = null;
            var resulthistorysize = 0;
            lock (_imglock) {
                foreach (var img in _imgList) {
                    if (img.Value.GetVector().Length != 4096 || !_imgList.ContainsKey(img.Value.BestId) || InHistory(img.Key, img.Value.BestId)) {
                        continue;
                    }

                    var historysize = GetHistorySize(img.Key);

                    if (result == null) {
                        result = img.Value;
                        resulthistorysize = historysize;
                        continue;
                    }

                    if (historysize < resulthistorysize) {
                        result = img.Value;
                        resulthistorysize = historysize;
                        continue;
                    }

                    if (historysize == resulthistorysize && img.Value.Distance < result.Distance) {
                        result = img.Value;
                        continue;
                    }
                }
            }

            return result;
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
                result = _imgList.Values.Where(e => _imgList.ContainsKey(e.BestId) && !InHistory(e.Id, e.BestId) && e.GetVector().Length == 4096).OrderBy(e => e.LastView).Select(e => e.Id).ToArray();
            }

             return result;
        }

        public static int GetHistorySize(int id)
        {
            int historysize;
            lock (_imglock) {
                historysize = _pairsList.ContainsKey(id) ? _pairsList[id].Count : 0;
            }

            return historysize;
        }

        public static bool InHistory(int idx, int idy)
        {
            bool result;
            lock (_imglock) {
                result = _pairsList.ContainsKey(idx) && _pairsList[idx].ContainsKey(idy);
            }

            return result;
        }
    }
}
