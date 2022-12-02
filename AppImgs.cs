using System;
using System.Collections.Generic;
using System.Linq;

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

        public static bool ContainsId(string hash)
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

        public static Img GetNextView()
        {
            string id;
            Img imgX;
            var rand = AppVars.IRandom(0, 2);
            if (rand == 0) {
                rand = AppVars.IRandom(0, _imgList.Keys.Count - 1);
                id = _imgList.Keys[rand];
            }
            else {
                var minlv = _imgList.Values.Min(e => e.LastView);
                var nextday = minlv.AddDays(1);
                var oldday = _imgList.Values.Where(e => e.LastView < nextday).Select(e => e.Hash).ToArray();
                rand = AppVars.IRandom(0, oldday.Length - 1);
                id = oldday[rand];
            }

            imgX = _imgList[id];
            return imgX;
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
    }
}
