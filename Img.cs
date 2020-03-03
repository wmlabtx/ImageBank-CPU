using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrName, value);
                _person = Helper.GetPerson(_name);
            }
        }

        private string _person;
        public string Person { get => _person; }

        public string Checksum { get; }

        private int _generation;
        public int Generation
        {
            get => _generation;
            set
            {
                _generation = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrGeneration, value);
            }
        }

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

        private int _lastid;
        public int LastId
        {
            get
            {
                return _lastid;
            }
            set
            {
                _lastid = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastId, value);
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

        private DateTime _lastchange;
        public DateTime LastChange
        {
            get => _lastchange;
            set
            {
                _lastchange = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastChange, value);
            }
        }

        private DateTime _lastfind;
        public DateTime LastFind
        {
            get => _lastfind;
            set
            {
                _lastfind = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastFind, value);
            }
        }

        private readonly float[] _vector;

        public float[] Vector()
        {
            return _vector;
        }

        public string File
        {
            get
            {
                var filename = Helper.GetFileName(Name);
                return filename;
            }
        }

        public Img(
            int id,
            string name,
            string checksum,
            int generation,
            DateTime lastview,
            int nextid,
            float distance,
            int lastid,
            DateTime lastchange,
            DateTime lastfind,
            float[] vector)
        {
            Id = id;
            _name = name;
            _person = Helper.GetPerson(_name);
            Checksum = checksum;
            _generation = generation;
            _lastview = lastview;
            _nextid = nextid;
            _distance = distance;
            _lastid = lastid;
            _lastchange = lastchange;
            _lastfind = lastfind;
            _vector = vector;
        }
    }
}