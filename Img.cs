using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }
        public DateTime? DateTaken { get; }

        private int _lastid;
        public int LastId {
            get => _lastid;
            set {
                _lastid = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrLastId, value);
            }
        }

        private int _bestid;
        public int BestId {
            get => _bestid;
            set {
                _bestid = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestId, value);
            }
        }

        private float _bestdistance;
        public float BestDistance {
            get => _bestdistance;
            set {
                _bestdistance = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestDistance, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrLastView, value);
            }
        }

        public Img(
            int id,
            string name,
            string hash,
            DateTime? datetaken,
            int lastid,
            int bestid,
            float bestdistance,
            DateTime lastview
            )
        {
            Id = id;
            Name = name;
            Hash = hash;
            DateTaken = datetaken;
            _lastid = lastid;
            _bestid = bestid;
            _bestdistance = bestdistance;
            _lastview = lastview;
        }
    }
}