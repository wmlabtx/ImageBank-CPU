using ImageMagick;
using OpenCvSharp;
using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }

        public string Checksum { get; }

        public string Name => Helper.GetName(Id);

        public string Folder => Helper.GetFolder(Id);

        public string FileName => Helper.GetFileName(Name, Folder);

        private int _person;
        public int Person
        {
            get => _person;
            set
            {
                _person = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrPerson, value);
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
            get => _lastid;
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

        private readonly Mat _vector;

        public Mat Vector()
        {
            return _vector;
        }

        private int _format;
        public int Format
        {
            get => _format;
            set
            {
                _format = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrFormat, value);
            }
        }

        private readonly ulong[] _scalar;

        public ulong[] Scalar()
        {
            return _scalar;
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
            int person,
            DateTime lastview,
            int nextid,
            float distance,
            int lastid,
            Mat vector,
            int format,
            ulong[] scalar,
            int counter)
        {
            Id = id;
            Checksum = checksum;
            _person = person;
            _lastview = lastview;
            _nextid = nextid;
            _distance = distance;
            _lastid = lastid;
            _vector = vector;
            _format = format;
            _scalar = scalar;
            _counter = counter;
        }
    }
}