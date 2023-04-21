using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

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
                        var minrev = short.MaxValue;
                        short rev;
                        foreach (var img in _imgList.Values) {
                            if (img.Next.Equals(img.Hash) || !_imgList.TryGetValue(img.Next, out Img imgnext)) {
                                continue;
                            }

                            rev = Math.Min(img.Review, imgnext.Review);
                            if (imgX == null || rev < minrev || (rev == minrev && img.Distance < imgX.Distance)) {
                                minrev = rev;
                                imgX = img;
                            }
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
                    var minrev = short.MaxValue;
                    short rev;
                    foreach (var img in _imgList.Values) {
                        if (img.Next.Equals(img.Hash) || !_imgList.TryGetValue(img.Next, out Img imgnext)) {
                            rev = img.Review;
                        }
                        else {
                            rev = Math.Min(img.Review, imgnext.Review);
                        }

                        if (imgX == null || rev < minrev || (rev == minrev && img.LastCheck < imgX.LastCheck)) { 
                            minrev = rev;
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

        public static void Populate(IProgress<string> progress)
        {
            var shadow = GetShadow();
            var now = DateTime.Now;
            var counter = 0;
            foreach (var e in shadow) {
                counter++;
                var rimg = e.Value;
                if (rimg.LastView.Year <= 2020) {
                    rimg.SetReview(0);
                    var shortfilename = rimg.GetShortFileName();
                    var message = $"{counter}) {shortfilename}: {rimg.Distance:F4} ({rimg.Review})";
                    progress.Report(message);
                }
                else {
                    var days = now.Subtract(rimg.LastView).Days;
                    if (days < 3) {
                        rimg.SetReview(2);
                        var shortfilename = rimg.GetShortFileName();
                        var message = $"{counter}) {shortfilename}: {rimg.Distance:F4} ({rimg.Review})";
                        progress.Report(message);
                    }
                }
            }
        }

        public static string GetCounters()
        {
            var total = 0;
            var revcount = 0;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    total = _imgList.Count;
                    var minrev = _imgList.Values.Min(e => e.Review);
                    revcount = _imgList.Values.Count(e => e.Review == minrev);
                }
                finally {
                    Monitor.Exit(_imglock);
                }
            }
            else {
                throw new Exception();
            }

            var result = $"{revcount}/{total}";
            return result;
        }
    }
}
