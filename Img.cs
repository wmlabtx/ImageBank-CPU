using System;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }

        public ulong Hash { get; }

        public ulong PHash { get; }

        public int Width { get; }

        public int Heigth { get; }

        public int Size { get; }

        public Scd Scd { get; }

        private readonly ulong[] _descriptors;

        public ulong[] GetDescriptors()
        {
            return _descriptors;
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

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrPath, value);
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

        private string _dt;
        public string Dt
        {
            get => _dt;
            set
            {
                _dt = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDt, value);
            }
        }

        private float _dv;
        public float Dv
        {
            get => _dv;
            set
            {
                _dv = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDv, value);
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

        public Img(
            string name,
            ulong hash,
            ulong phash,
            int width,
            int heigth,
            int size,
            Scd scd,
            ulong[] descriptors,
            int folder,
            string path,
            int counter,
            DateTime lastcheck,
            DateTime lastview,
            string dt,
            float dv,
            string nextname
            )
        {
            Name = name;
            Hash = hash;
            PHash = phash;
            Width = width;
            Heigth = heigth;
            Size = size;
            Scd = scd;
            _descriptors = descriptors;

            _folder = folder;
            _path = path;
            _counter = counter;
            _lastcheck = lastcheck;
            _lastview = lastview;

            _dt = dt;
            _dv = dv;
            _nextname = nextname;
        }
    }
}