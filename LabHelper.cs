using System;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ImageBank
{
    public static class LabHelper
    {
        private static readonly BFMatcher _bfmatcher = new BFMatcher();
        private static readonly MSER _mser = MSER.Create();
        private const int Maxdim = 480;

        public static Mat GetColors(byte[] imagedata)
        {
            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata))
            using (var matrgb = BitmapConverter.ToMat(bitmap))
            using (var matlab = new Mat())
            using (var matresize = new Mat()) {
                Cv2.CvtColor(matrgb, matlab, ColorConversionCodes.BGR2Lab);
                var f = Maxdim / (float)Math.Min(matlab.Width, matlab.Height);
                Cv2.Resize(matlab, matresize, new Size(0, 0), f, f, InterpolationFlags.Cubic);
                var matpixels = matresize.Reshape(1, matresize.Cols * matresize.Rows);
                return matpixels;
            }
        }

        public static float[] GetLab(Mat matcolors, Mat matcenters)
        {
            var hist = new float[256];
            using (var matf = new Mat()) {
                matcolors.ConvertTo(matf, MatType.CV_32F);
                var matches = _bfmatcher.KnnMatch(matf, matcenters, k:2);
                foreach (var items in matches.Where(x => x.Length > 1)) {
                    if (items[0].Distance < 0.5f * items[1].Distance) {
                        hist[items[0].TrainIdx]++;
                    }
                }

                var sum = hist.Sum();
                for (var i = 0; i < 256; i++) {
                    hist[i] = (float)Math.Sqrt(hist[i] / sum);
                }
            }

            return hist;
        }

        public static float GetDistance(float[] x, float[] y)
        {
            var sum = 0f;
            for (var i = 0; i < x.Length; i++) {
                 sum += x[i] * y[i];
            }

            var sim = (float)Math.Sqrt(1f - sum);
            return sim;
        }

        public static Mat GetLab(byte[] imagedata, string name)
        {
            var matlabc = new Mat();
            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
                    return null;
                }

                using (var matraw = bitmap.ToMat())
                using (var mat = new Mat()) {
                    var f = Maxdim / (float)Math.Min(matraw.Width, matraw.Height);
                    Cv2.Resize(matraw, mat, new Size(0, 0), f, f, InterpolationFlags.Cubic);

                    using (var matlab = new Mat()) {
                        Cv2.CvtColor(mat, matlab, ColorConversionCodes.BGR2Lab);
                        _mser.MinArea = 64;
                        _mser.MaxArea = mat.Cols * mat.Rows / 16;
                        _mser.DetectRegions(matlab, out var msers, out var bboxes);
                        msers = msers.OrderByDescending(e => e.Length).Take(256).ToArray();
                        using (var matout = new Mat(mat.Rows, mat.Cols, mat.Type())) {
                            foreach (var mser in msers) {
                                var sumrgb = new float[3];
                                foreach (var point in mser) {
                                    var vec = mat.At<Vec3b>(point.Y, point.X);
                                    sumrgb[0] += vec.Item0;
                                    sumrgb[1] += vec.Item1;
                                    sumrgb[2] += vec.Item2;
                                }

                                var avg = new Vec3b(
                                    (byte)(sumrgb[0] / mser.Length),
                                    (byte)(sumrgb[1] / mser.Length),
                                    (byte)(sumrgb[2] / mser.Length));

                                foreach (var point in mser) {
                                    matout.At<Vec3b>(point.Y, point.X) = avg;
                                }
                            }

                            matout.SaveImage($"{name}-out.png");
                        }
                    }
                }
            }

            return matlabc;
        }

        public static float GetDistance(Mat x, Mat y)
        {
            var dmatch = _bfmatcher.Match(x, y);
            var distance = dmatch.Average(e => e.Distance);
            return distance;
        }
    }
}
