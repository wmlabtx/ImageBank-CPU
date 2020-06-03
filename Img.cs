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
            DateTime lastview
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
            _lastview = lastview;
        }
    }
}