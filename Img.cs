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
        public float[][] Vector { get; }
        public int[] Node { get; }

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
                if (_sim < 0f || _sim > 100f) {
                    throw new ArgumentException(nameof(_sim));
                }

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

        private int _nodeid;
        public int NodeId {
            get => _nodeid;
            set {
                _nodeid = value;
                if (_nodeid < 0) {
                    throw new ArgumentException(nameof(NodeId));
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNode0, value);
            }
        }

        private int _nodemirrorid;
        public int NodeMirrorId {
            get => _nodemirrorid;
            set {
                _nodemirrorid = value;
                if (_nodemirrorid < 0) {
                    throw new ArgumentException(nameof(NodeMirrorId));
                }

                ImgMdf.SqlUpdateProperty(Name, AppConsts.AttrNode1, value);
            }
        }

        public void SetNode(int index, int id)
        {
            if (index != 0 && index != 1) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (id < 0) {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            Node[index] = id;
            ImgMdf.SqlUpdateProperty(Name, index == 0 ? AppConsts.AttrNode0 : AppConsts.AttrNode1, id);
        }

        public Img(
            string name,
            string hash,
            int width,
            int height,
            int size,
            DateTime? datetaken,
            string metadata,
            float[] vector0,
            int node0,
            float[] vector1,
            int node1,
            string nexthash,
            float sim,
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

            Vector = new float[2][];
            Vector[0] = vector0;
            Vector[1] = vector1;

            Node = new int[2];
            Node[0] = node0;
            Node[1] = node1;

            _nexthash = nexthash;
            _sim = sim;
            _lastchanged = lastchanged;
            _lastview = lastview;
            _lastcheck = lastcheck;
            _generation = generation;
        }
    }
}