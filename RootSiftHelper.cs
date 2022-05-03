using OpenCvSharp;
using OpenCvSharp.Features2D;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public static class RootSiftHelper
    {
        private static readonly SIFT _sift = SIFT.Create(10000);
        private const int Maxdim = 768;
        private const int Maxdescriptors = 500;

        public static void Compute(float[][] matrix, out RootSiftDescriptor[][] descriptors, bool draw = false)
        {
            descriptors = new RootSiftDescriptor[2][];

            var numRows = matrix.Length;
            var numCols = matrix[0].Length;

            var fmin = float.MaxValue;
            var fmax = float.MinValue;
            for (var i = 0; i < numRows; i++) {
                for (var j = 0; j < numCols; j++) {
                    if (matrix[i][j] < fmin) {
                        fmin = matrix[i][j];
                    }

                    if (matrix[i][j] > fmax) {
                        fmax = matrix[i][j];
                    }
                }
            }

            Mat mat = null;
            try {
                using (var matraw = new Mat(numRows, numCols, MatType.CV_8U)) {
                    for (var i = 0; i < numRows; i++) {
                        for (var j = 0; j < numCols; j++) {
                            var val = (byte)(255f * (matrix[i][j] - fmin) / (fmax - fmin));
                            matraw.At<byte>(i, j) = val;
                        }
                    }

                    var f = Maxdim / (float)Math.Min(matraw.Width, matraw.Height);
                    if (f < 1f) {
                        mat = new Mat();
                        Cv2.Resize(matraw, mat, new Size(0, 0), f, f, InterpolationFlags.Cubic);
                    }
                    else {
                        mat = matraw.Clone();
                    }
                }

                var keypoints = _sift.Detect(mat);
                keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(Maxdescriptors).ToArray();
                using (var matdescriptors = new Mat()) {
                    _sift.Compute(mat, ref keypoints, matdescriptors);
                    if (draw) {
                        using (var matkeypoints = new Mat()) {
                            Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                            matkeypoints.SaveImage("mat-n.png");
                        }
                    }

                    for (var i = 0; i < matdescriptors.Rows; i++) {
                        Cv2.Normalize(matdescriptors.Row(i), matdescriptors.Row(i), 1.0, 0.0, NormTypes.L1);
                    }

                    Cv2.Sqrt(matdescriptors, matdescriptors);
                    matdescriptors.GetArray(out float[] fmatdescriptors);

                    descriptors[0] = new RootSiftDescriptor[fmatdescriptors.Length / 128];
                    for (var i = 0; i < descriptors[0].Length; i++) {
                        descriptors[0][i] = new RootSiftDescriptor(fmatdescriptors, i * 128 * sizeof(float));
                    }

                    using (var matflip = new Mat()) {
                        Cv2.Flip(mat, matflip, FlipMode.Y);
                        keypoints = _sift.Detect(matflip);
                        keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(Maxdescriptors).ToArray();
                        using (var matdescriptorsflip = new Mat()) {
                            _sift.Compute(matflip, ref keypoints, matdescriptorsflip);
                            if (draw) {
                                using (var matkeypoints = new Mat()) {
                                    Cv2.DrawKeypoints(matflip, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                    matkeypoints.SaveImage("mat-f.png");
                                }
                            }

                            for (var i = 0; i < matdescriptorsflip.Rows; i++) {
                                Cv2.Normalize(matdescriptorsflip.Row(i), matdescriptorsflip.Row(i), 1.0, 0.0, NormTypes.L1);
                            }

                            Cv2.Sqrt(matdescriptorsflip, matdescriptorsflip);
                            matdescriptorsflip.GetArray(out float[] fmatdescriptorsflip);

                            descriptors[1] = new RootSiftDescriptor[fmatdescriptorsflip.Length / 128];
                            for (var i = 0; i < descriptors[1].Length; i++) {
                                descriptors[1][i] = new RootSiftDescriptor(fmatdescriptorsflip, i * 128 * sizeof(float));
                            }
                        }
                    }

                }

            }
            finally {
                mat.Dispose();
            }
        }

        private static float GetDistance(IReadOnlyList<RootSiftDescriptor> x, IReadOnlyList<RootSiftDescriptor> y)
        {
            var mindistance = x[0].GetDistance(y[0]);
            mindistance = (from tx in x from ty in y select tx.GetDistance(ty)).Prepend(mindistance).Min();
            return mindistance;
        }

        public static float GetDistance(RootSiftDescriptor[][] x, RootSiftDescriptor[][] y)
        {
            var d0 = GetDistance(x[0], y[0]);
            var d1 = GetDistance(x[0], y[1]);
            var distance = Math.Min(d0, d1);
            return distance;
        }

        public static int GetMatch(ulong[][] x, ulong[][] y)
        {
            var d0 = GetMatch(x[0], y[0]);
            var d1 = GetMatch(x[0], y[1]);
            var distance = Math.Max(d0, d1);
            return distance;
        }

        private static int GetMatch(ulong[] x, ulong[] y)
        {
            if (x == null || x.Length == 0 || y == null || y.Length == 0) {
                return 0;
            }

            var m = 0;
            var i = 0;
            var j = 0;
            while (i < x.Length && j < y.Length) {
                if (x[i] == y[j]) {
                    m++;
                    i++;
                    j++;
                }
                else {
                    if (x[i] < y[j]) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            return m;
        }

        public static ulong[][] GetFingerprints(RootSiftDescriptor[][] descriptors)
        {
            var result = new ulong[2][];
            for (var i = 0; i < 2; i++) {
                result[i] = new ulong[descriptors[i].Length];
                for (var j = 0; j < descriptors[i].Length; j++) {
                    result[i][j] = descriptors[i][j].Fingerprint;
                }

                Array.Sort(result[i]);
            }

            return result;
        }
    }
}
