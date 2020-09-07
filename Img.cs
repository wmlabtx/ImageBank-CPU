using OpenCvSharp;
using System;
using System.Diagnostics.Contracts;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }

        public ulong Hash { get; }

        public int Width { get; }

        public int Heigth { get; }

        public int Size { get; }
         
        private Mat _descriptors;

        public Mat GetDescriptors()
        {
            return _descriptors;
        }
        public void SetDescriptors(Mat descriptors)
        {
            _descriptors = descriptors;
            if (_descriptors != null) {
                _descriptors.GetArray(out byte[] buffer);
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDescriptors, buffer);
            }
        }

        private int _folder;
        public int Folder
        {
            get => _folder;
            set
            {
                _folder = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrFolder, value);
            }
        }
        public string FileName => Helper.GetFileName(Name, Folder);

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

        private string _nextname;
        public string NextName
        {
            get => _nextname;
            set
            {
                _nextname = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNextName, value);
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

        public bool IsInHistory(string name)
        {
            var offset = 0;
            while (offset + 10 <= _history.Length) {
                if (string.CompareOrdinal(_history, offset, name, 0, 10) == 0) {
                    return true;
                }

                offset += 10;
            }

            return false;
        }

        public void AddToHistory(string name)
        {
            Contract.Requires(name != null && name.Length == 10);

            if (!IsInHistory(name)) {
                return;
            }

            _history = string.Concat(_history, name);
            ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrHistory, _history);
        }

        public void RemoveFromHistory(string name)
        {
            Contract.Requires(name != null && name.Length == 10);

            var offset = 0;
            while (offset + 10 <= _history.Length) {
                if (string.CompareOrdinal(_history, offset, name, 0, 10) == 0) {
                    _history = _history.Remove(offset, 10);
                    ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrHistory, _history);
                    return;
                }

                offset += 10;
            }
        }

        public int Counter
        {
            get
            {
                return _history.Length / 10;
            }
        }

        private string _family;
        public string Family
        {
            get => _family;
            set
            {
                _family = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrFamily, value);
            }
        }

        public Img(
            string name,
            ulong hash,
            int width,
            int heigth,
            int size,
            Mat descriptors,
            int folder,
            DateTime lastview,
            DateTime lastcheck,
            string nextname,
            string history,
            string family
            )
        {
            Name = name;
            Hash = hash;
            Width = width;
            Heigth = heigth;
            Size = size;

            _descriptors = descriptors;
            _folder = folder;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _nextname = nextname;

            _history = history;
            _family = family;
        }
    }
}