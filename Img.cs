using System;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }
        public string Hash { get; }
        public DateTime? DateTaken { get; }

        private int _family;
        public int Family {
            get => _family;
            set {
                _family = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrFamily, value);
            }
        }

        private string _bestnames;
        public string BestNames {
            get => _bestnames;
            set {
                _bestnames = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrBestNames, value);
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
            DateTime? datetaken,
            int family,
            string bestnames,
            DateTime lastchanged,
            DateTime lastview,
            DateTime lastcheck,
            int generation
            )
        {
            Name = name;
            Hash = hash;
            DateTaken = datetaken;
            _family = family;
            _bestnames = bestnames;
            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _generation = generation;
        }
    }
}