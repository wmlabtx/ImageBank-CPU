using System;
using System.IO;

namespace ImageBank
{
    public class RootSiftDescriptor
    {
        private readonly float[] _vector;

        public RootSiftDescriptor(float[] array, int offset)
        {
            _vector = new float[128];
            Buffer.BlockCopy(array, offset, _vector, 0, 128 * sizeof(float));
        }

        public RootSiftDescriptor(BinaryReader br)
        {
            var array = br.ReadBytes(128 * sizeof(float));
            _vector = new float[128];
            Buffer.BlockCopy(array, 0, _vector, 0, 128 * sizeof(float));
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

        public void MoveToward(RootSiftDescriptor destination, float k)
        {
            for (var i = 0; i < _vector.Length; i++) {
                _vector[i] = (_vector[i] * (1 - k)) + (destination._vector[i] * k);
            }
        }

        public RootSiftDescriptor Average(RootSiftDescriptor other)
        {
            var vector = new float[_vector.Length];
            for (var i = 0; i < _vector.Length; i++) {
                vector[i] = (_vector[i] + other._vector[i]) / 2f;
            }

            var result = new RootSiftDescriptor(vector, 0);
            return result;
        }

        public void Save(BinaryWriter bw)
        {
            var array = new byte[128 * sizeof(float)];
            Buffer.BlockCopy(_vector, 0, array, 0, 128 * sizeof(float));
            bw.Write(array, 0, array.Length);
        }
    }
}
