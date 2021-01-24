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
                if (string.IsNullOrEmpty(value) || value.Length > 32) {
                    throw new ArgumentException(@"string.IsNullOrEmpty(_folder) || _folder.Length > 24");
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

        public byte[] Blob { get; }

        private readonly OrbDescriptor[] _descriptors;
        public OrbDescriptor[] GetDescriptors()
        {
            return _descriptors;
        }

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

        private byte _counter;
        public byte Counter
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
                if (_distance < 0f || _distance > 256f) {
                    throw new ArgumentException("_distance < 0f || _distance > 256f");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDistance, value);
            }
        }

        public static OrbDescriptor[] BlobToDescriptors(byte[] blob)
        {
            var maxdescriptors = blob.Length / 32;
            if (maxdescriptors < 2 || maxdescriptors > AppConsts.MaxOrbsInImage) {
                throw new ArgumentException("maxdescriptors < 2 || maxdescriptors > AppConsts.MaxOrbsInImage");
            }

            var descriptors = new OrbDescriptor[maxdescriptors];
            for (var i = 0; i < maxdescriptors; i++) {
                descriptors[i] = new OrbDescriptor(blob, i * 32);
            }

            return descriptors;
        }

        public Img(
            string name,
            string folder,
            string hash,
            byte[] blob,
            DateTime lastadded,
            DateTime lastview,
            byte counter,
            DateTime lastcheck,
            string nexthash,
            float distance
            )
        {
            if (string.IsNullOrEmpty(name) || name.Length > 32) {
                throw new ArgumentException("string.IsNullOrEmpty(name) || name.Length > 32");
            }

            Name = name;

            if (string.IsNullOrEmpty(folder) || folder.Length > 32) {
                throw new ArgumentException(@"string.IsNullOrEmpty(_folder) || _folder.Length > 32");
            }

            _folder = folder;

            if (string.IsNullOrEmpty(hash) || hash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != 32");
            }

            Hash = hash;

            if (blob == null || blob.Length == 0 || (blob.Length % 32) != 0) {
                throw new ArgumentException("blob == null || blob.Length == 0 || (blob.Length % 32) != 0");
            }

            Blob = blob;
            _descriptors = BlobToDescriptors(blob);

            LastAdded = lastadded;
            _lastview = lastview;
            _counter = counter;
            _lastcheck = lastcheck;

            if (string.IsNullOrEmpty(nexthash) || nexthash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(nexthash) || nexthash.Length != 32");
            }

            _nexthash = nexthash;

            if (distance < 0f || distance > 256f) {
                throw new ArgumentException("distance < 0f || distance > 256f");
            }

            _distance = distance;
        }
    }
}