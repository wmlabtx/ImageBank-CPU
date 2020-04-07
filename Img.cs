using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }

        private string _checksum;
        public string Checksum
        {
            get => _checksum;
            set
            {
                _checksum = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrChecksum, value);
            }
        }

        public string Name => Helper.GetName(Id);

        public string Folder => Helper.GetFolder(Id);

        public string FileName => Helper.GetFileName(Name, Folder);

        private int _nextid;
        public int NextId
        {
            get => _nextid;
            set
            {
                _nextid = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrNextId, value);
            }
        }

        private float _distance;
        public float Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrDistance, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck
        {
            get => _lastcheck;
            set
            {
                _lastcheck = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastCheck, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView
        {
            get => _lastview;
            set
            {
                _lastview = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastadded;
        public DateTime LastAdded
        {
            get => _lastadded;
            set
            {
                _lastadded = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastAdded, value);
            }
        }

        private Scd _vector;

        public Scd Vector
        {
            get => _vector;
            set
            {
                if (value != null) {
                    _vector = value;
                    var buffer = value.GetBuffer();
                    ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrVector, buffer);
                }
            }
        }

        private MagicFormat _format;
        public MagicFormat Format
        {
            get => _format;
            set
            {
                _format = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrFormat, (int)value);
            }
        }

        private int _counter;
        public int Counter
        {
            get => _counter;
            set
            {
                _counter = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrCounter, value);
            }
        }

        public Img(
            int id,
            string checksum,
            DateTime lastview,
            int nextid,
            float distance,
            DateTime lastcheck,
            DateTime lastadded,
            Scd vector,
            MagicFormat format,
            int counter)
        {
            Id = id;
            _checksum = checksum;
            _lastview = lastview;
            _nextid = nextid;
            _distance = distance;
            _lastcheck = lastcheck;
            _lastadded = lastadded;
            _vector = vector;
            _format = format;
            _counter = counter;
        }
    }
}