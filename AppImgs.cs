using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

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
            var minlastcheck = GetMinLastCheck();
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    foreach (var e in _imgList.Values) {
                        if (e.BestHash.Equals(img.Hash)) {
                            e.SetLastCheck(minlastcheck);
                        }
                    }

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
                    lv = _imgList.Count > 0 ? _imgList.Min(e => e.Value.LastView).AddSeconds(1) : DateTime.Now;
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

        public static DateTime GetMinLastCheck()
        {
            DateTime lv;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    lv = _imgList.Count > 0 ? _imgList.Min(e => e.Value.LastCheck).AddSeconds(1) : DateTime.Now;
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
                        var scope = _imgList.Values.Where(e => !e.Hash.Equals(e.BestHash) && _imgList.ContainsKey(e.BestHash));
                        if (scope.Count() < 2) {
                            imgX = null;
                        }
                        else {
                            //var mincounter = scope.Min(e => e.Counter);
                            imgX = scope.OrderBy(e => e.LastView).FirstOrDefault();
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
            Img imgX;
            if (Monitor.TryEnter(_imglock, AppConsts.LockTimeout)) {
                try {
                    if (_imgList.Count < 2) {
                        imgX = null;
                    }
                    else {
                        imgX = _imgList.Values.OrderBy(e => e.LastCheck).First();
                        /*
                        var scope = _imgList.Values.OrderBy(e => e.LastView);
                        imgX = null;
                        foreach (var img in scope) {
                            if (img.Hash.Equals(img.BestHash) || !_imgList.ContainsKey(img.BestHash)) {
                                imgX = img;
                                break;
                            }
                        }

                        if (imgX == null) {
                            imgX = scope.OrderBy(e => e.LastCheck).First();
                        }
                        */
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
            var shadow = GetShadow();
            var similars = new List<Tuple<string, float>>();
            foreach (var e in shadow) {
                if (imgX.Hash.Equals(e.Item1)) {
                    continue;
                }

                var distance = VggHelper.GetDistance(imgX.GetVector(), e.Item3);
                similars.Add(Tuple.Create(e.Item1, distance));
            }

            similars.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            return similars;
        }

        public static void GetSimilar(Img imgX, out string besthash, out float distance)
        {
            var shadow = GetShadow();
            besthash = string.Empty;
            distance = 1f;
            var folderX = FileHelper.NameToFolder(imgX.Name);
            foreach (var e in shadow) {
                if (imgX.Hash.Equals(e.Item1)) {
                    continue;
                }

                if (!char.IsDigit(folderX[0])) {
                    if (!folderX.Equals(e.Item2)) {
                        continue;
                    }
                }

                var d = VggHelper.GetDistance(imgX.GetVector(), e.Item3);
                if (d < distance) {
                    besthash = e.Item1;
                    distance = d;
                }
            }

            if (string.IsNullOrEmpty(besthash)) {
                foreach (var e in shadow) {
                    if (imgX.Hash.Equals(e.Item1)) {
                        continue;
                    }

                    var d = VggHelper.GetDistance(imgX.GetVector(), e.Item3);
                    if (d < distance) {
                        besthash = e.Item1;
                        distance = d;
                    }
                }
            }
        }
    }
}
