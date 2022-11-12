using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public static class AppImgs
    {
        private static readonly SortedList<int, Img> _imgList = new SortedList<int, Img>();
        private static readonly SortedList<string, Img> _nameList = new SortedList<string, Img>();
        private static readonly SortedList<string, Img> _hashList = new SortedList<string, Img>();

        public static void Clear()
        {
            _imgList.Clear();
            _nameList.Clear();
            _hashList.Clear();
        }

        public static int Count()
        {
            return _imgList.Count;
        }

        public static void Add(Img img)
        {
            _imgList.Add(img.Id, img);
            _nameList.Add(img.Name, img);
            _hashList.Add(img.Hash, img);
        }

        public static bool TryGetValue(int id, out Img img)
        {
            bool result;
            result = _imgList.TryGetValue(id, out Img _img);
            img = _img;
            return result;
        }

        public static bool ContainsId(int id)
        {
            bool result;
            result = _imgList.ContainsKey(id);
            return result;
        }

        public static bool ContainsHash(string hash)
        {
            bool result;
            result = _hashList.ContainsKey(hash);
            return result;
        }

        public static bool TryGetHash(string hash, out Img img)
        {
            bool result;
            result = _hashList.TryGetValue(hash, out Img _img);
            img = _img;
            return result;
        }

        public static bool ContainsName(string name)
        {
            bool result;
            result = _nameList.ContainsKey(name);
            return result;
        }

        public static void Delete(Img img)
        {
            foreach (var e in _imgList) {
                if (e.Value.BestId == img.Id) {
                    e.Value.SetLastCheck(GetMinLastCheck());
                }
            }

            if (_imgList.ContainsKey(img.Id)) {
                _nameList.Remove(img.Name);
                _hashList.Remove(img.Hash);
                _imgList.Remove(img.Id);
                if (img.ClusterId > 0) {
                    AppClusters.Update(img.ClusterId);
                }
            }
        }

        public static DateTime GetMinLastView()
        {
            DateTime lv;
            lv = _imgList.Min(e => e.Value.LastView).AddSeconds(1);
            return lv;
        }

        public static DateTime GetMinLastCheck()
        {
            DateTime lv;
            lv = _imgList.Min(e => e.Value.LastCheck).AddSeconds(1);
            return lv;
        }

        public static Img GetNextCheck()
        {
            Img result = null;
            result = _imgList.Values.OrderBy(e => e.LastCheck).FirstOrDefault();
            return result;
        }

        public static Img GetNextView()
        {
            var minlv = _imgList.Values.Min(e => e.LastView);
            var nextday = minlv.AddDays(1);
            var oldday = _imgList.Values.Where(e => e.LastView < nextday).ToArray();
            var imgX = oldday.OrderBy(e => e.Distance).FirstOrDefault();
            return imgX;
        }

        public static List<Tuple<int, float[]>> GetVectors(int clusterid)
        {
            List<Tuple<int, float[]>> result = new List<Tuple<int, float[]>>();
            foreach (var e in _imgList) {
                if (e.Value.ClusterId == clusterid) {
                    var vector = AppDatabase.ImageGetVector(e.Key);
                    if (vector != null && vector.Length == 4096) {
                        result.Add(Tuple.Create(e.Key, vector));
                    }
                }
            }

            return result;
        }

        public static int[] GetKeys()
        {
            int[] result;
            result = _imgList.Keys.ToArray();
            return result;
        }
    }
}
