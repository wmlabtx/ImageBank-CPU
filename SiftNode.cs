namespace ImageBank
{
    public class SiftNode
    {
        public int Id { get; set; }
        public byte[] Core { get; set; }
        public float Sum { get; set; }
        public float Max { get; set; }
        public int Cnt { get; set; }
        public float Avg { get; set; }
        public int ChildId { get; set; }
    }
}
