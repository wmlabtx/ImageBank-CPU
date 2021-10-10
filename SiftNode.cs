namespace ImageBank
{
    public class SiftNode
    {
        public int Id { get; }

        private byte[] _core;
        public byte[] Core {
            get => _core;
            set {
                _core = value;
                ImgMdf.SqlNodesUpdateProperty(Id, AppConsts.AttrCore, _core);
            }
        }

        private float _sumdst;
        public float SumDst {
            get => _sumdst;
            set {
                _sumdst = value;
                ImgMdf.SqlNodesUpdateProperty(Id, AppConsts.AttrSumDst, _sumdst);
            }
        }

        private float _maxdst;
        public float MaxDst {
            get => _maxdst;
            set {
                _maxdst = value;
                ImgMdf.SqlNodesUpdateProperty(Id, AppConsts.AttrMaxDst, _maxdst);
            }
        }

        private int _cnt;
        public int Cnt {
            get => _cnt;
            set {
                _cnt = value;
                ImgMdf.SqlNodesUpdateProperty(Id, AppConsts.AttrCnt, _cnt);
            }
        }

        private float _avgdst;
        public float AvgDst {
            get => _avgdst;
            set {
                _avgdst = value;
                ImgMdf.SqlNodesUpdateProperty(Id, AppConsts.AttrAvgDst, _avgdst);
            }
        }

        private int _childid;
        public int ChildId {
            get => _childid;
            set {
                _childid = value;
                ImgMdf.SqlNodesUpdateProperty(Id, AppConsts.AttrChildId, _childid);
            }
        }

        public SiftNode(int id, byte[] core, float sumdst, float maxdst, int cnt, float avgdst, int childid)
        {
            Id = id;
            _core = core;
            _sumdst = sumdst;
            _maxdst = maxdst;
            _cnt = cnt;
            _avgdst = avgdst;
            _childid = childid;
        }
    }
}
