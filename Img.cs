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

        private string _person;
        public string Person
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

        private float _sim;
        public float Sim
        {
            get => _sim;
            set
            {
                _sim = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrSim, value);
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

        private readonly ulong[] _vector;

        public ulong[] Vector()
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
            string person,
            DateTime lastview,
            int nextid,
            float sim,
            DateTime lastcheck,
            ulong[] vector,
            int format,
            int counter)
        {
            Id = id;
            Checksum = checksum;
            _person = person;
            _lastview = lastview;
            _nextid = nextid;
            _sim = sim;
            _lastcheck = lastcheck;
            _vector = vector;
            _format = format;
            _counter = counter;
        }
    }
}