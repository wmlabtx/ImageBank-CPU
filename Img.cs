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

        private short[] _descriptors;
        public short[] GetDescriptors()
        {
            return _descriptors;
        }
        public void SetDescriptors(short[] descriptors)
        {
            _descriptors = descriptors;
            var buffer = ImageHelper.DescriptorsToBuffer(_descriptors);
            ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrColors, buffer);
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

        private DateTime _lastadded;
        public DateTime LastAdded
        {
            get => _lastadded;
            set
            {
                _lastadded = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastAdded, value);
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

        private float _distance;
        public float Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDistance, value);
            }
        }

        public Img(
            string name,
            ulong hash,
            int width,
            int heigth,
            int size,
            short[] descriptors,
            int folder,
            DateTime lastview,
            DateTime lastcheck,
            DateTime lastadded,
            string nextname,
            float distance,
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
            _lastadded = lastadded;
            _nextname = nextname;
            _distance = distance;

            _family = family;
        }
    }
}