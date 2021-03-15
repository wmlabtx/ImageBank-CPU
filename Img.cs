using System;
using System.IO;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }

        private string _folder;
        public string Folder
        {
            get => _folder;
            set
            {
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

        public byte[] Blob { get; private set; }

        private readonly ulong[] _descriptors;
        public ulong[] GetDescriptors()
        {
            return _descriptors;
        }

        public ulong Phash { get; }

        public DateTime LastAdded { get; }

        private DateTime _lastview;
        public DateTime LastView
        {
            get => _lastview;
            set
            {
                _lastview = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck
        {
            get => _lastcheck;
            set
            {
                _lastcheck = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastCheck, value);
            }
        }

        private string _nexthash;
        public string NextHash
        {
            get => _nexthash;
            set
            {
                _nexthash = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNextHash, value);
            }
        }

        private float _distance;
        public float Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                if (_distance < 0 || _distance > AppConsts.MaxDistance) {
                    throw new ArgumentException("_distance < 0 || _distance > AppConsts.MaxDistance");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDistance, value);
            }
        }

        private int _counter;
        public int Counter
        {
            get => _counter;
            set
            {
                _counter = value;
                if (_counter < 0) {
                    throw new ArgumentException("_counter < 0");
                }
                
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrCounter, value);
            }
        }

        private int _width;
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                if (_width <= 0)
                {
                    throw new ArgumentException("_width <= 0");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrWidth, value);
            }
        }

        private int _height;
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                if (_height <= 0)
                {
                    throw new ArgumentException("_height <= 0");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrHeight, value);
            }
        }

        private int _size;
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                if (_size <= 0)
                {
                    throw new ArgumentException("_size <= 0");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrSize, value);
            }
        }

        public int Ratio
        {
            get {
                    if (Width >= Height)
                    {
                        return Height * 100 / Width;
                    }
                    else
                    {
                        return Width * 100 / Height;
                    }
                }
        } 

        public Img(
            string name,
            string folder,
            string hash,
            int width,
            int height,
            int size,
            byte[] blob,
            ulong phash,
            DateTime lastadded,
            DateTime lastview,
            int counter,
            DateTime lastcheck,
            string nexthash,
            float distance
            )
        {
            if (string.IsNullOrEmpty(name) || name.Length > 32) {
                throw new ArgumentException("string.IsNullOrEmpty(name) || name.Length > 32");
            }

            Name = name;

            if (string.IsNullOrEmpty(folder)) {
                throw new ArgumentException(@"string.IsNullOrEmpty(folder)");
            }

            _folder = folder;

            if (string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength) {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength");
            }

            Hash = hash;

            _width = width;
            _height = height;
            _size = size;

            Blob = blob ?? throw new ArgumentException("blob == null");
            _descriptors = ImageHelper.ArrayTo64(blob);

            Phash = phash;

            LastAdded = lastadded;
            _lastview = lastview;
            _lastcheck = lastcheck;

            _counter = counter;

            if (string.IsNullOrEmpty(nexthash) || nexthash.Length != AppConsts.HashLength) {
                throw new ArgumentException("string.IsNullOrEmpty(nexthash) || nexthash.Length != AppConsts.HashLength");
            }

            _nexthash = nexthash;

            if (distance < 0f || distance > AppConsts.MaxDistance) {
                throw new ArgumentException("distance < 0f || distance > AppConsts.MaxDistance");
            }

            _distance = distance;
        }
    }
}