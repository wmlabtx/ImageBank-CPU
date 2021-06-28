using System;

namespace ImageBank
{
    public class Img
    {
        public string FileName { get; }
        public string Hash { get; }
        public int Width { get; }
        public int Height { get; }
        public int Size { get; }
        public DateTime? DateTaken { get; }
        public string MetaData { get; }
        public byte[] KazeOne { get; }
        public byte[] KazeTwo { get; }

        private string _nexthash;
        public string NextHash {
            get => _nexthash;
            set {
                _nexthash = value;
                ImgMdf.SqlUpdateProperty(FileName, AppConsts.AttrNextHash, value);
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

                ImgMdf.SqlUpdateProperty(FileName, AppConsts.AttrKazeMatch, value);
            }
        }

        private DateTime _lastchanged;
        public DateTime LastChanged {
            get => _lastchanged;
            set {
                _lastchanged = value;
                ImgMdf.SqlUpdateProperty(FileName, AppConsts.AttrLastChanged, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlUpdateProperty(FileName, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck {
            get => _lastcheck;
            set {
                _lastcheck = value;
                ImgMdf.SqlUpdateProperty(FileName, AppConsts.AttrLastCheck, value);
            }
        }

        private int _counter;
        public int Counter {
            get => _counter;
            set {
                _counter = value;
                if (_counter < 0) {
                    throw new ArgumentException("_counter < 0");
                }

                ImgMdf.SqlUpdateProperty(FileName, AppConsts.AttrCounter, value);
            }
        }

        public Img(
            string filename,
            string hash,
            int width,
            int height,
            int size,
            DateTime? datetaken,
            string metadata,
            byte[] kazeone,
            byte[] kazetwo,
            string nexthash,
            int kazematch,
            DateTime lastchanged,
            DateTime lastview,
            DateTime lastcheck,
            int counter
            ) {

            FileName = filename;
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
            _counter = counter;
        }

        public Img(
            string filename,
            Img other
            )
        {
            FileName = filename;
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
            _counter = other.Counter;
        }
    }
}