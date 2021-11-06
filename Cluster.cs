namespace ImageBank
{
    public class Cluster
    {
        public int Id { get; }

        private byte[] _descriptor;
        public byte[] Descriptor {
            get => _descriptor;
            set {
                _descriptor = value;
                ImgMdf.SqlClustersUpdateProperty(Id, AppConsts.AttrDescriptor, value);
            }
        }

        private int _nextid;
        public int NextId {
            get => _nextid;
            set {
                _nextid = value;
                ImgMdf.SqlClustersUpdateProperty(Id, AppConsts.AttrNextId, value);
            }
        }

        private float _distance;
        public float Distance {
            get => _distance;
            set {
                _distance = value;
                ImgMdf.SqlClustersUpdateProperty(Id, AppConsts.AttrDistance, value);
            }
        }

        public Cluster(
            int id,
            byte[] descriptor,
            int nextid,
            float distance
            )
        {
            Id = id;
            _descriptor = descriptor;
            _nextid = nextid;
            _distance = distance;
        }
    }
}
