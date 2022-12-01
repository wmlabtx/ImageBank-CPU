using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace ImageBank
{
    public static class AppPalette
    {
        private const int PaletteSize = 64;

        private static byte[] _palette = new byte[PaletteSize * 3];
        private static ColorLAB[] _labs = new ColorLAB[PaletteSize];

        public static void Create()
        {
            for (var i = 0; i < PaletteSize; i++) {
                _palette[i * 3] = (byte)AppVars.IRandom(0, 255);
                _palette[i * 3 + 1] = (byte)AppVars.IRandom(0, 255);
                _palette[i * 3 + 2] = (byte)AppVars.IRandom(0, 255);
            }

            ConvertPalette(_palette, ref _labs);
            Draw();
            Save();
        }

        private static void Draw()
        {
            const int ColorPlateDim = 64;
            const int ColorPlateBorder = 8;
            using (Bitmap bitmap = new Bitmap(8 * ColorPlateDim + ColorPlateBorder, 8 * ColorPlateDim + ColorPlateBorder, PixelFormat.Format24bppRgb))
            using (Graphics g = Graphics.FromImage(bitmap)) {
                for (var y = 0; y < 8; y++) {
                    for (var x = 0; x < 8; x++) {
                        var p = y * 8 + x;
                        var rect = new Rectangle(x * ColorPlateDim + ColorPlateBorder, y * ColorPlateDim + ColorPlateBorder, ColorPlateDim - ColorPlateBorder, ColorPlateDim - ColorPlateBorder);
                        var color = Color.FromArgb(_palette[p * 3], _palette[p * 3 + 1], _palette[p * 3 + 2]);
                        using (var brush = new SolidBrush(color)) {
                            g.FillRectangle(brush, rect);
                        }

                        p += 3;
                    }
                }

                bitmap.Save(AppConsts.FilePalette, ImageFormat.Png);
            }
        }

        private static void Save()
        {
            AppDatabase.VarsUpdateProperty(AppConsts.AttributePalette, _palette);
        }

        public static void Set(byte[] palette)
        {
            Array.Copy(palette, _palette, PaletteSize * 3);
            ConvertPalette(_palette, ref _labs);
        }

        private static void ConvertPalette(byte[] palette, ref ColorLAB[] labs)
        {
            for (var i = 0; i < PaletteSize; i++) {
                ColorHelper.ToLAB(palette[i * 3], palette[i * 3 + 1], palette[i * 3 + 2], out var l, out var a, out var b);
                labs[i].L = l;
                labs[i].A = a;
                labs[i].B = b;
            }
        }

        private static void ConvertPalette(ColorLAB[] labs, ref byte[] palette)
        {
            for (var i = 0; i < PaletteSize; i++) {
                ColorHelper.ToRGB(labs[i].L, labs[i].A, labs[i].B, out var r, out var g, out var b);
                palette[i * 3] = r;
                palette[i * 3 + 1] = g;
                palette[i * 3 + 2] = b;
            }
        }

        private static void FindCluster(ColorLAB lab, out int cluster, out double distance)
        {
            cluster = 0;
            distance = double.MaxValue;
            for (var i = 0; i < _labs.Length; i++) {
                var d = ColorHelper.Cie1976(lab.L, lab.A, lab.B, _labs[i].L, _labs[i].A, _labs[i].B);
                if (d < distance) {
                    cluster = i;
                    distance = d;
                }
            }
        }

        private static int FindFuzzyCluster(ColorLAB lab)
        {
            var v = 0;
            var vd = double.MaxValue;
            var wd = double.MaxValue;
            for (var i = 0; i < _labs.Length; i++) {
                var distance = ColorHelper.Cie1976(lab.L, lab.A, lab.B, _labs[i].L, _labs[i].A, _labs[i].B);
                if (distance < vd) {
                    wd = vd;
                    v = i;
                    vd = distance;
                }
            }

            return vd < 0.75 * wd ? v: -1;
        }

        public static double Learn(Bitmap bitmap)
        {
            const int DIM = 384;
            const int BORDER = 32;
            const double W = DIM * DIM;
            var colors = new List<ColorLAB>[PaletteSize];
            var errors = new double[PaletteSize];
            for (var i = 0; i < colors.Length; i++) {
                colors[i] = new List<ColorLAB>();
                errors[i] = 0.0;
            }

            using (var bitmapcut = BitmapHelper.ScaleAndCut(bitmap, DIM, BORDER)) {
                var bitmapdata = bitmapcut.LockBits(new Rectangle(0, 0, bitmapcut.Width, bitmapcut.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var stride = bitmapdata.Stride;
                var data = new byte[Math.Abs(bitmapdata.Stride * bitmapdata.Height)];
                Marshal.Copy(bitmapdata.Scan0, data, 0, data.Length);
                bitmapcut.UnlockBits(bitmapdata);
                var offsety = 0;
                for (var y = 0; y < bitmapcut.Height; y++) {
                    var offsetx = offsety;
                    for (var x = 0; x < bitmapcut.Width; x++) {
                        var rbyte = data[offsetx + 2];
                        var gbyte = data[offsetx + 1];
                        var bbyte = data[offsetx];
                        offsetx += 3;
                        ColorHelper.ToLAB(rbyte, gbyte, bbyte, out var l, out var a, out var b);
                        var lab = new ColorLAB() { L = l, A = a, B = b };
                        FindCluster(lab, out int cluster, out double distance);
                        colors[cluster].Add(lab);
                        errors[cluster] += distance * distance;
                    }

                    offsety += stride;
                }
            }

            var sumerror = errors.Sum();

            for (var i = 0; i < colors.Length; i++) {
                var suml = _labs[i].L * W;
                var suma = _labs[i].A * W;
                var sumb = _labs[i].B * W;
                foreach (var c in colors[i]) {
                    suml += c.L;
                    suma += c.A;
                    sumb += c.B;
                }

                _labs[i].L = suml / (W + colors[i].Count);
                _labs[i].A = suma / (W + colors[i].Count);
                _labs[i].B = sumb / (W + colors[i].Count);
            }

            ConvertPalette(_labs, ref _palette);
            Draw();
            Save();

            return sumerror;
        }

        public static float[] ComputeHist(Bitmap bitmap)
        {
            const int DIM = 224;
            const int BORDER = 32;
            var hist = new float[PaletteSize];
            using (var bitmapcut = BitmapHelper.ScaleAndCut(bitmap, DIM, BORDER)) {
                var bitmapdata = bitmapcut.LockBits(new Rectangle(0, 0, bitmapcut.Width, bitmapcut.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var stride = bitmapdata.Stride;
                var data = new byte[Math.Abs(bitmapdata.Stride * bitmapdata.Height)];
                Marshal.Copy(bitmapdata.Scan0, data, 0, data.Length);
                bitmapcut.UnlockBits(bitmapdata);
                var offsety = 0;
                for (var y = 0; y < bitmapcut.Height; y++) {
                    var offsetx = offsety;
                    for (var x = 0; x < bitmapcut.Width; x++) {
                        var rbyte = data[offsetx + 2];
                        var gbyte = data[offsetx + 1];
                        var bbyte = data[offsetx];
                        offsetx += 3;
                        ColorHelper.ToLAB(rbyte, gbyte, bbyte, out var l, out var a, out var b);
                        var lab = new ColorLAB() { L = l, A = a, B = b };
                        var cluster = FindFuzzyCluster(lab);
                        if (cluster >= 0) {
                            hist[cluster]++;
                        }
                    }

                    offsety += stride;
                }
            }

            var sum = hist.Sum();
            for (var i = 0; i < PaletteSize; i++) {
                hist[i] = (float)Math.Sqrt(hist[i] / sum);
            }

            return hist;
        }

        public static float GetDistance(float[] x, float[] y)
        {
            if (x.Length == 0 || y.Length == 0 || x.Length != y.Length) {
                return 1f;
            }

            var sum = 0f;
            for (var i = 0; i < x.Length; i++) {
                sum += x[i] * y[i];
            }

            sum = (float)Math.Min(1.0, sum);
            var sim = (float)Math.Sqrt(1f - sum);
            return sim;
        }
    }
}
