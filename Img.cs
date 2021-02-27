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

        public string FileName => $"{AppConsts.PathHp}\\{Folder}\\{Name}{AppConsts.DbxExtension}";

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

        private string _history;
        public string History
        {
            get => _history;
            set
            {
                _history = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrHistory, value);
            }
        }

        public bool IsInHistory(string hash)
        {
            if (string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength)
            {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength");
            }

            var offset = 0;
            while (offset + AppConsts.HashLength <= _history.Length)
            {
                if (string.CompareOrdinal(_history, offset, hash, 0, AppConsts.HashLength) == 0)
                {
                    return true;
                }

                offset += AppConsts.HashLength;
            }

            return false;
        }

        public void AddToHistory(string hash)
        {
            if (string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength)
            {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength");
            }

            if (IsInHistory(hash))
            {
                return;
            }

            _history = string.Concat(_history, hash);
            ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrHistory, _history);
        }

        public void RemoveFromHistory(string hash)
        {
            if (string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength)
            {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength");
            }

            var offset = 0;
            while (offset + AppConsts.HashLength <= _history.Length)
            {
                if (string.CompareOrdinal(_history, offset, hash, 0, AppConsts.HashLength) == 0)
                {
                    _history = _history.Remove(offset, AppConsts.HashLength);
                    ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrHistory, _history);
                    return;
                }

                offset += AppConsts.HashLength;
            }
        }

        public int Counter
        {
            get
            {
                return _history.Length / AppConsts.HashLength;
            }
        }

        public Img(
            string name,
            string folder,
            string hash,
            byte[] blob,
            ulong phash,
            DateTime lastadded,
            DateTime lastview,
            string history,
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

            if (string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength) {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != AppConsts.HashLength");
            }

            Hash = hash;
            Blob = blob ?? throw new ArgumentException("blob == null");
            _descriptors = ImageHelper.ArrayTo64(blob);

            Phash = phash;

            LastAdded = lastadded;
            _lastview = lastview;
            _history = history;
            _lastcheck = lastcheck;

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