using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ImageBank
{
    public static class AppImgs
    {
        private static readonly SortedList<string, Img> _imgList = new SortedList<string, Img>();
        private static readonly SortedList<string, Img> _nameList = new SortedList<string, Img>();

        public static void Clear()
        {
            _imgList.Clear();
            _nameList.Clear();
        }

        public static int Count()
        {
            return _imgList.Count;
        }

        public static void Add(Img img)
        {
            _imgList.Add(img.Hash, img);
            _nameList.Add(img.Name, img);
        }

        public static bool TryGetValue(string hash, out Img img)
        {
            bool result = _imgList.TryGetValue(hash, out Img _img);
            img = _img;
            return result;
        }

        public static bool ContainsHash(string hash)
        {
            bool result = _imgList.ContainsKey(hash);
            return result;
        }

        public static bool ContainsName(string name)
        {
            bool result = _nameList.ContainsKey(name);
            return result;
        }

        public static void Delete(Img img)
        {
            _nameList.Remove(img.Name);
            _imgList.Remove(img.Hash);
        }

        public static DateTime GetMinLastView()
        {
            DateTime lv = _imgList.Count > 0 ? _imgList.Min(e => e.Value.LastView).AddSeconds(1) : DateTime.Now;
            return lv;
        }

        public static DateTime GetMinLastCheck()
        {
            DateTime lv = _imgList.Count > 0 ? _imgList.Min(e => e.Value.LastCheck).AddSeconds(1) : DateTime.Now;
            return lv;
        }

        public static Img GetNextView()
        {
            if (_imgList.Count < 2) {
                return null;
            }

            var scope = _imgList.Values.Where(e => !e.Hash.Equals(e.BestHash) && _imgList.ContainsKey(e.BestHash));
            if (scope.Count() < 2) {
                return null;
            }

            var mincounter = scope.Min(e => e.Counter);
            var imgX = scope.Where(e => e.Counter == mincounter).OrderBy(e => e.Distance).FirstOrDefault();
            return imgX;
        }

        public static Img GetNextCheck()
        {
            if (_imgList.Count < 2) {
                return null;
            }

            var scope = _imgList.Values.OrderBy(e => e.LastCheck);
            foreach (var img in scope) {
                if (img.Hash.Equals(img.BestHash) || !_imgList.ContainsKey(img.BestHash)) {
                    return img;
                }
            }

            return scope.First();
        }

        public static string[] GetKeys()
        {
            string[] result;
            result = _imgList.Keys.ToArray();
            return result;
        }

        public static List<Tuple<string, float>> GetSimilars(Img imgX)
        {
            var similars = new List<Tuple<string, float>>();
            var scope = _imgList.Values.Where(e => !string.Equals(e.Hash, imgX.Hash));
            foreach (var img in scope) {
                var distance = VggHelper.GetDistance(imgX.GetVector(), img.GetVector());
                similars.Add(Tuple.Create(img.Hash, distance));
            }

            similars.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            return similars;
        }

        public static void GetSimilar(Img imgX, out string besthash, out float distance)
        {
            besthash = string.Empty;
            distance = 1f;
            var scope = _imgList.Values.Where(e => !string.Equals(e.Hash, imgX.Hash));
            foreach (var img in scope) {
                var d = VggHelper.GetDistance(imgX.GetVector(), img.GetVector());
                if (d < distance) {
                    besthash = img.Hash;
                    distance = d;
                }
            }
        }
    }
}
