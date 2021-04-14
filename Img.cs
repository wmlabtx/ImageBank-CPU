using OpenCvSharp;
using System;
using System.IO;

namespace ImageBank
{
    public class Img
    {
        public string Name { get; }

        private string _folder;
        public string Folder {
            get => _folder;
            set {
                if (string.IsNullOrEmpty(value)) {
                    throw new ArgumentException(@"string.IsNullOrEmpty(value)");
                }

                var oldfilename = FileName;
                _folder = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrFolder, value);
                var newfilename = FileName;
                var directory = Path.GetDirectoryName(newfilename);
                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                File.Move(oldfilename, newfilename);
            }
        }

        public string FileName => $"{AppConsts.PathHp}\\{Folder}\\{Name}{AppConsts.MzxExtension}";

        public string Hash { get; }

        public byte[] ColorDescriptors { get; }

        private float _colordistance;
        public float ColorDistance {
            get {
                return _colordistance;
            }
            set {
                _colordistance = value;
                if (_colordistance < 0f) {
                    throw new ArgumentException("_colordistance < 0f");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrColorDistance, value);
            }
        }

        public ulong[] PerceptiveDescriptors { get; }

        private int _perceptivedistance;
        public int PerceptiveDistance {
            get => _perceptivedistance;
            set {
                _perceptivedistance = value;
                if (_perceptivedistance < 0 || _perceptivedistance > AppConsts.MaxPerceptiveDistance) {
                    throw new ArgumentException("_perceptivedistance < 0 || _perceptivedistance > AppConsts.MaxPerceptiveDistance");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrPerceptiveDistance, value);
            }
        }

        public Mat OrbDescriptors { get; }

        public KeyPoint[] OrbKeyPoints { get; }

        private float _orbdistance;
        public float OrbDistance {
            get => _orbdistance;
            set {
                _orbdistance = value;
                if (_orbdistance < 0f || _orbdistance > AppConsts.MaxOrbDistance) {
                    throw new ArgumentException("_orbdistance < 0f || _orbdistance > AppConsts.MaxOrbDistance");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrOrbDistance, value);
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

        private string _nexthash;
        public string NextHash {
            get => _nexthash;
            set {
                _nexthash = value;
                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNextHash, value);
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

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrCounter, value);
            }
        }

        public int Width { get; }
        public int Height { get; }
        public int Size { get; }
        public int Id { get; }

        private int _lastid;
        public int LastId {
            get => _lastid;
            set {
                _lastid = value;
                if (_lastid < 0) {
                    throw new ArgumentException("_lastid < 0");
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrLastId, value);
            }
        }

        public Img(
            int id,
            string name,
            string folder,
            string hash,

            int width,
            int height,
            int size,

            byte[] colordescriptors,
            float colordistance,
            ulong[] perceptivedescriptors,
            int perceptivedistance,
            Mat orbdescriptors,
            KeyPoint[] orbkeypoints,
            float orbdistance,
           
            DateTime lastchanged,
            DateTime lastview,
            DateTime lastcheck,

            string nexthash,
            int lastid,
            int counter
            ) {
            if (id <= 0) {
                throw new ArgumentException("id <= 0");
            }

            Id = id;

            if (string.IsNullOrEmpty(name) || name.Length > 32) {
                throw new ArgumentException("string.IsNullOrEmpty(name) || name.Length > 32");
            }

            Name = name;

            if (string.IsNullOrEmpty(folder) || name.Length > 12) {
                throw new ArgumentException("string.IsNullOrEmpty(folder) || folder.Length > 12");
            }

            _folder = folder;

            if (string.IsNullOrEmpty(hash) || hash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(hash) || hash.Length != 32");
            }

            Hash = hash;

            if (width <= 0) {
                throw new ArgumentException("width <= 0");
            }

            Width = width;

            if (height <= 0) {
                throw new ArgumentException("height <= 0");
            }

            Height = height;

            if (size <= 0) {
                throw new ArgumentException("size <= 0");
            }

            Size = size;

            if (colordescriptors == null || colordescriptors.Length != 1024) {
                throw new ArgumentException("colordescriptors == null || colordescriptors.Length != 1024");
            }

            ColorDescriptors = colordescriptors;

            if (colordistance < 0f) {
                throw new ArgumentException("colordistance < 0f");
            }

            _colordistance = colordistance;

            if (perceptivedescriptors == null || perceptivedescriptors.Length != 4) {
                throw new ArgumentException("perceptivedescriptors == null || perceptivedescriptors.Length != 4");
            }

            PerceptiveDescriptors = perceptivedescriptors;

            if (perceptivedistance < 0 || perceptivedistance > AppConsts.MaxPerceptiveDistance) {
                throw new ArgumentException("perceptivedistance < 0 || perceptivedistance > AppConsts.MaxPerceptiveDistance");
            }

            _perceptivedistance = perceptivedistance;

            if (orbdescriptors == null) {
                throw new ArgumentException("orbdescriptors == null");
            }

            OrbDescriptors = orbdescriptors;

            if (orbkeypoints == null || orbkeypoints.Length == 0 || orbkeypoints.Length > 250) {
                throw new ArgumentException("orbkeypoints == null || orbkeypoints.Length == 0 || orbkeypoints.Length > 250");
            }

            OrbKeyPoints = orbkeypoints;

            if (orbdistance < 0f || orbdistance > AppConsts.MaxOrbDistance) {
                throw new ArgumentException("orbdistance < 0f || orbdistance > AppConsts.MaxOrbDistance");
            }

            _orbdistance = orbdistance;

            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;

            if (string.IsNullOrEmpty(nexthash) || nexthash.Length != 32) {
                throw new ArgumentException("string.IsNullOrEmpty(nexthash) || nexthash.Length != 32");
            }

            _nexthash = nexthash;

            if (counter < 0) {
                throw new ArgumentException("counter < 0");
            }

            _counter = counter;
            

            if (lastid < 0) {
                throw new ArgumentException("lastid < 0");
            }

            _lastid = lastid;
        }
    }
}