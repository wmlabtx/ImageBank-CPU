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
        private static readonly object _imglock = new object();

        public static void Clear()
        {
            _imgList.Clear();
            _nameList.Clear();
        }

        public static int Count()
        {
            int count;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    count = _imgList.Count;
                }
                finally { 
                    Monitor.Exit(_imglock); 
                }
            }
            else {
                throw new Exception();
            }

            return count;
        }

        public static void Add(Img img)
        {
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    _imgList.Add(img.Hash, img);
                    _nameList.Add(img.Name, img);
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }
        }

        public static bool TryGetValue(string hash, out Img img)
        {
            bool result;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    result = _imgList.TryGetValue(hash, out Img _img);
                    img = _img;
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            return result;
        }

        public static bool ContainsHash(string hash)
        {
            bool result;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    result = _imgList.ContainsKey(hash);
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            return result;
        }

        public static bool ContainsName(string name)
        {
            bool result;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    result = _nameList.ContainsKey(name);
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            return result;
        }

        public static void Delete(Img img)
        {
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    _nameList.Remove(img.Name);
                    _imgList.Remove(img.Hash);
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }
        }

        public static DateTime GetMinLastView()
        {
            DateTime lv;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    lv = _imgList.Count > 0 ? _imgList.Min(e => e.Value.LastView).AddSeconds(-1) : DateTime.Now;
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            return lv;
        }

        public static Img GetNextView()
        {
            Img imgX;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    if (_imgList.Count < 2) {
                        imgX = null;
                    }
                    else {
                        var minlv = _imgList.Min(e => e.Value.LastView);
                        imgX = _imgList.First(e => e.Value.LastView.Equals(minlv)).Value;
                    }
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            return imgX;
        }

        public static string[] GetKeys()
        {
            string[] result;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    result = _imgList.Keys.ToArray();
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            return result;
        }

        private static SortedList<string, Img> GetShadow()
        {
            var shadow = new SortedList<string, Img>();
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    shadow = new SortedList<string, Img>(_imgList);
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            return shadow;
        }

        public static List<string> GetSimilars(Img imgX)
        {
            var similars = new List<string>();
            var shadow = GetShadow();
            shadow.Remove(imgX.Hash);
            var nexthash = imgX.NextHash;
            while (shadow.ContainsKey(nexthash)) {
                similars.Add(nexthash);
                var imgY = shadow[nexthash];
                shadow.Remove(nexthash);
                nexthash = imgY.NextHash;
            }

            var clist = new List<Tuple<string, float>>();
            var vlist = new List<Tuple<string, float>>();
            foreach (var e in shadow) {
                var cd = ColorHelper.GetDistance(imgX.GetHistogram(), e.Value.GetHistogram());
                clist.Add(Tuple.Create(e.Key, cd));
                var vd = VggHelper.GetDistance(imgX.GetVector(), e.Value.GetVector());
                vlist.Add(Tuple.Create(e.Key, vd));
            }

            clist.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            vlist.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            var i = 0;
            while (i < clist.Count && i < vlist.Count && similars.Count < 100) {
                var chash = clist[i].Item1;
                if (shadow.ContainsKey(chash)) {
                    similars.Add(chash);
                    shadow.Remove(chash);
                }

                var vhash = vlist[i].Item1;
                if (shadow.ContainsKey(vhash)) {
                    similars.Add(vhash);
                    shadow.Remove(vhash);
                }

                i++;
            }

            return similars;
        }
    }
}
