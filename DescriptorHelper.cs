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
        public static bool Compute(Bitmap bitmap, out byte[] colors)
        {
            Contract.Requires(bitmap != null && bitmap.Width > 0 && bitmap.Height > 0);
            colors = null;
            using (var bitmapx = Helper.ResizeBitmap(bitmap, 10, 10)) {
                using (var matx = BitmapConverter.ToMat(bitmapx)) {
                    matx.GetArray<Vec3b>(out var rgbpixels);
                    var lcolors = new List<byte[]>();
                    for (var i = 0; i < 100; i++) {
                        var pixel = new byte[4];
                        pixel[0] = rgbpixels[i].Item2;
                        pixel[1] = rgbpixels[i].Item1;
                        pixel[2] = rgbpixels[i].Item0;
                        var r2 = rgbpixels[i].Item2 >> 6;
                        var g2 = rgbpixels[i].Item1 >> 6;
                        var b2 = rgbpixels[i].Item0 >> 6;
                        pixel[3] = (byte)((r2 << 4) | (g2 << 2) | b2);
                        lcolors.Add(pixel);
                    }

                    lcolors = lcolors.OrderBy(e => e[3]).ToList();
                    colors = new byte[400];
                    for (var i = 0; i < 100; i++) {
                        Buffer.BlockCopy(lcolors[i], 0, colors, i * 4, 4);
                    }
                }
            }

            return true;
        }

        public static float GetDistance(byte[] x, byte[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            var list = new List<Tuple<int, int, float>>();
            var xoffset = 0;
            while (xoffset < x.Length) {
                var yoffset = 0;
                while (yoffset < y.Length) {
                    if( x[xoffset + 3] == y[yoffset + 3]) {
                        var mr = (x[xoffset] + y[yoffset]) / 2f; // 127.5
                        var dr2 = (float)(x[xoffset] - y[yoffset]) * (x[xoffset] - y[yoffset]); // 65025
                        var dg2 = (float)(x[xoffset + 1] - y[yoffset + 1]) * (x[xoffset + 1] - y[yoffset + 1]);
                        var db2 = (float)(x[xoffset + 2] - y[yoffset + 2]) * (x[xoffset + 2] - y[yoffset + 2]);
                        var distance = (float)Math.Sqrt(              // max=768f
                                ((2f + (mr / 256f)) * dr2) +          // 162435
                                (4f * dg2) +                          // 260100
                                ((2f + ((255f - mr) / 256f)) * db2)   // 162435
                            );

                        list.Add(new Tuple<int, int, float>(xoffset, yoffset, distance));
                    }
                    
                    yoffset += 4;
                }

                xoffset += 4;
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
            var maxi = Math.Min(100, distances.Count);
            var i = 0;
            for (; i < maxi; i++) {
                sum += distances[i] * k;
                count += k;
                k *= 0.9f;
                if (k < 0.01) {
                    break;
                }
            }

            if (k >= 0.01) {
                for (; i < maxi; i++) {
                    sum += 768f * k;
                    count += k;
                    k *= 0.9f;
                    if (k < 0.01) {
                        break;
                    }
                }
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
                if (x[i + 3] == y[j + 3]) {
                    i += 4;
                    j += 4;
                    m++;
                }
                else {
                    if (x[i] < y[j]) {
                        i += 4;
                    }
                    else {
                        j += 4;
                    }
                }
            }

            return m;
        }
    }
}
