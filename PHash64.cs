using System;

namespace ImageBank
{
    public class PHash64
    {
        private readonly ulong[] v64;

        public PHash64()
        {
            v64 = new ulong[4];
            for (int i = 0; i < 4; i++) {
                v64[i] = 0;
            }
        }

        public PHash64(byte[] buffer, int offset)
        {
            v64 = new ulong[4];
            Buffer.BlockCopy(buffer, offset, v64, 0, 32);
        }

        public void SetBit(int k)
        {
            v64[(k & 0xFF) >> 6] |= 1UL << (k & 0x3F);
        }

        public int HammingDistance(PHash64 other)
        {
            int n = 0;
            for (int i = 0; i < 4; i++) {
                n += PopCount(v64[i] ^ other.v64[i]);
            }

            return n;
        }

        private static int PopCount(ulong x)
        {
            x = x - ((x & 0xAAAAAAAAAAAAAAAA) >> 1);
            x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333);
            x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0F;
            x = (x * 0x0101010101010101) >> 56;
            return (int)x;
        }

        public byte[] ToArray()
        {
            var buffer = new byte[32];
            Buffer.BlockCopy(v64, 0, buffer, 0, 32);
            return buffer;
        }
    }
}