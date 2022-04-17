using System;
using System.Security.Cryptography;

namespace ImageBank
{
    public class RootSiftDescriptor
    {
        private readonly float[] _vector;
        public ulong Fingerprint { get; }

        public RootSiftDescriptor(float[] array, int offset)
        {
            _vector = new float[128];
            Buffer.BlockCopy(array, offset, _vector, 0, 128 * sizeof(float));

            var buffer = new byte[128];
            for (var i = 0; i < _vector.Length; i++) {
                buffer[i] = (byte)Math.Round(_vector[i] * 3f);
            }

            using (var md5 = MD5.Create()) {
                var hashmd5 = md5.ComputeHash(buffer);
                Fingerprint = BitConverter.ToUInt64(hashmd5, 0);
            }
        }

        public float GetDistance(RootSiftDescriptor other)
        {
            var distance = 0f;
            for (var i = 0; i < _vector.Length; i++) {
                distance += (_vector[i] - other._vector[i]) * (_vector[i] - other._vector[i]);
            }

            distance = (float)Math.Sqrt(distance);
            return distance;
        }
    }
}
