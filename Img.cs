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

        private byte[] _diff;
        public byte[] Diff
        {
            set
            {
                _diff = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDiff, value);
            }
        }

        public byte[] GetDiff()
        {
            return _diff;
        }

        private DateTime _lastchanged;
        public DateTime LastChanged
        {
            get => _lastchanged;
            set
            {
                _lastchanged = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastChanged, value);
            }
        }

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

        public int Width { get; }
        public int Height { get; }
        public int Size { get; }
        public int Id { get; }

        private int _lastid;
        public int LastId
        {
            get => _lastid;
            set
            {
                _lastid = value;
                if (_lastid < 0) {
                    throw new ArgumentException("_lastid < 0");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastId, value);
            }
        }

        public byte[] PBlob { get; private set; }

        private readonly ulong[] _hashes;
        public ulong[] GetHashes()
        {
            return _hashes;
        }

        private int _distance;
        public int Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                if (_distance < 0) {
                    throw new ArgumentException("_distance < 0");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDistance, value);
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
            DateTime lastchanged,
            DateTime lastview,
            int counter,
            DateTime lastcheck,
            string nexthash,
            byte[] diff,
            int id,
            int lastid,
            byte[] pblob,
            int distance
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

            Blob = blob ?? throw new ArgumentException("blob == null");
            _descriptors = ImageHelper.ArrayTo64(blob);

            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;

            if (counter < 0) {
                throw new ArgumentException("counter < 0");
            }

            _counter = counter;
            _nexthash = nexthash;

            if (diff == null || diff.Length > 100) { 
                throw new ArgumentException("diff == null || diff.Length > 100");
            }

            _diff = diff;

            if (id <= 0) {
                throw new ArgumentException("id <= 0");
            }

            Id = id;

            if (lastid < 0) {
                throw new ArgumentException("lastid < 0");
            }

            _lastid = lastid;

            PBlob = pblob ?? throw new ArgumentException("pblob == null");
            _hashes = ImageHelper.ArrayTo64(pblob);

            if (distance < 0) {
                throw new ArgumentException("distance < 0");
            }

            _distance = distance;
        }
    }
}