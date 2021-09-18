using System;

namespace ImageBank
{
    public class Hash256
    {
        public static readonly int HASH256_NUM_SLOTS = 4;
        private static readonly Random RNG = new Random(); // for the fuzz() method

        public ulong[] v64;

        public Hash256()
        {
            v64 = new ulong[HASH256_NUM_SLOTS];
            for (int i = 0; i < HASH256_NUM_SLOTS; i++) {
                v64[i] = 0;
            }
        }

        public Hash256(byte[] buffer, int offset)
        {
            v64 = new ulong[HASH256_NUM_SLOTS];
            Buffer.BlockCopy(buffer, offset, v64, 0, 32);
        }

        public void SetBit(int k)
        {
            v64[(k & 0xFF) >> 6] |= 1UL << (k & 0x3F);
        }

        public int HammingDistance(Hash256 other)
        {
            int n = 0;
            for (int i = 0; i < HASH256_NUM_SLOTS; i++) {
                n += PopCount(v64[i] ^ other.v64[i]);
            }

            return n;
        }

        public static int PopCount(ulong x)
        {
            x = x - ((x & 0xAAAAAAAAAAAAAAAA) >> 1);
            x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333);
            x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0F;
            x = (x * 0x0101010101010101) >> 56;
            return (int)x;
        }
    }
}
