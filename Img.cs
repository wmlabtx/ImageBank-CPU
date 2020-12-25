using System;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }

        public int Width { get; }

        public int Heigth { get; }

        public int Size { get; }

        private OrbDescriptor[] _descriptors;
        public OrbDescriptor[] GetDescriptors()
        {
            return _descriptors;
        }
        public void SetDescriptors(OrbDescriptor[] descriptors)
        {
            _descriptors = descriptors;
            var array = ImageHelper.DescriptorsToArray(descriptors);
            ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrDescriptors, array);
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

        public DateTime LastAdded { get; }

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

        private float _sim;
        public float Sim
        {
            get => _sim;
            set
            {
                _sim = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrSim, value);
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

        public Img(
            string name,
            int width,
            int heigth,
            int size,
            OrbDescriptor[] descriptors,
            int folder,
            DateTime lastview,
            DateTime lastcheck,
            DateTime lastadded,
            string nextname,
            float sim,
            string family,
            byte counter
            )
        {
            Name = name;
            Width = width;
            Heigth = heigth;
            Size = size;
            LastAdded = lastadded;

            _descriptors = descriptors;
            _folder = folder;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _nextname = nextname;
            _sim = sim;

            _family = family;
            _counter = counter;
        }
    }
}