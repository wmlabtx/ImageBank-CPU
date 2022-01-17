using OpenCvSharp;
using OpenCvSharp.Features2D;
using System;
using System.Linq;

namespace ImageBank
{
    public static class SiftHelper
    {
        private static readonly SIFT _sift = SIFT.Create(10000);

        public static Mat GetDescriptors(float[][] matrix)
        {
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

            Mat mat = new Mat();
            using (var matraw = new Mat(numRows, numCols, MatType.CV_8U)) {
                for (int i = 0; i < numRows; i++) {
                    for (int j = 0; j < numCols; j++) {
                        byte val = (byte)(255f * (matrix[i][j] - fmin) / (fmax - fmin));
                        matraw.At<byte>(i, j) = val;
                    }
                }
                
                var f = 512f / Math.Min(matraw.Width, matraw.Height);
                if (f < 1f) {
                    Cv2.Resize(matraw, mat, new Size(0, 0), f, f, InterpolationFlags.Cubic);
                }
                else {
                    mat = matraw.Clone();
                }
            }

            var keypoints = _sift.Detect(mat);
            keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
            var matdescriptors = new Mat();
            _sift.Compute(mat, ref keypoints, matdescriptors);
            /*
            using (var matkeypoints = new Mat()) {
                Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                matkeypoints.SaveImage("matkeypoints.png");
            }
            */

            using (var matflip = new Mat()) {
                Cv2.Flip(mat, matflip, FlipMode.Y);
                keypoints = _sift.Detect(matflip);
                keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
                using (var matdescriptorsflip = new Mat()) {
                    _sift.Compute(matflip, ref keypoints, matdescriptorsflip);
                    matdescriptors.PushBack(matdescriptorsflip);
                }
            }

            mat.Dispose();
            return matdescriptors;
        }
    }
}
