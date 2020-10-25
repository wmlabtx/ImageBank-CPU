using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImageBank
{
    public static class ImageHelper
    {
        private static readonly SortedDictionary<int, short> _rgb2lab = new SortedDictionary<int, short>();
        private static readonly SortedDictionary<int, double> _lab2distance = new SortedDictionary<int, double>();

        public static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
        {
            Contract.Requires(data != null);

            try {
                using (var ms = new MemoryStream(data)) {
                    bitmap = (Bitmap)Image.FromStream(ms);
                }
                
                return true;
            }
            catch (ArgumentException) {
                bitmap = null;
                return false;
            }
        }

        public static Bitmap RepixelBitmap(Bitmap bitmap)
        {
            Contract.Requires(bitmap != null);
            var bitmap24bppRgb = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap24bppRgb)) {
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }

            return bitmap24bppRgb;
        }

        public static MagicFormat GetMagicFormat(byte[] imagedata)
        {
            // https://en.wikipedia.org/wiki/List_of_file_signatures
            Contract.Requires(imagedata != null);

            if (imagedata[0] == 0xFF && imagedata[1] == 0xD8 && imagedata[2] == 0xFF) {
                return MagicFormat.Jpeg;
            }

            if (imagedata[0] == 0x52 && imagedata[1] == 0x49 && imagedata[2] == 0x46 && imagedata[3] == 0x46 &&
                imagedata[8] == 0x57 && imagedata[9] == 0x45 && imagedata[10] == 0x42 && imagedata[11] == 0x50) {
                if (imagedata[15] == ' ') {
                    return MagicFormat.WebP;
                }

                if (imagedata[15] == 'L') {
                    return MagicFormat.WebPLossLess;
                }

                return MagicFormat.Unknown;
            }

            if (imagedata[0] == 0x89 && imagedata[1] == 0x50 && imagedata[2] == 0x4E && imagedata[3] == 0x47) {
                return MagicFormat.Png;
            }

            if (imagedata[0] == 0x42 && imagedata[1] == 0x4D) {
                return MagicFormat.Bmp;
            }

            return MagicFormat.Unknown;
        }

        public static bool GetImageDataFromBitmap(Bitmap bitmap, out byte[] imagedata)
        {
            Contract.Requires(bitmap != null);

            try {
                using (var ms = new MemoryStream()) {
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    imagedata = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, imagedata, 0, (int)ms.Length);
                }

                return true;
            }
            catch (ArgumentException) {
                imagedata = null;
                return false;
            }
        }

        public static bool GetImageDataFromFile(
            string filename,
            out byte[] imagedata,
            out Bitmap bitmap,
            out ulong hash,
            out string message)
        {
            imagedata = null;
            bitmap = null;
            hash = 0;
            message = null;
            if (!File.Exists(filename)) {
                message = "missing file";
                return false;
            }

            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension)) {
                message = "no extention";
                return false;
            }

            if (
                !extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.PngExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.BmpExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpgExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpegExtension, StringComparison.OrdinalIgnoreCase)
                ) {
                message = "unknown extention";
                return false;
            }

            imagedata = File.ReadAllBytes(filename);
            if (imagedata == null || imagedata.Length == 0) {
                message = "imgdata == null || imgdata.Length == 0";
                return false;
            }

            if (extension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase)) {
                var password = Path.GetFileNameWithoutExtension(filename);
                imagedata = Helper.DecryptDat(imagedata, password);
                if (imagedata == null) {
                    message = "cannot be decrypted";
                    return false;
                }
            }

            if (extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                var password = Path.GetFileNameWithoutExtension(filename);
                imagedata = Helper.Decrypt(imagedata, password);
                if (imagedata == null) {
                    message = "cannot be decrypted";
                    return false;
                }
            }

            if (!GetBitmapFromImageData(imagedata, out bitmap)) {
                message = "bad image";
                return false;
            }

            var bitmapchanged = false;

            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                bitmap = RepixelBitmap(bitmap);
                bitmapchanged = true;
            }

            var magicformat = GetMagicFormat(imagedata);
            if (magicformat != MagicFormat.Jpeg) {
                bitmapchanged = true;
            }

            if (bitmapchanged) {
                if (!GetImageDataFromBitmap(bitmap, out imagedata)) {
                    message = "encode error";
                    return false;
                }

                File.WriteAllBytes(filename, imagedata);
            }

            hash = Helper.ComputeHash(imagedata);
            return true;
        }

        public static int ComputeRGBHash(int red, int green, int blue)
        {
            return (red << 16) | (green << 8) | blue;
        }

        public static short ComputeLABHash(double l, double a, double b)
        {
            // L 0.0 100.0 = 100.0                  * 0.15 [0 - 15]
            // A -86.18 98.25 = 184.43  186     +87        [0 - 28]
            // B -107.86 94.48 = 202.34 204 0.1569 + 108   [0 - 30] 

            var ol = (int)Math.Round(l * 0.15); // [0..100] -> 0..15 // 5 bit
            var oa = (int)Math.Round((a + 87.0) * 0.15); // [-86..98] -> 0..28 // 5 bit
            var ob = (int)Math.Round((b + 108.0) * 0.15); // [-108..94] -> 0..30 // 5 bit
            return (short)((ol << 10) | (oa << 5) | ob);
        }

        public static void ConvertToLAB(short labhash, out double l, out double a, out double b)
        {
            var ol = (labhash >> 10) & 0x1F;
            var oa = (labhash >> 5) & 0x1F;
            var ob = (labhash >> 0) & 0x1F;
            l = ol / 0.15;
            a = (oa / 0.15) - 87.0;
            b = (ob / 0.15) - 109.0;
        }

        public static void ConvertToLAB(int ir, int ig, int ib, out double dl, out double da, out double db)
        {
            var r = ir / 255.0;
            var g = ig / 255.0;
            var b = ib / 255.0;

            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            var x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            var y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            var z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

            dl = (116.0 * y) - 16.0;
            da = 500.0 * (x - y);
            db = 200.0 * (y - z);
        }

        private static double Deg2rad(double deg)
        {
            return deg * (Math.PI / 180.0);
        }

        public static double EDistance(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            var edistance = Math.Sqrt((l1 - l2) * (l1 - l2)) + ((a1 - a2) * (a1 - a2)) + ((b1 - b2) * (b1 - b2));
            return edistance;
        }

        public static double Distance(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            const double kL = 1.0, kC = 1.0, kH = 1.0;
            const double pow25To7 = 6103515625.0; // pow(25, 7)
            var deg360InRad = Deg2rad(360.0);
            var deg180InRad = Deg2rad(180.0);

            /* Equation 2 */
            double C1 = Math.Sqrt((a1 * a1) + (b1 * b1));
            double C2 = Math.Sqrt((a2 * a2) + (b2 * b2));

            /* Equation 3 */
            double barC = (C1 + C2) / 2.0;

            /* Equation 4 */
            double G = 0.5 * (1.0 - Math.Sqrt(Math.Pow(barC, 7.0) / (Math.Pow(barC, 7.0) + pow25To7)));

            /* Equation 5 */
            double a1Prime = (1.0 + G) * a1;
            double a2Prime = (1.0 + G) * a2;

            /* Equation 6 */
            double CPrime1 = Math.Sqrt((a1Prime * a1Prime) + (b1 * b1));
            double CPrime2 = Math.Sqrt((a2Prime * a2Prime) + (b2 * b2));

            /* Equation 7 */
            double hPrime1;
            if (Math.Abs(b1) < 0.000001 && Math.Abs(a1Prime) < 0.000001) {
                hPrime1 = 0.0;
            }
            else {
                hPrime1 = Math.Atan2(b1, a1Prime);
                /* 
                 * This must be converted to a hue angle in degrees between 0 
                 * and 360 by addition of 20 to negative hue angles.
                 */
                if (hPrime1 < 0) {
                    hPrime1 += deg360InRad;
                }
            }

            double hPrime2;
            if (Math.Abs(b2) < 0.000001 && Math.Abs(a2Prime) < 0.000001) {
                hPrime2 = 0.0;
            }
            else {
                hPrime2 = Math.Atan2(b2, a2Prime);
                /* 
                 * This must be converted to a hue angle in degrees between 0 
                 * and 360 by addition of 2 to negative hue angles.
                 */
                if (hPrime2 < 0) {
                    hPrime2 += deg360InRad;
                }
            }

            /* Equation 8 */
            double deltaLPrime = l2 - l1;

            /* Equation 9 */
            double deltaCPrime = CPrime2 - CPrime1;

            /* Equation 10 */
            double deltahPrime;
            double CPrimeProduct = CPrime1 * CPrime2;
            if (Math.Abs(CPrimeProduct) < 0.000001) {
                deltahPrime = 0.0;
            }
            else {
                /* Avoid the Math.Abs() call */
                deltahPrime = hPrime2 - hPrime1;
                if (deltahPrime < -deg180InRad) {
                    deltahPrime += deg360InRad;
                }
                else {
                    if (deltahPrime > deg180InRad) {
                        deltahPrime -= deg360InRad;
                    }
                }
            }

            /* Equation 11 */
            double deltaHPrime = 2.0 * Math.Sqrt(CPrimeProduct) * Math.Sin(deltahPrime / 2.0);

            /* Equation 12 */
            double barLPrime = (l1 + l2) / 2.0;

            /* Equation 13 */
            double barCPrime = (CPrime1 + CPrime2) / 2.0;

            /* Equation 14 */
            double barhPrime, hPrimeSum = hPrime1 + hPrime2;
            if (Math.Abs(CPrime1 * CPrime2) < 0.000001) {
                barhPrime = hPrimeSum;
            }
            else {
                if (Math.Abs(hPrime1 - hPrime2) <= deg180InRad)
                    barhPrime = hPrimeSum / 2.0;
                else {
                    if (hPrimeSum < deg360InRad) {
                        barhPrime = (hPrimeSum + deg360InRad) / 2.0;
                    }
                    else {
                        barhPrime = (hPrimeSum - deg360InRad) / 2.0;
                    }
                }
            }

            /* Equation 15 */
            double T = 1.0 - (0.17 * Math.Cos(barhPrime - Deg2rad(30f))) +
                (0.24 * Math.Cos(2.0 * barhPrime)) +
                (0.32 * Math.Cos((3.0 * barhPrime) + Deg2rad(6f))) -
                (0.20 * Math.Cos((4.0 * barhPrime) - Deg2rad(63f)));

            /* Equation 16 */
            double deltaTheta = Deg2rad(30f) * Math.Exp(-Math.Pow((barhPrime - Deg2rad(275f)) / Deg2rad(25f), 2.0));

            /* Equation 17 */
            double R_C = 2.0 * Math.Sqrt(Math.Pow(barCPrime, 7.0) / (Math.Pow(barCPrime, 7.0) + pow25To7));

            /* Equation 18 */
            double S_L = 1 + ((0.015 * Math.Pow(barLPrime - 50.0, 2.0)) / Math.Sqrt(20 + Math.Pow(barLPrime - 50.0, 2.0)));

            /* Equation 19 */
            double S_C = 1 + (0.045 * barCPrime);

            /* Equation 20 */
            double S_H = 1 + (0.015 * barCPrime * T);

            /* Equation 21 */
            double R_T = (-Math.Sin(2.0 * deltaTheta)) * R_C;

            /* Equation 22 */
            double deltaE = Math.Sqrt(
                Math.Pow(deltaLPrime / (kL * S_L), 2.0) +
                Math.Pow(deltaCPrime / (kC * S_C), 2.0) +
                Math.Pow(deltaHPrime / (kH * S_H), 2.0) +
                (R_T * (deltaCPrime / (kC * S_C)) * (deltaHPrime / (kH * S_H))));

            return deltaE;
        }

        public static double Distance(int x, int y)
        {
            var pair = (x << 16) | y;
            double distance;
            if (!_lab2distance.TryGetValue(pair, out distance)) {
                ConvertToLAB((short)x, out double l1, out double a1, out double b1);
                ConvertToLAB((short)y, out double l2, out double a2, out double b2);
                distance = EDistance(l1, a1, b1, l2, a2, b2);
                //_lab2distance.Add(pair, distance);
            }

            return distance;
        }

        public static bool ComputeLabs(Bitmap bitmap, out short[] labs)
        {
            Contract.Requires(bitmap != null);
            Contract.Requires(bitmap.PixelFormat == PixelFormat.Format24bppRgb);

            labs = null;
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            IntPtr ptr = bmpdata.Scan0;
            var bytes = bitmap.Width * bitmap.Height * 3;
            byte[] brgs = new byte[bytes];
            Marshal.Copy(ptr, brgs, 0, bytes);
            bitmap.UnlockBits(bmpdata);
            var offset = 0;
            var clusters = new SortedList<short, int>();
            while (offset < brgs.Length) {
                var red = brgs[offset + 2];
                var green = brgs[offset + 1];
                var blue = brgs[offset];
                var rgbhash = ComputeRGBHash(red, green, blue);
                if (!_rgb2lab.TryGetValue(rgbhash, out var labhash)) {
                    ConvertToLAB(red, green, blue, out var l, out var a, out var b);
                    labhash = ComputeLABHash(l, a, b);
                    _rgb2lab.Add(rgbhash, labhash);
                }

                if (clusters.ContainsKey(labhash)) {
                    clusters[labhash]++;
                }
                else {
                    clusters.Add(labhash, 1);
                }
                
                offset += 3;
            }

            var list = new List<ClusterLAB>();
            foreach (var e in clusters) {
                ConvertToLAB(e.Key, out double l, out double a, out double b);
                list.Add(new ClusterLAB(l, a, b, e.Value));
            }

            list.RemoveAll(e => e.D < 10);

            while (list.Count > 16) {
                var mindistance = double.MaxValue;
                var imin = -1;
                var jmin = -1;
                for (var i = 0; i < list.Count - 1; i++) {
                    for (var j = i + 1; j < list.Count; j++) {

                        var distance = EDistance(list[i].L, list[i].A, list[i].B, list[j].L, list[j].A, list[j].B);
                        if (distance < mindistance) {
                            mindistance = distance;
                            imin = i;
                            jmin = j;
                        }
                    }
                }

                if (imin == -1 || jmin == -1) {
                    break;
                }

                var v1 = list[imin].V;
                var v2 = list[jmin].V;
                var v = v1 + v2;
                var l = ((list[imin].L * v1) + (list[jmin].L * v2)) / v;
                var a = ((list[imin].A * v1) + (list[jmin].A * v2)) / v;
                var b = ((list[imin].B * v1) + (list[jmin].B * v2)) / v;
                list.RemoveAt(jmin);
                list.RemoveAt(imin);
                list.Add(new ClusterLAB(l, a, b, v));
            }

            labs = clusters
                .ToArray()
                .OrderByDescending(e => e.Value)
                .Take(16)
                .Select(e => e.Key)
                .ToArray();

            return true;
        }

        public static double Distance(short[] x, short[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Requires(x.Length > 0);
            Contract.Requires(y.Length > 0);
            var sum = 0.0;
            for (var i = 0; i < x.Length; i++) {
                var mindistance = double.MaxValue;
                for (var j = 0; j < y.Length; j++) {
                    var distance = Distance(x[i], y[j]);
                    if (distance < mindistance) {
                        mindistance = distance;
                    }
                }

                sum += mindistance;
            }

            var avg = sum / x.Length;
            return avg;
        }
    }
}
