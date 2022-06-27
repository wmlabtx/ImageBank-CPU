using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace ImageBank
{
    public static class BitmapHelper
    {
        public static System.Windows.Media.ImageSource ImageSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null) {
                return null;
            }

            var handle = bitmap.GetHbitmap();
            try {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally {
                NativeMethods.DeleteObject(handle);
            }
        }

        public static Bitmap ImageDataToBitmap(byte[] imagedata)
        {
            try {
                using (var image = new MagickImage(imagedata)) {
                    var bitmap = image.ToBitmap();
                    if (bitmap.PixelFormat == PixelFormat.Format24bppRgb) {
                        return bitmap;
                    }

                    var bitmap24BppRgb = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                    using (var g = Graphics.FromImage(bitmap24BppRgb)) {
                        g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    }

                    return bitmap24BppRgb;
                }
            }
            catch (MagickMissingDelegateErrorException) {
                return null;
            }
            catch (MagickCorruptImageErrorException) {
                return null;
            }
        }

        public static bool BitmapToImageData(Bitmap bitmap, out byte[] imagedata)
        {
            try {
                var mf = new MagickFactory();
                using (var image = new MagickImage(mf.Image.Create(bitmap))) {
                    image.Format = MagickFormat.WebP;
                    image.Quality = 100;
                    image.Settings.SetDefine(MagickFormat.WebP, "lossless", false);
                    using (var ms = new MemoryStream()) {
                        image.Write(ms);
                        imagedata = ms.ToArray();
                        return true;
                    }
                }
            }
            catch (MagickException) {
                imagedata = null;
                return false;
            }
        }

        public static Bitmap ScaleAndCut(Bitmap bitmap, int dim, int border)
        {
            Bitmap bitmapdim;
            var bigdim = dim + border * 2;
            int width;
            int height;
            if (bitmap.Width >= bitmap.Height) {
                height = bigdim;
                width = (int)Math.Round(bitmap.Width * bigdim / (float)bitmap.Height);
            }
            else {
                width = bigdim;
                height = (int)Math.Round(bitmap.Height * bigdim / (float)bitmap.Width);

            }

            using (Bitmap bitmapbigdim = new Bitmap(width, height, PixelFormat.Format24bppRgb)) {
                using (Graphics g = Graphics.FromImage(bitmapbigdim)) {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bitmap, 0, 0, width, height);
                }

                int x;
                int y;
                if (width >= height) {
                    x = border + (width - height) / 2;
                    y = border;
                }
                else {
                    x = border;
                    y = border + (height - width) / 2;
                }

                //bitmapbigdim.Save("bdim.png", ImageFormat.Png);
                bitmapdim = bitmapbigdim.Clone(new Rectangle(x, y, dim, dim), PixelFormat.Format24bppRgb);
            }

            return bitmapdim;
        }

        public static void ToLAB(byte rbyte, byte gbyte, byte bbyte, out float lfloat, out float afloat, out float bfloat)
        {
            var r = rbyte / 255.0;
            var g = gbyte / 255.0;
            var b = bbyte / 255.0;

            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            var x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            var y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            var z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

            lfloat = (float)((116.0 * y) - 16.0);
            afloat = (float)(500.0 * (x - y));
            bfloat = (float)(200.0 * (y - z));
        }

        public static void ToRGB(float lfloat, float afloat, float bfloat, out byte rbyte, out byte gbyte, out byte bbyte)
        {
            var y = (lfloat + 16.0) / 116.0;
            var x = afloat / 500.0 + y;
            var z = y - bfloat / 200.0;

            x = 0.95047 * ((x * x * x > 0.008856) ? x * x * x : (x - 16.0 / 116.0) / 7.787);
            y = 1.00000 * ((y * y * y > 0.008856) ? y * y * y : (y - 16.0 / 116.0) / 7.787);
            z = 1.08883 * ((z * z * z > 0.008856) ? z * z * z : (z - 16.0 / 116.0) / 7.787);

            var r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            var g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            var b = x * 0.0557 + y * -0.2040 + z * 1.0570;

            r = (r > 0.0031308) ? (1.055 * Math.Pow(r, 1.0 / 2.4) - 0.055) : 12.92 * r;
            g = (g > 0.0031308) ? (1.055 * Math.Pow(g, 1.0 / 2.4) - 0.055) : 12.92 * g;
            b = (b > 0.0031308) ? (1.055 * Math.Pow(b, 1.0 / 2.4) - 0.055) : 12.92 * b;

            rbyte = (byte)(Math.Max(0, Math.Min(1.0, r)) * 255);
            gbyte = (byte)(Math.Max(0, Math.Min(1.0, g)) * 255);
            bbyte = (byte)(Math.Max(0, Math.Min(1.0, b)) * 255);
        }
    }
}
