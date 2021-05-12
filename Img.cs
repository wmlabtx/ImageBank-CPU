using System;
using System.IO;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }

        private string _folder;
        public string Folder {
            get => _folder;
            set {
                if (string.IsNullOrEmpty(value)) {
                    throw new ArgumentException(@"string.IsNullOrEmpty(value)");
                }

                var oldfilename = FileName;
                _folder = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrFolder, value);
                var newfilename = FileName;
                var directory = Path.GetDirectoryName(newfilename);
                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                File.Move(oldfilename, newfilename);
            }
        }

        public string FileName => $"{AppConsts.PathHp}\\{Folder}\\{Name}{AppConsts.MzxExtension}";

        public string Hash { get; }

        private int _akazepairs;
        public int AkazePairs {
            get => _akazepairs;
            set {
                _akazepairs = value;
                if (_akazepairs < 0 || _akazepairs > AppConsts.MaxDescriptors) {
                    throw new ArgumentException("_akazepairs < 0 || _akazepairs > AppConsts.MaxDescriptors");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrAkazePairs, value);
            }
        }

        public byte[] AkazeCentroid { get; }
        public byte[] AkazeMirrorCentroid { get; }

        private DateTime _lastchanged;
        public DateTime LastChanged {
            get => _lastchanged;
            set {
                _lastchanged = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastChanged, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck {
            get => _lastcheck;
            set {
                _lastcheck = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastCheck, value);
            }
        }

        private string _nexthash;
        public string NextHash {
            get => _nexthash;
            set {
                _nexthash = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNextHash, value);
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

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrCounter, value);
            }
        }

        public int Width { get; }
        public int Height { get; }
        public int Size { get; }
        public int Id { get; }

        public Img(
            int id,
            string name,
            string folder,
            string hash,

            int width,
            int height,
            int size,

            int akazepairs,
            byte[] akazecentroid,
            byte[] akazemirrorcentroid,

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

            if (string.IsNullOrEmpty(name) || name.Length > 32) {
                throw new ArgumentException("string.IsNullOrEmpty(name) || name.Length > 32");
            }

            Name = name;

            if (string.IsNullOrEmpty(folder) || name.Length > 12) {
                throw new ArgumentException("string.IsNullOrEmpty(folder) || folder.Length > 12");
            }

            _folder = folder;

            if (string.IsNullOrEmpty(hash) || hash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != 32");
            }

            Hash = hash;

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

            _akazepairs = akazepairs;

            if (akazecentroid == null || akazecentroid.Length != 488) {
                throw new ArgumentException("akazecentroid == null || akazecentroid.Length != 488");
            }

            AkazeCentroid = akazecentroid;

            if (akazemirrorcentroid == null || akazemirrorcentroid.Length != 488) {
                throw new ArgumentException("akazemirrorcentroid == null || akazemirrorcentroid.Length != 488");
            }

            AkazeMirrorCentroid = akazemirrorcentroid;

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