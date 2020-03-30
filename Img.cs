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

        private readonly ulong[] _vector;

        public ulong[] Vector()
        {
            return _vector;
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
            float sim,
            int lastid,
            ulong[] vector,
            MagicFormat format,
            int counter)
        {
            Id = id;
            Checksum = checksum;
            _lastview = lastview;
            _nextid = nextid;
            _sim = sim;
            _lastid = lastid;
            _vector = vector;
            _format = format;
            _counter = counter;
        }
    }
}