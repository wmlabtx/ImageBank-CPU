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
        public FeaturePoint[] Fp { get; }
        public FeaturePoint[] FpMirror { get; }

        private string _nexthash;
        public string NextHash {
            get => _nexthash;
            set {
                _nexthash = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNextHash, value);
            }
        }

        private float _sim;
        public float Sim {
            get => _sim;
            set {
                _sim = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrSim, value);
            }
        }

        private DateTime _lastchanged;
        public DateTime LastChanged {
            get => _lastchanged;
            set {
                _lastchanged = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastChanged, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck {
            get => _lastcheck;
            set {
                _lastcheck = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastCheck, value);
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

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrGeneration, value);
            }
        }

        public short[] Rv;
        public short[] RvMirror;

        public Img(
            string name,
            string hash,
            int width,
            int height,
            int size,
            DateTime? datetaken,
            string metadata,
            FeaturePoint[] fp,
            FeaturePoint[] fpmirror,
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

            Fp = fp;
            FpMirror = fpmirror;

            _nexthash = nexthash;
            _sim = sim;
            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _generation = generation;

            Rv = ImageHelper.GetRandomVector(Fp);
            RvMirror = ImageHelper.GetRandomVector(FpMirror);
        }
    }
}