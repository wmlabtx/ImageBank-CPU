using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public static class OrbHelper
    {
        private static readonly BFMatcher _bfMatcher = new BFMatcher(NormTypes.Hamming, true);

        public static bool ComputeOrbs(Bitmap thumb, out Mat vector, out ulong[] scalar)
        {
            Contract.Requires(thumb != null);
            vector = null;
            scalar = null;
            using (var orb = ORB.Create(AppConsts.MaxDescriptorsInImage)) {
                try {
                    using (var matcolor = BitmapConverter.ToMat(thumb)) {
                        if (matcolor.Width == 0 || matcolor.Height == 0) {
                            return false;
                        }

                        using (var mat = new Mat()) {
                            Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                            vector = new Mat();
                            orb.DetectAndCompute(mat, null, out _, vector);
                            if (vector.Rows == 0 || vector.Cols != 32) {
                                throw new Exception();
                            }

                            while (vector.Rows > AppConsts.MaxDescriptorsInImage) {
                                vector = vector.RowRange(0, AppConsts.MaxDescriptorsInImage);
                            }

                            using (var orb1000 = ORB.Create(1000))
                            using (var mat1000 = new Mat()) {
                                orb1000.DetectAndCompute(mat, null, out _, mat1000);
                                if (mat1000.Rows == 0 || mat1000.Cols != 32) {
                                    throw new Exception();
                                }

                                var counter = 0;
                                var bstat = new int[256];
                                mat1000.GetArray(out byte[] buffer);
                                var offset = 0;
                                var descriptor = new byte[32];
                                while (offset < buffer.Length) {
                                    Buffer.BlockCopy(buffer, offset, descriptor, 0, 32);
                                    var ba = new BitArray(descriptor);
                                    for (var i = 0; i < 256; i++) {
                                        if (ba[i]) {
                                            bstat[i]++;
                                        }
                                    }

                                    counter++;
                                    offset += 32;
                                }

                                var mid = counter / 2;
                                var result = new byte[32];
                                var ib = 0;
                                byte mask = 0x01;
                                for (var i = 0; i < 256; i++) {
                                    if (bstat[i] > mid) {
                                        result[ib] |= mask;
                                    }

                                    if (mask == 0x80) {
                                        ib++;
                                        mask = 0x01;
                                    }
                                    else {
                                        mask <<= 1;
                                    }
                                }

                                scalar = new ulong[4];
                                Buffer.BlockCopy(result, 0, scalar, 0, 32);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex);
                    vector = null;
                    scalar = null;
                    throw;
                }
            }

            return true;
        }

        public static float GetDistance(Mat x, Mat y)
        {
            var bfMatches = _bfMatcher.Match(x, y);
            var distances = bfMatches
                .OrderBy(e => e.Distance)
                .Select(e => e.Distance)
                .ToArray();

            return distances.Sum() / distances.Length;
        }

        public static int GetDistance(ulong[] x, ulong[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            if (x.Length != 4 || y.Length != 4) {
                return 256;
            }

            var distance = 0;
            for (var i = 0; i < 4; i++) {
                distance += Intrinsic.PopCnt(x[i] ^ y[i]);
            }

            return distance;
        }
    }
}
