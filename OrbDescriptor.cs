using System;
using System.Diagnostics.Contracts;

namespace ImageBank
{
    public class OrbDescriptor
    {
        private readonly ulong[] _vector;

        public OrbDescriptor(byte[] buffer, int offset)
        {
            Contract.Requires(buffer != null);

            _vector = new ulong[4];
            Buffer.BlockCopy(buffer, offset, _vector, 0, 32);
        }

        public OrbDescriptor(ushort[] buffer, int offset)
        {
            Contract.Requires(buffer != null);

            _vector = new ulong[4];
            Buffer.BlockCopy(buffer, offset, _vector, 0, 32);
        }

        public int Distance(OrbDescriptor other)
        {
            Contract.Requires(other != null);

            var distance = 0;
            for (var i = 0; i < 4; i++) {
                distance += Intrinsic.PopCnt(_vector[i] ^ other._vector[i]);
            }

            return distance;
        }
    }
}
