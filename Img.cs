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
                if (string.IsNullOrEmpty(value) || value.Length > 128) {
                    throw new ArgumentException(@"string.IsNullOrEmpty(_folder) || _folder.Length > 128");
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

        private ulong[] _descriptors;
        public ulong[] GetDescriptors()
        {
            return _descriptors;
        }

        private byte[] _mapdescriptors;
        public byte[] GetMapDescriptors()
        {
            return _mapdescriptors;
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

        private int _counter;
        public int Counter
        {
            get => _counter;
            set
            {
                _counter = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrCounter, value);
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

        public Img(
            string name,
            string folder,
            string hash,
            byte[] blob,
            byte[] mapdescriptors,
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

            if (string.IsNullOrEmpty(folder) || folder.Length > 128) {
                throw new ArgumentException(@"string.IsNullOrEmpty(_folder) || _folder.Length > 128");
            }

            _folder = folder;

            if (string.IsNullOrEmpty(hash) || hash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != 32");
            }

            Hash = hash;

            if (blob == null) {
                throw new ArgumentException("blob == null");
            }

            Blob = blob;
            _descriptors = ImageHelper.ArrayTo64(blob);

            _mapdescriptors = mapdescriptors;

            Phash = phash;

            LastAdded = lastadded;
            _lastview = lastview;
            _counter = counter;
            _lastcheck = lastcheck;

            if (string.IsNullOrEmpty(nexthash) || nexthash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(nexthash) || nexthash.Length != 32");
            }

            _nexthash = nexthash;

            if (distance < 0f || distance > AppConsts.MaxDistance) {
                throw new ArgumentException("distance < 0f || distance > AppConsts.MaxDistance");
            }

            _distance = distance;
        }
    }
}