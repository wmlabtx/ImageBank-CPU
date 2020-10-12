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
        public static bool Compute(Bitmap bitmap, out ColorLAB[] spectre)
        {
            Contract.Requires(bitmap != null && bitmap.Width > 0 && bitmap.Height > 0);
            spectre = null;
            using (var bitmapx = Helper.ResizeBitmap(bitmap, 8, 8)) {
                using (var matx = BitmapConverter.ToMat(bitmapx)) {
                    matx.GetArray<Vec3b>(out var rgbpixels);
                    spectre = new ColorLAB[rgbpixels.Length];
                    for (var i = 0; i < rgbpixels.Length; i++) {
                        var colorRGB = new ColorRGB(rgbpixels[i].Item2, rgbpixels[i].Item1, rgbpixels[i].Item0);
                        spectre[i] = new ColorLAB(colorRGB);
                    }
                }
            }

            return true;
        }

        public static float GetDistance(ColorLAB[] x, ColorLAB[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            var list = new List<Tuple<int, int, float>>();
            var xoffset = 0;
            while (xoffset < x.Length) {
                var yoffset = 0;
                while (yoffset < y.Length) {
                    var distance = x[xoffset].IRGB == y[yoffset].IRGB ?
                        x[xoffset].CIEDE2000(y[yoffset]) :
                        64f;

                    list.Add(new Tuple<int, int, float>(xoffset, yoffset, distance));
                    yoffset++;
                }

                xoffset++;
            }

            list = list.OrderBy(e => e.Item3).ToList();
            var distances = new List<float>();
            while (list.Count > 0) {
                var minx = list[0].Item1;
                var miny = list[0].Item2;
                var mind = list[0].Item3;
                distances.Add(mind);
                list.RemoveAll(e => e.Item1 == minx || e.Item2 == miny);
            }

            var sum = 0f;
            var count = 0f;
            var k = 1f;
            for (var i = 0; i < distances.Count; i++) {
                sum += distances[i] * k;
                count += k;
                k *= 0.9f;
            }

            var avgdistance = sum / count;
            return avgdistance;
        }

        public static void ToBuffer(ColorLAB[] spectre, out byte[] blab, out byte[] brgb)
        {
            Contract.Requires(spectre != null);

            brgb = new byte[spectre.Length];
            var fb = new float[spectre.Length * 3];
            for (var i = 0; i < spectre.Length; i++) {
                fb[i * 3] = spectre[i].L;
                fb[i * 3 + 1] = spectre[i].A;
                fb[i * 3 + 2] = spectre[i].B;
                brgb[i] = spectre[i].IRGB;
            }

            blab = new byte[fb.Length * sizeof(float)];
            Buffer.BlockCopy(fb, 0, blab, 0, blab.Length);
        }

        public static ColorLAB[] FromBuffer(byte[] blab, byte[] brgb)
        {
            Contract.Requires(blab != null);
            Contract.Requires(brgb != null);

            var fb = new float[blab.Length / sizeof(float)];
            Buffer.BlockCopy(blab, 0, fb, 0, blab.Length);
            var spectre = new ColorLAB[fb.Length / 3];
            for (var i = 0; i < spectre.Length; i++) {
                spectre[i] = new ColorLAB(fb[i * 3], fb[i * 3 + 1], fb[i * 3 + 2], brgb[i]);
            }

            return spectre;
        }

        public static int GetMatch(byte[] x, byte[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            var i = 0;
            var j = 0;
            var m = 0;
            while (i < x.Length && j < y.Length) {
                if (x[i] == y[j]) {
                    i++;
                    j++;
                    m++;
                }
                else {
                    if (x[i] < y[j]) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            return m;
        }
    }
}
