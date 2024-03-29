﻿using ImageMagick;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

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
                var image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                image.Freeze();
                return image;
            }
            finally {
                NativeMethods.DeleteObject(handle);
            }
        }

        public static MagickImage ImageDataToMagickImage(byte[] imagedata)
        {
            MagickImage magickImage = null;
            try {
                magickImage = new MagickImage(imagedata);
            }
            catch (MagickMissingDelegateErrorException) {
                magickImage = null;
            }
            catch (MagickCorruptImageErrorException) {
                magickImage = null;
            }

            return magickImage;
        }

        public static Bitmap MagickImageToBitmap(MagickImage magickImage, RotateFlipType rft)
        {
            var bitmap = magickImage.ToBitmap();
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb) {
                bitmap.RotateFlip(rft);
                return bitmap;
            }

            var bitmap24BppRgb = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap24BppRgb)) {
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }

            bitmap24BppRgb.RotateFlip(rft);
            return bitmap24BppRgb;
        }

        public static string GetRecommendedExt(MagickImage image)
        {
            switch (image.Format) {
                case MagickFormat.Jpeg:
                    return ".jpg";
                case MagickFormat.Png:
                    return ".png";
                case MagickFormat.Bmp:
                    return ".bmp";
                case MagickFormat.Gif:
                    return ".gif";
                case MagickFormat.WebP:
                    return ".webp";
                case MagickFormat.Heic:
                    return ".heic";
                default:
                    throw new Exception($"Unkown extension for {image.Format}");
            }
        }

        public static bool BitmapToImageData(Bitmap bitmap, MagickFormat magickFormat, out byte[] imagedata)
        {
            try {
                var mf = new MagickFactory();
                using (var image = new MagickImage(mf.Image.Create(bitmap))) {
                    image.Format = magickFormat;
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

        public static DateTime GetDateTaken(MagickImage magickImage, DateTime defaultValue)
        {
            var exif = magickImage.GetExifProfile();
            if (exif == null) {
                return defaultValue;
            }

            var possibleExifTags = new ExifTag<string>[] { ExifTag.DateTimeOriginal, ExifTag.DateTimeDigitized, ExifTag.DateTime };
            foreach (var tag in possibleExifTags) {
                var field = exif.GetValue(tag);
                if (field == null) {
                    continue;
                }

                var value = field.ToString();
                if (!DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)) {
                    continue;
                }

                return dt;
            }

            return defaultValue;
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

                bitmapdim = bitmapbigdim.Clone(new Rectangle(x, y, dim, dim), PixelFormat.Format24bppRgb);
            }

            return bitmapdim;
        }

        public static Bitmap BitmapXor(Bitmap xb, Bitmap yb)
        {
            BitmapData xd = xb.LockBits(new Rectangle(0, 0, xb.Width, xb.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] xa = new byte[xd.Stride * xd.Height];
            Marshal.Copy(xd.Scan0, xa, 0, xa.Length);
            xb.UnlockBits(xd);

            BitmapData yd = yb.LockBits(new Rectangle(0, 0, yb.Width, yb.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] ya = new byte[yd.Stride * yd.Height];
            Marshal.Copy(yd.Scan0, ya, 0, ya.Length);
            yb.UnlockBits(yd);

            for (var i = 0; i < xa.Length - 2; i += 3) {
                ya[i] = (byte)Math.Min(255, Math.Abs(xa[i] - ya[i]) << 3);
                ya[i + 1] = (byte)Math.Min(255, Math.Abs(xa[i + 1] - ya[i + 1]) << 3);
                ya[i + 2] = (byte)Math.Min(255, Math.Abs(xa[i + 2] - ya[i + 2]) << 3);
                if (ya[i] == 255 && ya[i + 1] == 255 && ya[i + 2] == 255) {
                    Bitmap cb = new Bitmap(yb);
                    return cb;
                }
            }

            Bitmap zb = new Bitmap(yb);
            BitmapData zd = zb.LockBits(new Rectangle(0, 0, zb.Width, zb.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(ya, 0, zd.Scan0, ya.Length);
            zb.UnlockBits(zd);

            return zb;
        }

        public static double GetBlur(Bitmap x)
        {
            double result = 0.0;
            using (var bitmap = ScaleAndCut(x, 512, 16))
            using (var cvcolor = BitmapConverter.ToMat(bitmap))
            using (var cvgray = new Mat())
            using (var lap = new Mat())
            using (var mu = new Mat())
            using (var sigma = new Mat()) {
                Cv2.CvtColor(BitmapConverter.ToMat(x), cvgray, ColorConversionCodes.BGR2GRAY);
                Cv2.Laplacian(cvgray, lap, MatType.CV_64F);
                Cv2.MeanStdDev(cvgray, mu, sigma);
                sigma.GetArray(out double[] array);
                result = array[0] * array[0];
            }

            return result;
        }

        private static double EotfPq(double val)
        {
            const double m1 = 1305.0 / 8192.0;
            const double m2 = 2523.0 / 32.0;
            const double c1 = 107.0 / 128.0;
            const double c2 = 2413.0 / 128.0;
            const double c3 = 2392.0 / 128.0;
            var ym1 = Math.Pow(val, m1);
            var e = Math.Pow((c1 + c2 * ym1) / (1.0 + c3 * ym1), m2);
            return e;
        }

        public static void RGB2ITP(int rb, int gb, int bb, out double id, out double td, out double pd)
        {
            var rd = rb / 255.0;
            var gd = gb / 255.0;
            var bd = bb / 255.0;
            var ld = (1688.0 * rd + 2146.0 * gd + 262.0 * bd) / 4096.0;
            var md = (683.0 * rd + 2951.0 * gd + 462.0 * bd) / 4096.0;
            var sd = (99.0 * rd + 309.0 * gd + 3688.0 * bd) / 4096.0;
            var ld1 = EotfPq(ld);
            var md1 = EotfPq(md);
            var sd1 = EotfPq(sd);
            id = (2048.0 * ld1 + 2048.0 * md1) / 4096.0;
            td  = 0.5 * (6610.0 * ld1 - 13613.0 * md1 + 7003.0 * sd1) / 4096.0 + 0.5;
            pd = (17933.0 * ld1 - 17390.0 * md1 - 543.0 * sd1) / 4096.0 + 0.5;
        }
    }
}
