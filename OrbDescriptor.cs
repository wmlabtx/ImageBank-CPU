using System;

namespace ImageBank
{
    public class OrbDescriptor
    {
        public ulong[] Vector { get; }
        public int BitCount { get; }

        public OrbDescriptor(byte[] array, int offset)
        {
            Vector = new ulong[4];
            Buffer.BlockCopy(array, offset, Vector, 0, 32);
            BitCount =
                Intrinsic.PopCnt(Vector[0]) +
                Intrinsic.PopCnt(Vector[1]) +
                Intrinsic.PopCnt(Vector[2]) +
                Intrinsic.PopCnt(Vector[3]);
        }
    }
}
