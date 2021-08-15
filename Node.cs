using System;

namespace ImageBank
{
    public class Node
    {
        public int NodeId { get; }
        public int[] ChildId { get; }

        private float _radius;
        public float Radius {
            get => _radius;
            set {
                _radius = value;
                if (_radius < 0f) {
                    throw new ArgumentOutOfRangeException(nameof(Radius));
                }

                ImgMdf.SqlUpdateProperty(NodeId, AppConsts.AttrRadius, _radius);
            }
        }

        private int _previd;
        public int PrevId {
            get => _previd;
            set {
                _previd = value;
                if (_previd < 0) {
                    throw new ArgumentOutOfRangeException(nameof(PrevId));
                }

                ImgMdf.SqlUpdateProperty(NodeId, AppConsts.AttrPrevId, _previd);
            }
        }

        private float[] _core;
        public float[] Core {
            get => _core;
            set {
                _core = value;
                if (_core == null) {
                    throw new ArgumentOutOfRangeException(nameof(Core));
                }

                ImgMdf.SqlUpdateProperty(NodeId, AppConsts.AttrCore, Helper.FloatToBuffer(_core));
            }
        }

        public void SetChildId(int index, int id)
        {
            if (index != 0 && index != 1) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (id < 0) {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            ChildId[index] = id;
            ImgMdf.SqlUpdateProperty(NodeId, index == 0 ? AppConsts.AttrChildId0 : AppConsts.AttrChildId1, id);
        }

        public Node(
            int nodeid,
            int previd,
            float[] core,
            float radius,
            int childid0,
            int childid1
            )
        {
            NodeId = nodeid;
            
            _previd = previd;
            _core = core;
            _radius = radius;

            ChildId = new int[2];
            ChildId[0] = childid0;
            ChildId[1] = childid1;
        }

        public bool IsLeaf()
        {
            return ChildId[0] == 0 && ChildId[1] == 0;
        }
    }
}
