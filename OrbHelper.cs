using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public static class OrbHelper
    {
        private static readonly BFMatcher _bfMatcher = new BFMatcher(NormTypes.Hamming, true);

        public static bool ComputeOrbs(Bitmap thumb, out Mat orbs)
        {
            Contract.Requires(thumb != null);
            orbs = null;
            using (var orb = ORB.Create(AppConsts.MaxDescriptorsInImage)) {
                try {
                    using (var matcolor = BitmapConverter.ToMat(thumb)) {
                        if (matcolor.Width == 0 || matcolor.Height == 0) {
                            return false;
                        }
                        
                        using (var mat = new Mat()) {
                            Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                            orbs = new Mat();
                            orb.DetectAndCompute(mat, null, out _, orbs);
                            if (orbs.Rows == 0 || orbs.Cols != 32) {
                                throw new Exception();
                            }

                            while (orbs.Rows > AppConsts.MaxDescriptorsInImage) {
                                orbs = orbs.RowRange(0, AppConsts.MaxDescriptorsInImage);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex);
                    orbs = null;
                    throw;
                    //return false;
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
    }
}
