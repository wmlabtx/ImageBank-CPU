using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public static class OrbHepler
    {
        const int MAXDIM = 768;
        const int MAXDESCRIPTORS = 250;
        const int PIESES = 4;
        private static readonly FastFeatureDetector _fast = FastFeatureDetector.Create();
        private static readonly ORB _orb = ORB.Create();

        public static void ComputeBlob(Bitmap bitmap, out byte[] map, out ulong[] descriptors)
        {
            map = null;
            descriptors = null;

            using (var matsource = bitmap.ToMat())
            using (var matcolor = new Mat())
            {
                var f = (double)MAXDIM / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat())
                {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    mat.SaveImage("mat.png");
                    var keypoints = _fast.Detect(mat);
                    if (keypoints.Length > 0)
                    {
                        keypoints = keypoints.OrderByDescending(e => e.Octave).ThenByDescending(e => e.Response).Take(MAXDESCRIPTORS).ToArray();
                        using (var matdescriptors = new Mat())
                        {
                            _orb.Compute(mat, ref keypoints, matdescriptors);
                            if (matdescriptors.Rows > 0 && keypoints.Length > 0)
                            {
                                using (var matkeypoints = new Mat())
                                {
                                    Cv2.DrawKeypoints(matcolor, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                    matkeypoints.SaveImage("matkeypoints.png");
                                }

                                matdescriptors.GetArray(out byte[] array);
                                descriptors = ImageHelper.ArrayTo64(array);
                                map = new byte[keypoints.Length];
                                for (var i = 0; i < keypoints.Length; i++)
                                {
                                    var ix = (int)(keypoints[i].Pt.X * PIESES / mat.Width);
                                    var iy = (int)(keypoints[i].Pt.Y * PIESES / mat.Height);
                                    map[i] = (byte)(iy * PIESES + ix);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static float CompareBlob(byte[] m1, ulong[] d1, byte[] m2, ulong[] d2)
        {
            const int MAXDISTANCE = 256;
            var minhamming = new int[PIESES * PIESES];
            for (var i = 0; i < minhamming.Length; i++)
            {
                minhamming[i] = MAXDISTANCE;
            }

            for (var i = 0; i < m1.Length; i++)
            {
                for (var j = 0; j < m2.Length; j++)
                {
                    if (m1[i] != m2[j])
                    {
                        continue;
                    }

                    var hamming = 0;
                    for (var b = 0; b < 4; b++)
                    {
                        hamming += Intrinsic.PopCnt(d1[i * 4 + b] ^ d2[j * 4 + b]);
                    }

                    if (hamming < minhamming[m1[i]])
                    {
                        minhamming[m1[i]] = hamming;
                    }
                }
            }

            var a1 = minhamming.Where(e => e != MAXDISTANCE).ToArray();
            var c1 = Math.Max(1, a1.Length / 4);
            var a2 = a1.OrderBy(e => e).Take(c1).ToArray();
            var distance = (float)a2.Average();
            return (float)distance;
        }
    }
}
