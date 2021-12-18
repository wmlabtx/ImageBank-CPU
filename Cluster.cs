namespace ImageBank
{
    public class Cluster
    {
        public short Id { get; }

        private byte[] _descriptor;
        public byte[] Descriptor {
            get => _descriptor;
            set {
                _descriptor = value;
                ImgMdf.SqlClustersUpdateProperty(Id, AppConsts.AttrDescriptor, value);
            }
        }

        public Cluster(
            short id,
            byte[] descriptor
            )
        {
            Id = id;
            _descriptor = descriptor;
        }
    }
}
