using System;

namespace ImageBank
{
    public class Img
    {
        public string Id { get; }

        private string _folder;
        public string Folder
        {
            get => _folder;
            set
            {
                _folder = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrFolder, value);
            }
        }
        public string FileName => Helper.GetFileName(Id, Folder);

        private string _nextid;
        public string NextId
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

        private DateTime _lastmodified;
        public DateTime LastModified
        {
            get => _lastmodified;
            set
            {
                _lastmodified = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastModified, value);
            }
        }

        private byte[] _vector;

        public void SetVector(byte[] vector)
        {
            _vector = vector;
        }

        public byte[] GetVector()
        {
            return _vector;
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
            string id,
            string folder,
            DateTime lastview,
            string nextid,
            float distance,
            DateTime lastcheck,
            DateTime lastmodified,
            byte[] vector,
            int counter)
        {
            Id = id;
            _folder = folder;
            _lastview = lastview;
            _nextid = nextid;
            _distance = distance;
            _lastcheck = lastcheck;
            _lastmodified = lastmodified;
            _vector = vector;
            _counter = counter;
        }
    }
}