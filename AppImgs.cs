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
        private static readonly List<int> _stackList = new List<int>();

        public static void Clear()
        {
            lock (_imglock) {
                _imgList.Clear();
                _nameList.Clear();
                _hashList.Clear();
                _stackList.Clear();
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
                _stackList.Insert(0, img.Id);
            }
        }

        public static void Resort()
        {
            lock (_imglock) {
                _stackList.Clear();
                _stackList.AddRange(_imgList.OrderBy(e => e.Value.LastView).Select(e => e.Key).ToList());
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
                    _stackList.Remove(img.Id);
                    _nameList.Remove(img.Name);
                    _hashList.Remove(img.Hash);
                    _imgList.Remove(img.Id);
                }

                foreach (var e in _imgList) {
                    if (e.Value.BestId == img.Id) {
                        SetFirst(e.Key);
                    }
                }
            }
        }

        public static void SetLast(int id)
        {
            lock (_imglock) {
                _stackList.Remove(id);
                _stackList.Add(id);
            }
        }

        public static void SetFirst(int id)
        {
            lock (_imglock) {
                _stackList.Remove(id);
                _stackList.Insert(0, id);
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

        public static Img GetNextCheck()
        {
            Img result;
            lock (_imglock) {
                var idX = _stackList.ElementAt(0);
                while (!_imgList.TryGetValue(idX, out result) && _stackList.Count > 1) {
                    _stackList.RemoveAt(0);
                    idX = _stackList.ElementAt(0);
                }
            }

            return result;
        }

        public static Img GetNextView()
        {
            Img result = null;
            lock (_imglock) {
                foreach (var img in _imgList) {
                    if (img.Value.GetVector().Length != 4096) {
                        continue;
                    }

                    if (img.Value.InHistory(img.Value.BestId)) {
                        continue;
                    }

                    if (!_imgList.TryGetValue(img.Value.BestId, out Img imgN)) {
                        continue;
                    }

                    if (img.Value.FamilyId > 0 && img.Value.FamilyId == imgN.FamilyId) {
                        continue;
                    }

                    if (result == null) {
                        result = img.Value;
                        continue;
                    }

                    if (img.Value.GetHistorySize() < result.GetHistorySize()) {
                        result = img.Value;
                        continue;
                    }

                    if (img.Value.GetHistorySize() == result.GetHistorySize() && img.Value.Distance < result.Distance) {
                        result = img.Value;
                        continue;
                    }
                }
            }

            return result;
        }

        public static List<Tuple<int, float[], int>> GetShadow()
        {
            var result = new List<Tuple<int, float[], int>>();
            lock (_imglock) {
                foreach (var img in _imgList) {
                    result.Add(new Tuple<int, float[], int>(img.Key, img.Value.GetVector(), img.Value.FamilyId));
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
                var scope = _imgList.OrderBy(e => e.Value.Distance);
                foreach (var img in scope) {
                    if (!_imgList.TryGetValue(img.Value.BestId, out Img imgN) || img.Value.InHistory(img.Value.BestId)) {
                        result = img.Value;
                        break;
                    }

                    if (img.Value.FamilyId > 0 && imgN.FamilyId == img.Value.FamilyId) {
                        result = img.Value;
                        break;
                    }
                }
            }

            return result;
        }

        public static int AllocateFamilyId()
        {
            int result;
            lock (_imglock) {
                result = _imgList.Max(e => e.Value.FamilyId) + 1;
            }

            return result;
        }

        public static int GetFamilySize(int familyid)
        {
            int result;
            lock (_imglock) {
                result = _imgList.Count(e => e.Value.FamilyId == familyid);
            }

            return result;
        }
    }
}
