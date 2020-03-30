using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public static class OrbHelper
    {
        public static bool ComputeOrbs(Bitmap thumb, out ulong[] vector)
        {
            Contract.Requires(thumb != null);
            vector = null;
            using (var orb = ORB.Create(AppConsts.MaxDescriptorsInImage)) {
                using (var matcolor = BitmapConverter.ToMat(thumb)) {
                    if (matcolor.Width == 0 || matcolor.Height == 0) {
                        return false;
                    }

                    using (var mat = new Mat()) {
                        Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                        using (var descriptors = new Mat()) {
                            orb.DetectAndCompute(mat, null, out _, descriptors);
                            if (descriptors.Rows == 0 || descriptors.Cols != 32) {
                                return false;
                            }

                            descriptors.GetArray(out byte[] buffer);
                            var length = Math.Min(buffer.Length, AppConsts.MaxDescriptorsInImage * 32);
                            vector = new ulong[length / sizeof(ulong)];
                            Buffer.BlockCopy(buffer, 0, vector, 0, length);
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

        public static int GetDistance(ulong[] x, int xoffset, ulong[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var mindistance = int.MaxValue;
            var yoffset = 0;
            while (yoffset < y.Length) {
                var distance = GetDistance(x, xoffset, y, yoffset);
                if (distance < mindistance) {
                    mindistance = distance;
                }

                yoffset += 4;
            }

            return mindistance;
        }

        public static float GetSim(ulong[] x, ulong[] y, int maxorbs)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var list = new List<Tuple<int, int, int>>();
            var xoffset = 0;
            var xmax = Math.Min(maxorbs * 4, x.Length);
            var ymax = Math.Min(maxorbs * 4, y.Length);
            while (xoffset < xmax) {
                var yoffset = 0;
                while (yoffset < ymax) {
                    var distance = GetDistance(x, xoffset, y, yoffset);
                    if (distance < 64) {
                        list.Add(new Tuple<int, int, int>(xoffset, yoffset, distance));
                    }

                    yoffset += 4;
                }

                xoffset += 4;
            }

            if (list.Count == 0) {
                return GetSimLow(x, y, maxorbs);
            }

            list = list.OrderBy(e => e.Item3).ToList();

            var sum = 0f;
            while (list.Count > 0) {
                var minx = list[0].Item1;
                var miny = list[0].Item2;
                var mind = list[0].Item3;
                sum += 64 - mind;
                list.RemoveAll(e => e.Item1 == minx || e.Item2 == miny);
            }

            var sim = sum * 4f / x.Length;
            return sim;
        }

        public static float GetSimLow(ulong[] x, ulong[] y, int maxorbs)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var list = new List<Tuple<int, int, int>>();
            var xoffset = 0;
            var xmax = Math.Min(maxorbs * 4, x.Length);
            var ymax = Math.Min(maxorbs * 4, y.Length);
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

            var sum = 0f;
            while (list.Count > 0) {
                var minx = list[0].Item1;
                var miny = list[0].Item2;
                var mind = list[0].Item3;
                sum += (196 - (mind - 64)) / 196f;
                list.RemoveAll(e => e.Item1 == minx || e.Item2 == miny);
            }

            var sim = sum * 4f / x.Length;
            return sim;
        }

    }
}
