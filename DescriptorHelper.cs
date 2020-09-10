using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public static class DescriptorHelper
    {
        public static int GetHammimgDistance(ulong[] x, int xoffset, ulong[] y, int yoffset)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var distance = 0;
            for (var i = 0; i < 4; i++) {
                distance += Intrinsic.PopCnt(x[xoffset + i] ^ y[yoffset + i]);
            }

            return distance;
        }

        public static float GetDistance(ulong[] x, ulong[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var list = new List<Tuple<int, int, int>>();
            var xoffset = 0;
            var xmax = Math.Min(AppConsts.MaxDescriptorsInImage * 4, x.Length);
            var ymax = Math.Min(AppConsts.MaxDescriptorsInImage * 4, y.Length);
            while (xoffset < xmax) {
                var yoffset = 0;
                while (yoffset < ymax) {
                    var hammingdistance = GetHammimgDistance(x, xoffset, y, yoffset);
                    list.Add(new Tuple<int, int, int>(xoffset, yoffset, hammingdistance));
                    yoffset += 4;
                }

                xoffset += 4;
            }

            list = list.OrderBy(e => e.Item3).ToList();
            var sum = 0f;
            var count = 0f;
            var k = 1f;
            while (list.Count > 0) {
                var minx = list[0].Item1;
                var miny = list[0].Item2;
                var mind = list[0].Item3;
                sum += mind * k;
                count += k;
                k *= 0.9f;
                list.RemoveAll(e => e.Item1 == minx || e.Item2 == miny);
            }

            var distance = sum / count;
            return distance;
        }


        public static bool Compute(Bitmap bitmap, out ulong[] descriptors)
        {
            Contract.Requires(bitmap != null);
            descriptors = null;
            using (var orb = ORB.Create(AppConsts.MaxDescriptorsInImage)) {
                using (var matcolor = BitmapConverter.ToMat(bitmap)) {
                    if (matcolor.Width == 0 || matcolor.Height == 0) {
                        return false;
                    }

                    using (var mat = new Mat()) {
                        Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                        using (var orbdescriptors = new Mat()) {
                            orb.DetectAndCompute(mat, null, out _, orbdescriptors);
                            if (orbdescriptors.Rows == 0) {
                                return false;
                            }

                            orbdescriptors.GetArray<byte>(out var bdata);
                            var len = Math.Min(AppConsts.MaxDescriptorsInImage, orbdescriptors.Rows);
                            descriptors = new ulong[len * 4];
                            Buffer.BlockCopy(bdata, 0, descriptors, 0, bdata.Length);
                        }
                    }
                }
            }

            return true;
        }
    }
}
