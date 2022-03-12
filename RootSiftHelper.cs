using OpenCvSharp;
using OpenCvSharp.Features2D;
using System;
using System.Linq;

namespace ImageBank
{
    public static class RootSiftHelper
    {
        private static readonly SIFT _sift = SIFT.Create(10000);
        private const int MAXDIM = 768;
        private const int MAXDESCRIPTORS = 250;

        public static RootSiftDescriptor[] Compute(float[][] matrix, bool draw = false)
        {
            RootSiftDescriptor[] result;

            int numRows = matrix.Length;
            int numCols = matrix[0].Length;

            float fmin = float.MaxValue;
            float fmax = float.MinValue;
            for (int i = 0; i < numRows; i++) {
                for (int j = 0; j < numCols; j++) {
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
                    for (int i = 0; i < numRows; i++) {
                        for (int j = 0; j < numCols; j++) {
                            byte val = (byte)(255f * (matrix[i][j] - fmin) / (fmax - fmin));
                            matraw.At<byte>(i, j) = val;
                        }
                    }

                    var f = MAXDIM / (float)Math.Min(matraw.Width, matraw.Height);
                    if (f < 1f) {
                        mat = new Mat();
                        Cv2.Resize(matraw, mat, new Size(0, 0), f, f, InterpolationFlags.Cubic);
                    }
                    else {
                        mat = matraw.Clone();
                    }
                }

                var keypoints = _sift.Detect(mat);
                keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(MAXDESCRIPTORS).ToArray();
                using (var matdescriptors = new Mat()) {
                    _sift.Compute(mat, ref keypoints, matdescriptors);
                    //matdescriptors.GetArray(out float[] f);
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
                    using (var matflip = new Mat()) {
                        Cv2.Flip(mat, matflip, FlipMode.Y);
                        keypoints = _sift.Detect(matflip);
                        keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(MAXDESCRIPTORS).ToArray();
                        using (var matdescriptorsflip = new Mat()) {
                            _sift.Compute(matflip, ref keypoints, matdescriptorsflip);
                            //matdescriptorsflip.GetArray(out float[] fflip);
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
                            matdescriptors.PushBack(matdescriptorsflip);
                            matdescriptors.GetArray(out float[] fmatdescriptors);
                            result = new RootSiftDescriptor[fmatdescriptors.Length / 128];
                            for (var i = 0; i < result.Length; i++) {
                                result[i] = new RootSiftDescriptor(fmatdescriptors, i * 128 * sizeof(float));
                            }
                        }
                    }

                }

            }
            finally {
                mat.Dispose();
            }

            return result;
        }

        public static float GetMinDistance(RootSiftDescriptor[] x, RootSiftDescriptor[] y)
        {
            var mindistance = float.MaxValue;
            for (var i = 0; i < x.Length; i++) {
                for (var j = 0; j < y.Length; j++) {
                    var distance = x[i].GetDistance(y[j]);
                    mindistance = Math.Min(mindistance, distance);
                }
            }

            return mindistance;
        }

        public static float GetDistance(ushort[] x, ushort[] y)
        {
            if (x == null || x.Length == 0 || y == null || y.Length == 0) {
                return 100f;
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

            var distance = 100f * (x.Length - m) / x.Length;
            return distance;
        }
    }
}
