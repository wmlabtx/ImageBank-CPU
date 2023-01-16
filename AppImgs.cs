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

        public static int Count(string family)
        {
            int count;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    count = _imgList.Count(e => e.Value.Family.Equals(family, StringComparison.OrdinalIgnoreCase));
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
                        var shadow = GetShadow();
                        var vlist = new SortedList<string, Tuple<string, DateTime>>();
                        foreach (var e in shadow) {
                            if (vlist.ContainsKey(e.Value.Family)) {
                                if (vlist[e.Value.Family].Item2 < e.Value.LastView) {
                                    vlist[e.Value.Family] = Tuple.Create(e.Key, e.Value.LastView);
                                }
                            }
                            else {
                                vlist.Add(e.Value.Family, Tuple.Create(e.Key, e.Value.LastView));
                            }
                        }

                        var minlv = vlist.Min(e => e.Value.Item2);
                        var minhash = vlist.First(e => e.Value.Item2 == minlv).Value.Item1;
                        imgX = _imgList[minhash];
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
            var clist = new SortedList<string, Tuple<string, float>>();
            var vlist = new SortedList<string, Tuple<string, float>>();
            var dlist = new SortedList<string, Tuple<string, float>>();
            foreach (var e in shadow) {
                if (imgX.Family.Equals(e.Value.Family, StringComparison.OrdinalIgnoreCase)) {
                    similars.Add(e.Key);
                    continue;
                }

                var cd = ColorHelper.GetDistance(imgX.GetHistogram(), e.Value.GetHistogram());
                if (clist.ContainsKey(e.Value.Family)) {
                    if (cd < clist[e.Value.Family].Item2) {
                        clist[e.Value.Family] = Tuple.Create(e.Key, cd);
                    }
                }
                else {
                    clist.Add(e.Value.Family, Tuple.Create(e.Key, cd));
                }

                var vd = VggHelper.GetDistance(imgX.GetVector(), e.Value.GetVector());
                if (vlist.ContainsKey(e.Value.Family)) {
                    if (vd < vlist[e.Value.Family].Item2) {
                        vlist[e.Value.Family] = Tuple.Create(e.Key, vd);
                    }
                }
                else {
                    vlist.Add(e.Value.Family, Tuple.Create(e.Key, vd));
                }

                var dd = (float)Math.Abs(imgX.DateTaken.Subtract(e.Value.DateTaken).TotalDays);
                if (dlist.ContainsKey(e.Value.Family)) {
                    if (dd < dlist[e.Value.Family].Item2) {
                        dlist[e.Value.Family] = Tuple.Create(e.Key, dd);
                    }
                }
                else {
                    dlist.Add(e.Value.Family, Tuple.Create(e.Key, dd));
                }
            }

            var cl = clist.OrderBy(e => e.Value.Item2).ToArray();
            var vl = vlist.OrderBy(e => e.Value.Item2).ToArray();
            var dl = dlist.OrderBy(e => e.Value.Item2).ToArray();

            var i = 0;
            var xlist = new List<Tuple<string, string>>();
            while (i < cl.Length && xlist.Count < 100) {
                var cf = cl[i].Key;
                if (!xlist.Any(e => e.Item1 == cf)) {
                    var ch = cl[i].Value.Item1;
                    xlist.Add(Tuple.Create(cf, cl[i].Value.Item1));
                }

                var vf = vl[i].Key;
                if (!xlist.Any(e => e.Item1 == vf)) {
                    var vh = vl[i].Value.Item1;
                    xlist.Add(Tuple.Create(vf, vl[i].Value.Item1));
                }

                var df = dl[i].Key;
                if (!xlist.Any(e => e.Item1 == df)) {
                    var dh = dl[i].Value.Item1;
                    xlist.Add(Tuple.Create(df, dl[i].Value.Item1));
                }

                i++;
            }

            similars.AddRange(xlist.Select(e => e.Item2).ToList());
            return similars;
        }

        /*
        public static void RenameFamily(string of, string nf)
        {
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    var scope = _imgList.Values.Where(e => e.Family.Equals(of, StringComparison.OrdinalIgnoreCase)).ToArray();
                    foreach (var img in scope) {
                        img.SetFamily(nf);
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
        */

        /*
        public static void Populate()
        {
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    var shadow = GetShadow();
                    foreach (var e in shadow) {
                        var filename = FileHelper.NameToFileName(hash:e.Key, name:e.Value.Name);
                        var lastmodified = File.GetLastWriteTime(filename);
                        if (lastmodified > DateTime.Now) {
                            lastmodified = DateTime.Now;
                        }

                        var imagedata = FileHelper.ReadEncryptedFile(filename);
                        using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                            var datetaken = BitmapHelper.GetDateTaken(magickImage, DateTime.Now);
                            if (datetaken < lastmodified) {
                                lastmodified = datetaken;
                            }
                        }

                        e.Value.SetDateTaken(lastmodified);
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
        */
    }
}
