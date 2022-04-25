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
        private const int Maxdim = 768;

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
