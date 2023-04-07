using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    var lc = _imgList.Min(e => e.Value.LastCheck);
                    foreach (var e in _imgList.Where(e => e.Value.Next.Equals(img.Hash))) { 
                        e.Value.SetLastCheck(lc);
                    }
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
            Img imgX = null;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    if (_imgList.Count < 2) {
                        imgX = null;
                    }
                    else {
                        var minrev = int.MaxValue;
                        var mindistance = float.MaxValue;
                        foreach (var img in _imgList.Values) {
                            if (img.Hash.Equals(img.Next)) {
                                continue;
                            }

                            if (!_imgList.TryGetValue(img.Next, out var imgn)) {
                                continue;
                            }

                            var rev = Math.Min(img.Review, imgn.Review);
                            var lv = img.LastView;
                            if (imgn.LastView > lv) {
                                lv = imgn.LastView;
                            }

                            var distance = img.Distance;
                            if (rev < minrev) {
                                minrev = rev;
                                mindistance = distance;
                                imgX = img;
                            }
                            else {
                                if (rev == minrev && distance < mindistance) {
                                    mindistance = distance;
                                    imgX = img;
                                }
                            }
                        }

                        if (!_imgList.TryGetValue(imgX.Next, out var imgY)) {
                            imgX = null;
                        }
                        else {
                            imgX.SetReview((short)(imgX.Review + 1));
                            imgY.SetReview((short)(imgY.Review + 1));
                        }
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

        public static Img GetNextCheck()
        {
            Img imgX = null;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    foreach (var img in _imgList.Values.OrderBy(e => e.LastView)) {
                        if (img.Hash.Equals(img.Next) || !_imgList.ContainsKey(img.Next)) { 
                            imgX = img;
                            break;
                        }

                        if (imgX == null || (imgX != null && img.LastCheck < imgX.LastCheck)) {
                            imgX = img;
                        }
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

        public static SortedList<string, Img> GetShadow()
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
            var ifolder = AppVars.IRandom(0, 255);
            /*
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
            */

            var folder = $"{ifolder:x2}";
            return folder;
        }

        public static List<string> GetSimilars(Img imgX)
        {
            var similars = new List<string>();
            similars.Add(imgX.Next);

            /*
            var shadow = GetShadow();
            shadow.Remove(imgX.Hash);
            var vlist = new List<Tuple<string, float>>();
            var dlist = new List<Tuple<string, float>>();
            foreach (var e in shadow) {
                var vd = VggHelper.GetDistance(imgX.GetVector(), e.Value.GetVector());
                vlist.Add(Tuple.Create(e.Key, vd));
                var dd = (float)Math.Abs(imgX.DateTaken.Subtract(e.Value.DateTaken).TotalDays);
                dlist.Add(Tuple.Create(e.Key, dd));
            }

            var vl = vlist.OrderBy(e => e.Item2).ToArray();
            var dl = dlist.OrderBy(e => e.Item2).ToArray();

            var i = 0;
            while (i < vl.Length && similars.Count < 100) {
                if (!similars.Any(e => e == vl[i].Item1)) {
                    similars.Add(vl[i].Item1);
                }

                if (!similars.Any(e => e == dl[i].Item1)) {
                    similars.Add(dl[i].Item1);
                }

                i++;
            }
            */

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
            var counters = new SortedList<int, int>();
            var shadow = GetShadow();
            foreach (var e in shadow.Values) {
                if (counters.ContainsKey(e.Review)) {
                    counters[e.Review]++;
                }
                else {
                    counters.Add(e.Review, 1);
                }
            }

            var sb = new StringBuilder();
            var counterkeymax = counters.Keys[counters.Keys.Count - 1];
            for (var i = 0; i <= counterkeymax; i++) {
                var val = counters.ContainsKey(i) ? counters[i] : 0;
                if (i > 0) {
                    sb.Append('/');
                }

                sb.Append(val);
            }

            var result = sb.ToString();
            return result;
        }
    }
}
