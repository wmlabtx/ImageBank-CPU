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

        public Cluster(
            int id,
            byte[] descriptor
            )
        {
            Id = id;
            _descriptor = descriptor;
        }
    }
}
