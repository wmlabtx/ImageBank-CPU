using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.ImgHash;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public static class OrbHelper
    {
        public static bool Compute(Bitmap bitmap, out ulong phash, out ulong[] descriptors)
        {
            Contract.Requires(bitmap != null);
            phash = 0;
            descriptors = null;
            using (var orb = ORB.Create(AppConsts.MaxDescriptorsInImage)) {
                using (var matcolor = BitmapConverter.ToMat(bitmap)) {
                    if (matcolor.Width == 0 || matcolor.Height == 0) {
                        return false;
                    }

                    using (var mat = new Mat()) {
                        Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                        using (var phashmaker = PHash.Create())
                        using (var matphash = new Mat()) {
                            phashmaker.Compute(mat, matphash);
                            if (matphash.Rows != 1 || matphash.Cols != 8) {
                                return false;
                            }

                            matphash.GetArray(out byte[] buffer);
                            phash = BitConverter.ToUInt64(buffer, 0);
                        }

                        using (var matdescriptors = new Mat()) {
                            orb.DetectAndCompute(mat, null, out _, matdescriptors);
                            if (matdescriptors.Rows == 0 || matdescriptors.Cols != 32) {
                                return false;
                            }

                            matdescriptors.GetArray(out byte[] buffer);
                            var length = Math.Min(buffer.Length, AppConsts.MaxDescriptorsInImage * 32);
                            descriptors = new ulong[length / sizeof(ulong)];
                            Buffer.BlockCopy(buffer, 0, descriptors, 0, length);
                        }
                    }
                }
            }

            return true;
        }

        public static int GetDistance(ulong[] x, int xoffset, ulong[] y, int yoffset)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var distance = 0;
            for (var i = 0; i < 4; i++) {
                distance += Intrinsic.PopCnt(x[xoffset + i] ^ y[yoffset + i]);
            }

            return distance;
        }

        public static int[] GetMatches(ulong[] x, ulong[] y)
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
                    var distance = GetDistance(x, xoffset, y, yoffset);
                    list.Add(new Tuple<int, int, int>(xoffset, yoffset, distance));
                    yoffset += 4;
                }

                xoffset += 4;
            }

            list = list.OrderBy(e => e.Item3).ToList();
            var matches = new List<int>();
            while (list.Count > 0) {
                var minx = list[0].Item1;
                var miny = list[0].Item2;
                var mind = list[0].Item3;
                matches.Add(mind);
                list.RemoveAll(e => e.Item1 == minx || e.Item2 == miny);
            }

            return matches.ToArray();
        }
    }
}
