using System;
using System.Drawing;

namespace ImageBank
{
    public class HsHelper
    {
        /*
        const int NUMPALS = 16;

        public static byte[] Compute(Bitmap bitmap)
        {            
            const int DIM = 512;
            var hs = new byte[4 * NUMPALS * 3];
            using (var matsource = bitmap.ToMat())
            using (var matx = matsource.Resize(new OpenCvSharp.Size(DIM, DIM), 0, 0, InterpolationFlags.Cubic))
            using (var matycc = new Mat())
            {
                Cv2.CvtColor(matx, matycc, ColorConversionCodes.BGR2Lab);
                var zones = new Rect[4];
                zones[0] = new Rect(0, 0, matycc.Cols, matycc.Rows / 2);
                zones[1] = new Rect(0, 0, matycc.Cols / 2, matycc.Rows);
                zones[2] = new Rect(0, matycc.Rows / 2, matycc.Cols, matycc.Rows / 2);
                zones[3] = new Rect(matycc.Cols / 2, 0, matycc.Cols / 2, matycc.Rows);
                using (var matpal = new Mat())
                {
                    for (var zone = 0; zone < 4; zone++)
                    {
                        using (var matp = matycc.Clone(zones[zone]))
                        {
                            var matv = new Mat();
                            matp.ConvertTo(matv, MatType.CV_32F);
                            matv = matv.Reshape(1, (int)matv.Total());


                            using (var bestlabels = new Mat())
                            using (var centers = new Mat())
                            {
                                Cv2.Kmeans(matv, NUMPALS, bestlabels, new TermCriteria(CriteriaType.Eps, 100, 0.25), 100, KMeansFlags.PpCenters, centers);
                                matpal.PushBack(centers);
                            }
                        }
                    }

                    using(var matb = new Mat())
                    {
                        matpal.ConvertTo(matb, MatType.CV_8U);
                        matb.GetArray(out hs);
                    }
                }
            }

            return hs;
        }

        public static float Compare(byte[] x, byte[] y)
        {
            var minsum = float.MaxValue;
            for (var xo = 0; xo < x.Length; xo += NUMPALS * 3)
            {
                for (var yo = 0; yo < y.Length; yo += NUMPALS * 3)
                {
                    var sum = 0f;
                    for (var xop = 0; xop < NUMPALS * 3; xop += 3)
                    {
                        var mindp = float.MaxValue;
                        for (var yop = 0; yop < NUMPALS * 3; yop += 3)
                        {
                            var dy = x[xo + xop] - y[yo + yop];
                            var dr = x[xo + xop + 1] - y[yo + yop + 1];
                            var db = x[xo + xop + 2] - y[yo + yop + 2];
                            var dp = (float)Math.Sqrt(dy*dy + dr*dr + db*db);
                            mindp = Math.Min(mindp, dp);
                        }

                        sum += mindp;
                    }

                    minsum = Math.Min(minsum, sum);
                }
            }

            return minsum;
        }
        */
    }
}
