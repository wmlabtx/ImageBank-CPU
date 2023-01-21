using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ImageBank
{
    public static class AppImgs
    {
        private static readonly SortedList<string, Img> _imgList = new SortedList<string, Img>();
        private static readonly object _imglock = new object();

        public static void Clear()
        {
            _imgList.Clear();
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

        public static void Delete(Img img)
        {
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
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
                        var minlastview = _imgList.Min(e => e.Value.LastView);
                        imgX = _imgList.First(e => e.Value.LastView == minlastview).Value;
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

        public static bool ValidBestHash(Img img)
        {
            bool result;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    result = _imgList.ContainsKey(img.BestHash) && !img.Hash.Equals(img.BestHash);
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

        public static string GetFolder()
        {
            var folders = new int[256];
            var shadow = GetShadow();
            foreach (var e in shadow) {
                var val = int.Parse(e.Value.Folder, System.Globalization.NumberStyles.HexNumber);
                folders[val]++;
            }

            var index = 0;
            var minval = int.MaxValue;
            for (var i = 0; i < folders.Length; i++) {
                if (folders[i] == 0) {
                    index = i;
                    break;
                }

                if (folders[i] < minval) {
                    index = i;
                    minval = folders[i];
                }
            }

            var folder = $"{index:x2}";
            return folder;
        }

        public static float GetDistance(Img imgX) 
        {
            var shadow = GetShadow();
            shadow.Remove(imgX.Hash);
            var distance = 1f;
            foreach (var e in shadow) {
                var cd = ColorHelper.GetDistance(imgX.GetHistogram(), e.Value.GetHistogram());
                distance = Math.Min(distance, cd);
                var vd = VggHelper.GetDistance(imgX.GetVector(), e.Value.GetVector());
                distance = Math.Min(distance, vd);
            }

            return distance;
        }

        public static List<string> GetSimilars(Img imgX)
        {
            var similars = new List<string>();
            if (ValidBestHash(imgX)) {
                similars.Add(imgX.BestHash);
            }
            
            var shadow = GetShadow();
            shadow.Remove(imgX.Hash);
            var clist = new List<Tuple<string, float>>();
            var vlist = new List<Tuple<string, float>>();
            var dlist = new List<Tuple<string, float>>();
            foreach (var e in shadow) {
                var cd = ColorHelper.GetDistance(imgX.GetHistogram(), e.Value.GetHistogram());
                clist.Add(Tuple.Create(e.Key, cd));

                var vd = VggHelper.GetDistance(imgX.GetVector(), e.Value.GetVector());
                vlist.Add(Tuple.Create(e.Key, vd));

                var dd = (float)Math.Abs(imgX.DateTaken.Subtract(e.Value.DateTaken).TotalDays);
                dlist.Add(Tuple.Create(e.Key, dd));
            }

            var cl = clist.OrderBy(e => e.Item2).ToArray();
            var vl = vlist.OrderBy(e => e.Item2).ToArray();
            var dl = dlist.OrderBy(e => e.Item2).ToArray();

            var i = 0;
            while (i < cl.Length && similars.Count < 100) {
                if (!similars.Any(e => e == vl[i].Item1)) {
                    similars.Add(vl[i].Item1);
                }

                if (!similars.Any(e => e == cl[i].Item1)) {
                    similars.Add(cl[i].Item1);
                }

                if (!similars.Any(e => e == dl[i].Item1)) {
                    similars.Add(dl[i].Item1);
                }

                i++;
            }

            return similars;
        }

        /*
        public static void Populate(IProgress<string> progress)
        {
            var shadow = GetShadow();
            foreach (var e in shadow) {
                var rimg = e.Value;
                var distance = GetDistance(rimg);
                if (Math.Abs(rimg.Distance - distance) > 0.0001f) {
                    var shortfilename = rimg.GetShortFileName();
                    var message = $"{shortfilename}: {rimg.Distance:F4} {AppConsts.CharRightArrow} {distance:F4}";
                    progress.Report(message);
                    rimg.SetDistance(distance);
                }
            }
        }
        */

        public static string GetCounters()
        {
            var counters = new int[2];
            var shadow = GetShadow();
            foreach (var e in shadow) {
                if (shadow.ContainsKey(e.Value.BestHash) && !e.Key.Equals(e.Value.BestHash)) {
                    counters[0]++;
                }
                else {
                    counters[1]++;
                }
            }

            var result = $"{counters[0]}/{counters[1]}";
            return result;
        }
    }
}
