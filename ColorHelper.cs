using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace ImageBank
{
    public static class ColorHelper
    {
        private const int PaletteSize = 256;
        private const int PaletteSizeSqrt = 16;
        private const int DIM = 384;
        private const int BORDER = 32;

        private static byte[] _palette = new byte[PaletteSize * 3];
        private static ColorLAB[] _labs = new ColorLAB[PaletteSize];

        public static void CreateRandomPalette()
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

        private static void ConvertPalette(byte[] palette, ref ColorLAB[] labs)
        {
            for (var i = 0; i < PaletteSize; i++) {
                ToLAB(palette[i * 3], palette[i * 3 + 1], palette[i * 3 + 2], out var l, out var a, out var b);
                labs[i].L = l;
                labs[i].A = a;
                labs[i].B = b;
            }
        }

        private static void ConvertPalette(ColorLAB[] labs, ref byte[] palette)
        {
            for (var i = 0; i < PaletteSize; i++) {
                ToRGB(labs[i].L, labs[i].A, labs[i].B, out var r, out var g, out var b);
                palette[i * 3] = r;
                palette[i * 3 + 1] = g;
                palette[i * 3 + 2] = b;
            }
        }

        private static void Draw()
        {
            const int ColorPlateDim = 64;
            const int ColorPlateBorder = 8;
            using (Bitmap bitmap = new Bitmap(PaletteSizeSqrt * ColorPlateDim + ColorPlateBorder, PaletteSizeSqrt * ColorPlateDim + ColorPlateBorder, PixelFormat.Format24bppRgb))
            using (Graphics g = Graphics.FromImage(bitmap)) {
                for (var y = 0; y < PaletteSizeSqrt; y++) {
                    for (var x = 0; x < PaletteSizeSqrt; x++) {
                        var p = y * PaletteSizeSqrt + x;
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

        private static double CubicRoot(double n)
        {
            return Math.Pow(n, 1.0 / 3.0);
        }

        private static double PivotRgb(double n)
        {
            return (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
        }

        private static double PivotXyz(double n)
        {
            return n > 0.008856 ? CubicRoot(n) : (903.3 * n + 16.0) / 116.0;
        }

        public static void ToLAB(byte rb, byte gb, byte bb, out double l, out double a, out double b)
        {
            var rf = PivotRgb(rb / 255.0);
            var gf = PivotRgb(gb / 255.0);
            var bf = PivotRgb(bb / 255.0);

            var x = rf * 0.4124 + gf * 0.3576 + bf * 0.1805;
            var y = rf * 0.2126 + gf * 0.7152 + bf * 0.0722;
            var z = rf * 0.0193 + gf * 0.1192 + bf * 0.9505;

            x = PivotXyz(x / 95.047);
            y = PivotXyz(y / 100.0);
            z = PivotXyz(z / 108.883);

            l = Math.Max(0, 116.0 * y - 16.0);
            a = 500.0 * (x - y);
            b = 200.0 * (y - z);
        }

        private static byte ToRgb(double n)
        {
            return (byte)Math.Min(255, Math.Max(0, (int)(n * 255)));
        }

        public static void ToRGB(double l, double a, double b, out byte rb, out byte gb, out byte bb)
        {
            var y = (l + 16.0) / 116.0;
            var x = a / 500.0 + y;
            var z = y - b / 200.0;

            x = (Math.Pow(x, 3) > 0.008856 ? Math.Pow(x, 3) : (x - 16.0 / 116.0) / 7.787) * 0.95047;
            y = (Math.Pow(y, 3) > 0.008856 ? Math.Pow(y, 3) : (y - 16.0 / 116.0) / 7.787);
            z = (Math.Pow(z, 3) > 0.008856 ? Math.Pow(z, 3) : (z - 16.0 / 116.0) / 7.787) * 1.08883;

            var rd = x * 3.2406 + y * -1.5372 + z * -0.4986;
            var gd = x * -0.9689 + y * 1.8758 + z * 0.0415;
            var bd = x * 0.0557 + y * -0.2040 + z * 1.0570;

            rd = rd > 0.0031308 ? 1.055 * Math.Pow(rd, 1 / 2.4) - 0.055 : 12.92 * rd;
            gd = gd > 0.0031308 ? 1.055 * Math.Pow(gd, 1 / 2.4) - 0.055 : 12.92 * gd;
            bd = bd > 0.0031308 ? 1.055 * Math.Pow(bd, 1 / 2.4) - 0.055 : 12.92 * bd;

            rb = ToRgb(rd);
            gb = ToRgb(gd);
            bb = ToRgb(bd);
        }

        private static double Distance(double a, double b)
        {
            return (a - b) * (a - b);
        }

        public static double Cie1976(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            var differences = Distance(l1, l2) + Distance(a1, a2) + Distance(b1, b2);
            return Math.Sqrt(differences);
        }

        public static double Cie1994(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            var deltaL = l1 - l2;
            var deltaA = a1 - a2;
            var deltaB = b1 - b2;

            var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
            var c2 = Math.Sqrt(a2 * a2 + b2 * b2);
            var deltaC = c1 - c2;

            var deltaH = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
            deltaH = deltaH < 0 ? 0 : Math.Sqrt(deltaH);

            const double sl = 1.0;
            const double kc = 1.0;
            const double kh = 1.0;

            var sc = 1.0 + 0.045 * c1;
            var sh = 1.0 + 0.015 * c1;

            var deltaLKlsl = deltaL / (1.0 * sl);
            var deltaCkcsc = deltaC / (kc * sc);
            var deltaHkhsh = deltaH / (kh * sh);
            var i = deltaLKlsl * deltaLKlsl + deltaCkcsc * deltaCkcsc + deltaHkhsh * deltaHkhsh;
            return i < 0 ? 0 : Math.Sqrt(i);
        }

        private static void FindCluster(ColorLAB lab, out int cluster, out double distance)
        {
            cluster = 0;
            distance = double.MaxValue;
            for (var i = 0; i < _labs.Length; i++) {
                var d = Cie1976(lab.L, lab.A, lab.B, _labs[i].L, _labs[i].A, _labs[i].B);
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
                var distance = Cie1976(lab.L, lab.A, lab.B, _labs[i].L, _labs[i].A, _labs[i].B);
                if (distance < vd) {
                    wd = vd;
                    v = i;
                    vd = distance;
                }
            }

            return vd < 0.75 * wd ? v : -1;
        }

        public static void Learn()
        {
            ColorLAB[] _labcolors = new ColorLAB[128 * 128 * 128];
            ColorLAB[] _clusters = new ColorLAB[PaletteSize];
            int[] _counters = new int[PaletteSize];
            Debug.WriteLine("Building LAB colors");
            var counter = 0;
            for (int ri = 0; ri < 256; ri += 2) {
                for (int gi = 0; gi < 256; gi += 2) {
                    for (int bi = 0; bi < 256; bi += 2) {
                        ToLAB((byte)ri, (byte)gi, (byte)bi, out double l, out double a, out double b);
                        _labcolors[counter].L = l;
                        _labcolors[counter].A = a;
                        _labcolors[counter].B = b;
                        counter++;
                    }
                }
            }

            var epoh = 0;
            double error;
            while (true) {
                error = 0.0;
                for (var i = 0; i < PaletteSize; i++) {
                    _clusters[i].L = 0.0;
                    _clusters[i].A = 0.0;
                    _clusters[i].B = 0.0;
                    _counters[i] = 0;
                }

                for (var i = 0; i < _labcolors.Length; i++) {
                    FindCluster(_labcolors[i], out int cluster, out double distance);
                    _clusters[cluster].L += _labcolors[i].L;
                    _clusters[cluster].A += _labcolors[i].A;
                    _clusters[cluster].B += _labcolors[i].B;
                    _counters[cluster]++;
                    error += distance;
                }

                for (var i = 0; i < PaletteSize; i++) {
                    _clusters[i].L /= _counters[i];
                    _clusters[i].A /= _counters[i];
                    _clusters[i].B /= _counters[i];
                }

                Array.Copy(_clusters, _labs, _clusters.Length);
                _labs = _labs.OrderBy(e => e.L).ThenBy(e => e.A).ThenBy(e => e.B).ToArray();

                ConvertPalette(_labs, ref _palette);
                Draw();
                Save();

                epoh++;

                error /= 128 * 128 * 128;
                var min = _counters.Min();
                var max = _counters.Max();
                Debug.WriteLine($"{epoh}: e:{error:F4} {min}-{max}");
            }
        }

        public static float[] CalculateHistogram(Bitmap bitmap)
        {
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
                        ToLAB(rbyte, gbyte, bbyte, out var l, out var a, out var b);
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

        public static bool IsHistogram(float[] x)
        {
            foreach(var e in x) {
                if (e > 0.0001f) {
                    return true;
                }
            }

            return false;
        }

        public static string GetDescription(float[] x)
        {
            if (x == null || x.Length == 0 || !IsHistogram(x)) {
                return "no data";
            }

            var value = x.Max();
            var index = Array.IndexOf(x, value);
            var result = $"{index:X2}:{value * 100f:F1}%";
            return result;
        }

        public static float GetDistance(float[] x, float[] y)
        {
            if (x.Length == 0 || y.Length == 0 || x.Length != y.Length || !IsHistogram(x) || !IsHistogram(y)) {
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