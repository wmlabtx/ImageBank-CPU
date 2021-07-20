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
        public KazePoint[] KazeOne { get; }
        public KazePoint[] KazeTwo { get; }

        private string _nexthash;
        public string NextHash {
            get => _nexthash;
            set {
                _nexthash = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNextHash, value);
            }
        }

        private int _kazematch;
        public int KazeMatch {
            get => _kazematch;
            set {
                _kazematch = value;
                if (_kazematch < 0 || _kazematch > AppConsts.MaxDescriptors) {
                    throw new ArgumentException("_kazematch < 0 || _kazematch > AppConsts.MaxDescriptors");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrKazeMatch, value);
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

        public float Sim => (float)KazeMatch / KazeOne.Length;

        public Img(
            string name,
            string hash,
            int width,
            int height,
            int size,
            DateTime? datetaken,
            string metadata,
            KazePoint[] kazeone,
            KazePoint[] kazetwo,
            string nexthash,
            int kazematch,
            DateTime lastchanged,
            DateTime lastview,
            DateTime lastcheck,
            int generation
            ) {

            Name = name;
            Hash = hash;
            Width = width;
            Height = height;
            Size = size;
            DateTaken = datetaken;
            MetaData = metadata;
            KazeOne = kazeone;
            KazeTwo = kazetwo;

            _nexthash = nexthash;
            _kazematch = kazematch;
            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _generation = generation;
        }

        public Img(
            string name,
            Img other
            )
        {
            Name = name;
            Hash = other.Hash;
            Width = other.Width;
            Height = other.Height;
            Size = other.Size;
            KazeOne = other.KazeOne;
            KazeTwo = other.KazeTwo;

            _nexthash = other.NextHash;
            _kazematch = other.KazeMatch;
            _lastchanged = other.LastChanged;
            _lastview = other.LastView;
            _lastcheck = other.LastCheck;
            _generation = other.Generation;
        }
    }
}