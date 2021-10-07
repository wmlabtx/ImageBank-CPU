using System;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public class FHashEx
    {
        /*
        private static readonly ORB _orb = new ORB();
        private static readonly BFMatcher _bfmatcher = new BFMatcher(DistanceType.Hamming);
        private readonly Mat[] _descriptors;

        private static Mat GetDescriptors(int rows, int cols, byte[] bmatrix)
        {
            using (var mat = new Mat(rows, cols, DepthType.Cv8U, 1)) {
                mat.SetTo(bmatrix);

                using (var graymat = new Mat())
                using (var saliency = new StaticSaliencySpectralResidual())
                using (var saliencymap = new Mat())
                using (var saliencybinarymap = new Mat()) {
                    var f = 512.0 / Math.Max(mat.Width, mat.Height);
                    CvInvoke.Resize(mat, graymat, new Size(0, 0), f, f, Inter.Linear);
                    //graymat.Save("matrix.png");

                    saliency.Compute(graymat, saliencymap);

                    saliency.ComputeBinaryMap(saliencymap, saliencybinarymap);
                    using (var saliencybinarymapbitmap = saliencybinarymap * 255) {
                        //saliencybinarymapbitmap.Save("matrix_saliencybinarymap.png");
                        var keypoints = _orb.Detect(graymat, saliencybinarymapbitmap);
                        keypoints = keypoints.OrderByDescending(e => e.Size).Take(32).ToArray();
                        var descriptors = new Mat();
                        using (var vectorkeypoints = new VectorOfKeyPoint(keypoints)) {
                            _orb.Compute(graymat, vectorkeypoints, descriptors);
                            using (var finalkeypoints = new Mat()) {
                                Bgr keypointcolor = new Bgr(Color.White);
                                Features2DToolbox.DrawKeypoints(graymat, vectorkeypoints, finalkeypoints, keypointcolor, Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                                finalkeypoints.Save("matrix_finalkeypoints.png");
                            }
                        }

                        return descriptors;
                    }
                }
            }
        }

        public FHashEx(float[][] matrix)
        {
            _descriptors = new Mat[2];
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            var maxf = matrix[0][0];
            var minf = matrix[0][0];
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    if (matrix[i][j] > maxf) {
                        maxf = matrix[i][j];
                    }

                    if (matrix[i][j] < minf) {
                        minf = matrix[i][j];
                    }
                }
            }

            byte[] bmatrix = new byte[rows * cols];
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    var l = (byte)((matrix[i][j] - minf) * 255.0 / maxf);
                    bmatrix[i * cols + j] = l;
                }
            }

            _descriptors[0] = GetDescriptors(rows, cols, bmatrix);

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    var l = (byte)((matrix[i][j] - minf) * 255.0 / maxf);
                    bmatrix[i * cols + (cols - j - 1)] = l;
                }
            }

            _descriptors[1] = GetDescriptors(rows, cols, bmatrix);
        }

        private static int HammingDistance(Mat x, Mat y)
        {
            using (VectorOfDMatch vectordmatch = new VectorOfDMatch()) {
                _bfmatcher.Match(x, y, vectordmatch);
                var dmatch = vectordmatch.ToArray();
                var distance = (int)dmatch.Min(e => e.Distance);
                return distance;
            }
        }

        public int HammingDistance(FHashEx other)
        {
            var d0 = HammingDistance(_descriptors[0], other._descriptors[0]);
            var d1 = HammingDistance(_descriptors[0], other._descriptors[1]);
            var d = Math.Min(d0, d1);
            return d;
        }
        */
    }
}
