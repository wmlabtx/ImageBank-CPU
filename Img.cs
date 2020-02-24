using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrPath, value);
            }
        }

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

        private readonly float[] _vector;

        public float[] Vector()
        {
            return _vector;
        }

        public string Directory
        {
            get
            {
                var directory = $"{AppConsts.PathCollection}{Path}\\";
                return directory;
            }
        }

        public string File
        {
            get
            {
                var filename = Helper.GetFileName(Name, Path);
                return filename;
            }
        }

        public Img(
            int id,
            string name,
            string path,
            string checksum,
            int generation,
            DateTime lastview,
            int nextid,
            float distance,
            int lastid,
            DateTime lastchange,
            float[] vector)
        {
            Id = id;
            Name = name;
            _path = path;
            Checksum = checksum;
            _generation = generation;
            _lastview = lastview;
            _nextid = nextid;
            _distance = distance;
            _lastid = lastid;
            _lastchange = lastchange;
            _vector = vector;
        }
    }
}