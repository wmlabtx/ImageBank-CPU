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

        private string _nexthash;
        public string NextHash {
            get => _nexthash;
            set {
                _nexthash = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrNextHash, value);
            }
        }

        private float _sim;
        public float Sim {
            get => _sim;
            set {
                _sim = value;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrSim, value);
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
                if (_generation < 0) {
                    throw new ArgumentException("_generation < 0");
                }

                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrGeneration, value);
            }
        }

        private readonly short[][] _ki;
        public short[][] Ki => _ki;

        public void SetKi(short[][] ki)
        {
            _ki[0] = ki[0];
            ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrKi, Helper.ArrayFrom16(ki[0]));
            _ki[1] = ki[1];
            ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrKiMirror, Helper.ArrayFrom16(ki[1]));
        }

        public Img(
            string name,
            string hash,
            int width,
            int height,
            int size,
            DateTime? datetaken,
            string metadata,
            short[][] ki,
            string nexthash,
            float sim,
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

            _nexthash = nexthash;
            _sim = sim;
            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _generation = generation;

            _ki = new short[2][];
            _ki[0] = ki[0];
            _ki[1] = ki[1];
        }
    }
}