using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

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

        public static void SetName(Img img, string name)
        {
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    _nameList.Remove(img.Name);
                    img.SetName(name);
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
            var prevfolder = string.Empty;
            var panel = AppPanels.GetImgPanel(0);
            if (panel != null) {
                var imgprev = panel.Img;
                if (imgprev != null) {
                    prevfolder = FileHelper.NameToFolder(imgprev.Name);
                }
            }

            Img imgX;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    if (_imgList.Count < 2) {
                        imgX = null;
                    }
                    else {
                        var scope = _imgList.Where(e => !FileHelper.NameToFolder(e.Value.Name).Equals(prevfolder));
                        var minlv = scope.Min(e => e.Value.LastView);
                        imgX = scope.FirstOrDefault(e => e.Value.LastView.Equals(minlv)).Value;
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

        private static List<Tuple<string, string, byte[]>> GetShadow()
        {
            var shadow = new List<Tuple<string, string, byte[]>>();
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    foreach (var img in _imgList.Values) {
                        var folder = FileHelper.NameToFolder(img.Name);
                        shadow.Add(Tuple.Create(img.Hash, folder, img.GetVector()));
                    }
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

        public static List<Tuple<string, float>> GetSimilars(Img imgX)
        {
            var folderX = FileHelper.NameToFolder(imgX.Name);
            var shadow = GetShadow();
            var similars = new List<Tuple<string, float>>();
            if (folderX[0].Equals(AppConsts.CharLe)) {
                var bf = new SortedList<string, string>();
                var df = new SortedList<string, float>();
                foreach (var e in shadow) {
                    if (imgX.Hash.Equals(e.Item1)) {
                        continue;
                    }

                    var family = e.Item2[0].Equals(AppConsts.CharLe) ? "-" : e.Item2;
                    var distance = VggHelper.GetDistance(imgX.GetVector(), e.Item3);
                    if (bf.ContainsKey(family)) {
                        if (distance < df[family]) {
                            bf[family] = e.Item1;
                            df[family] = distance;
                        }
                    }
                    else {
                        bf.Add(family, e.Item1);
                        df.Add(family, distance);
                    }
                }

                foreach (var f in bf.Keys) {
                    similars.Add(Tuple.Create(bf[f], df[f]));
                }

                similars.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            }
            else {
                var bestd = 1f;
                var besth = string.Empty;
                var df = new SortedList<string, float>();
                foreach (var e in shadow) {
                    if (imgX.Hash.Equals(e.Item1)) {
                        continue;
                    }

                    var distance = VggHelper.GetDistance(imgX.GetVector(), e.Item3);
                    if (folderX.Equals(e.Item2)) {
                        if (distance < bestd) {
                            bestd = distance;
                            besth = e.Item1;
                        }
                    }
                    else {
                        similars.Add(Tuple.Create(e.Item1, distance));
                    }
                }

                similars.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                if (!string.IsNullOrEmpty(besth)) {
                    similars.Insert(0, Tuple.Create(besth, bestd));
                }
            }

            return similars;
        }
    }
}
