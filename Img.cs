using OpenCvSharp;
using System;

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

        public Img(
            string name,
            ulong hash,
            int width,
            int heigth,
            int size,
            Mat descriptors,
            int folder,
            int counter,
            DateTime lastview,
            DateTime lastcheck,
            string nextname
            )
        {
            Name = name;
            Hash = hash;
            Width = width;
            Heigth = heigth;
            Size = size;

            _descriptors = descriptors;
            _folder = folder;
            _counter = counter;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _nextname = nextname;
        }
    }
}