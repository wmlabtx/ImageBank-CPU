using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public int Folder { get; }

        public string FileName => $"{AppConsts.PathHp}\\{Folder:D2}\\{Id:D6}{AppConsts.MzxExtension}";

        public string Hash { get; }

        public byte[] AkazeCentroid { get; }
        public byte[] AkazeMirrorCentroid { get; }

        private int _akazepairs;
        public int AkazePairs {
            get => _akazepairs;
            set {
                _akazepairs = value;
                if (_akazepairs < 0 || _akazepairs > AppConsts.MaxDescriptors) {
                    throw new ArgumentException("_akazepairs < 0 || _akazepairs > AppConsts.MaxDescriptors");
                }

                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrAkazePairs, value);
            }
        }

        private DateTime _lastchanged;
        public DateTime LastChanged {
            get => _lastchanged;
            set {
                _lastchanged = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastChanged, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck {
            get => _lastcheck;
            set {
                _lastcheck = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastCheck, value);
            }
        }

        private string _nexthash;
        public string NextHash {
            get => _nexthash;
            set {
                _nexthash = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrNextHash, value);
            }
        }

        private int _counter;
        public int Counter {
            get => _counter;
            set {
                _counter = value;
                if (_counter < 0) {
                    throw new ArgumentException("_counter < 0");
                }

                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrCounter, value);
            }
        }

        public int Width { get; }
        public int Height { get; }
        public int Size { get; }

        public Img(
            int id,
            string hash,

            int width,
            int height,
            int size,

            byte[] akazecentroid,
            byte[] akazemirrorcentroid,
            int akazepairs,

            DateTime lastchanged,
            DateTime lastview,
            DateTime lastcheck,

            string nexthash,
            int counter
            ) {

            if (id <= 0) {
                throw new ArgumentException("id <= 0");
            }

            Id = id;

            if (string.IsNullOrEmpty(hash) || hash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != 32");
            }

            Hash = hash;
            var token  = ulong.Parse(Hash.Substring(0, 16), System.Globalization.NumberStyles.AllowHexSpecifier);
            Folder = (int)((token % 50) + 1);

            if (width <= 0) {
                throw new ArgumentException("width <= 0");
            }

            Width = width;

            if (height <= 0) {
                throw new ArgumentException("height <= 0");
            }

            Height = height;

            if (size <= 0) {
                throw new ArgumentException("size <= 0");
            }

            Size = size;
            
            if (akazepairs < 0 || akazepairs > AppConsts.MaxDescriptors) {
                throw new ArgumentException("akazepairs < 0 || akazepairs > AppConsts.MaxDescriptors");
            }

            if (akazecentroid == null || akazecentroid.Length == 0) {
                throw new ArgumentException("akazecentroid == null || akazecentroid.Length == 0");
            }

            AkazeCentroid = akazecentroid;

            if (akazemirrorcentroid == null || akazemirrorcentroid.Length == 0) {
                throw new ArgumentException("akazemirrorcentroid == null || akazemirrorcentroid.Length == 0");
            }

            AkazeMirrorCentroid = akazemirrorcentroid;

            _akazepairs = akazepairs;

            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;

            if (string.IsNullOrEmpty(nexthash) || nexthash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(nexthash) || nexthash.Length != 32");
            }

            _nexthash = nexthash;

            if (counter < 0) {
                throw new ArgumentException("counter < 0");
            }

            _counter = counter;
        }
    }
}