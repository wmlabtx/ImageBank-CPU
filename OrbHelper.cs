using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Drawing;


namespace ImageBank
{
    public static class OrbHelper
    {
        public static bool Compute(Bitmap bitmap, out byte[] vector)
        {
            Contract.Requires(bitmap != null);
            vector = null;
            using (var orb = ORB.Create(500)) {
                using (var thumb = Helper.ResizeBitmap(bitmap, 512, 512))
                using (var matcolor = BitmapConverter.ToMat(thumb)) {
                    if (matcolor.Width == 0 || matcolor.Height == 0) {
                        return false;
                    }

                    using (var mat = new Mat()) {
                        Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                        using (var descriptors = new Mat()) {
                            orb.DetectAndCompute(mat, null, out _, descriptors);
                            if (descriptors.Rows == 0 || descriptors.Cols != 32) {
                                return false;
                            }

                            descriptors.GetArray(out byte[] buffer);
                            var n = Math.Min(500, buffer.Length / 32);
                            var hist = new ulong[256];
                            ulong hmax = 0;
                            var offset = 0;
                            var descriptor = new byte[32];
                            for (var i = 0; i < n; i++) {
                                Buffer.BlockCopy(buffer, offset, descriptor, 0, 32);
                                offset += 32;
                                var ba = new BitArray(descriptor);
                                for (var j = 0; j < 256; j++) {
                                    if (ba[j]) {
                                        hist[j]++;
                                        if (hist[j] > hmax) {
                                            hmax = hist[j];
                                        }
                                    }
                                }
                            }

                            vector = new byte[256];
                            for (var j = 0; j < 256; j++) {
                                vector[j] = (byte)((hist[j] * 255) / hmax);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public static float CosineDistance(byte[] x, byte[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Requires(x.Length > 0);
            Contract.Requires(y.Length > 0);
            Contract.Requires(x.Length == y.Length);
            var m = 0f;
            var a = 0f;
            var b = 0f;
            for (var i = 0; i < x.Length; i++) {
                if (Math.Abs(x[i]) > 0.0001) {
                    m += (float)x[i] * y[i];
                    a += (float)x[i] * x[i];
                    b += (float)y[i] * y[i];
                }
            }

            var distance = (float)(100f * Math.Acos(m / (Math.Sqrt(a) * Math.Sqrt(b))) / Math.PI);
            return distance;
        }
    }
}
