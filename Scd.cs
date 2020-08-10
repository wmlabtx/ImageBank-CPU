using System;
using System.Diagnostics.Contracts;
using System.Drawing;

namespace ImageBank
{
    public class Scd
    {
        private int[] H;

        public Scd(int[] h)
        {
            H = h;
        }

        public Scd (Bitmap bitmap)
        {
            Contract.Requires(bitmap != null);

            var descriptor = new SCDDescriptor();
            descriptor.Apply(bitmap, 256, 0);
            Set(descriptor.haarTransformedHistogram);
        }

        public Scd(byte[] buffer)
        {
            Contract.Requires(buffer != null);
            H = new int[256];
            Buffer.BlockCopy(buffer, 0, H, 0, 256 * sizeof(int));
        }

        private void Set(double[] descriptor)
        {
            Contract.Requires(descriptor != null);
            H = new int[256];
            for (var i = 0; i < 256; i++) {
                H[i] = (int)descriptor[i];
            }
        }

        public Scd(double[] descriptor)
        {
            Set(descriptor);
        }

        public byte[] GetBuffer()
        {
            var buffer = new byte[256 * sizeof(int)];
            Buffer.BlockCopy(H, 0, buffer, 0, 256 * sizeof(int));
            return buffer;
        }

        public bool IsEmpty()
        {
            for (var i = 0; i < 256; i++) {
                if (H[i] != 0) {
                    return false;
                }
            }

            return true;
        }

        public int IsBw()
        {
            var count = 0;
            for (var i = 0; i < 256; i++) {
                if (H[i] == 0) {
                    count++;
                }
            }

            return count;
        }

        public float GetDistance(Scd other)
        {
            Contract.Requires(other != null);

            var distance = 0f;
            for (int l = 0; l < 256; l++) {
                distance += (H[l] - other.H[l]) * (H[l] - other.H[l]);
            }

            return distance / 256f;
        }
    }
}