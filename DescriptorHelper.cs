using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections;
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
                k *= 0.5f;
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
                            orb.DetectAndCompute(mat, null, out var keypoints, orbdescriptors);
                            if (orbdescriptors.Rows == 0 || keypoints.Length < AppConsts.MaxClustersInImage) {
                                return false;
                            }

                            orbdescriptors.GetArray<byte>(out var bdata);
                            var udata = new ulong[bdata.Length / sizeof(ulong)];
                            Buffer.BlockCopy(bdata, 0, udata, 0, bdata.Length);

                            var fdata = new float[keypoints.Length * 2];
                            for (var i = 0; i < keypoints.Length; i++) {
                                fdata[i * 2] = keypoints[i].Pt.X;
                                fdata[i * 2 + 1] = keypoints[i].Pt.Y;
                            }

                            using (var matkeypoints = new Mat(keypoints.Length, 2, MatType.CV_32F)) {
                                matkeypoints.SetArray(fdata);
                                using (var matlabels = new Mat()) {
                                    Cv2.Kmeans(matkeypoints, AppConsts.MaxClustersInImage, matlabels, TermCriteria.Both(100, 0.1), 1, KMeansFlags.PpCenters);
                                    matlabels.GetArray<int>(out var labels);
                                    descriptors = new ulong[AppConsts.MaxClustersInImage * 4];
                                    for (var ic = 0; ic < AppConsts.MaxClustersInImage; ic++) {
                                        var bitscounter = new int[256];
                                        var orb32 = new byte[32];
                                        var clustersize = 0;
                                        int bit;
                                        for (var i = 0; i < labels.Length; i++) {
                                            if (labels[i] != ic) {
                                                continue;
                                            }

                                            Buffer.BlockCopy(udata, i * 32, orb32, 0, 32);
                                            for (bit = 0; bit < 256; bit++) {
                                                var dby = bit / 8;
                                                var dbi = bit % 8;
                                                if ((orb32[dby] & (1 << dbi)) != 0) {
                                                    bitscounter[bit]++;
                                                }
                                            }

                                            clustersize++;
                                        }

                                        Array.Clear(orb32, 0, 32);
                                        var avg = clustersize / 2;
                                        for (bit = 0; bit < 256; bit++) {
                                            if (bitscounter[bit] > avg) {
                                                var dby = bit / 8;
                                                var dbi = bit % 8;
                                                orb32[dby] |= (byte)(1 << dbi);
                                            }
                                        }

                                        Buffer.BlockCopy(orb32, 0, descriptors, ic * 32, 32);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
