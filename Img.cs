using System;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }
        public string Hash { get; }
        public int Width { get; }
        public int Height { get; }
        public int Size { get; }
        public DateTime? DateTaken { get; }
        public string MetaData { get; }

        private string _lastname;
        public string LastName {
            get => _lastname;
            set {
                _lastname = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrLastName, value);
            }
        }

        private string _besthash;
        public string BestHash {
            get => _besthash;
            set {
                _besthash = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrBestHash, value);
            }
        }

        private float _distance;
        public float Distance {
            get => _distance;
            set {
                _distance = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrDistance, value);
            }
        }

        private DateTime _lastchanged;
        public DateTime LastChanged {
            get => _lastchanged;
            set {
                _lastchanged = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrLastChanged, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck {
            get => _lastcheck;
            set {
                _lastcheck = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrLastCheck, value);
            }
        }

        private int _generation;
        public int Generation {
            get => _generation;
            set {
                _generation = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrGeneration, value);
            }
        }

        public Img(
            string name,
            string hash,
            int width,
            int height,
            int size,
            DateTime? datetaken,
            string metadata,
            string lastname,
            string besthash,
            float distance,
            DateTime lastchanged,
            DateTime lastview,
            DateTime lastcheck,
            int generation
            )
        {
            Name = name;
            Hash = hash;
            Width = width;
            Height = height;
            Size = size;
            DateTaken = datetaken;
            MetaData = metadata;

            _lastname = lastname;
            _besthash = besthash;
            _distance = distance;
            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _generation = generation;
        }
    }
}