using System.IO;

namespace ImageBank
{
    public class Axon
    {
        public ushort X { get; }
        public ushort Y { get; }
        public ushort Age { get; set; }

        public Axon(ushort x, ushort y)
        {
            X = x;
            Y = y;
            Age = 0;
        }

        public Axon(BinaryReader br)
        {
            X = br.ReadUInt16();
            Y = br.ReadUInt16();
            Age = br.ReadUInt16();
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(X);
            bw.Write(Y);
            bw.Write(Age);
        }
    }
}