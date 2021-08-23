namespace ImageBank
{
    public class Descriptor
    {
        public long DescriptorId { get; }
        public ulong[] Vector { get; }

        private int _nodeid;
        public int NodeId {
            get => _nodeid;
            set {
                _nodeid = value;
                ImgMdf.SqlDescriptorsUpdateProperty(DescriptorId, AppConsts.AttrNodeId, value);
            }
        }

        public Descriptor(
            long descriptorid,
            ulong[] vector,
            int nodeid)
        {
            DescriptorId = descriptorid;
            Vector = vector;
            _nodeid = nodeid;
        }
    }
}
