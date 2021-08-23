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

        private readonly int[][] _vector;
        public int[][] Vector => _vector;

        public void SetVector(int index, int[] vector)
        {
            if (index == 0) {
                _vector[0] = vector;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrKi, Helper.ArrayFrom32(vector));
            }
            else {
                _vector[1] = vector;
                ImgMdf.SqlImagesUpdateProperty(Name, AppConsts.AttrKiMirror, Helper.ArrayFrom32(vector));
            }
        }

        public Img(
            string name,
            string hash,
            int width,
            int height,
            int size,
            DateTime? datetaken,
            string metadata,
            int[] ki,
            int[] kimirror,
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

            _vector = new int[2][];
            _vector[0] = ki;
            _vector[1] = kimirror;
        }
    }
}