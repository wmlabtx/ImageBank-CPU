using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
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
                using (MagickImage image = new MagickImage(imagedata)) {
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
        }

        public static byte[] BitmapToImageData(Bitmap bitmap)
        {
            try {
                var mf = new MagickFactory();
                using (var image = new MagickImage(mf.Image.Create(bitmap))) {
                    image.Format = MagickFormat.Jxl;
                    image.Quality = 100;
                    image.Settings.SetDefine(MagickFormat.WebP, "lossless", false);
                    using (var ms = new MemoryStream()) {
                        image.Write(ms);
                        var imagedata = ms.ToArray();
                        return imagedata;
                    }
                }
            }
            catch (MagickException) {
                return null;
            }
        }

        public static float GetBrightness(byte red, byte green, byte blue)
        {
            var r = red / 255.0;
            var g = green / 255.0;
            var b = blue / 255.0;
            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;
            var y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
            var l = (float)((116.0 * y) - 16.0);
            return l;
        }

        public static float[][] GetMatrix(byte[] imagedata)
        {
            int width;
            int height;
            int stride;
            byte[] data;
            using (var bitmap = ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
                    return null;
                }

                width = bitmap.Width;
                height = bitmap.Height;
                BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                stride = bitmapdata.Stride;
                data = new byte[Math.Abs(bitmapdata.Stride * bitmapdata.Height)];
                Marshal.Copy(bitmapdata.Scan0, data, 0, data.Length);
                bitmap.UnlockBits(bitmapdata);
            }

            var matrix = new float[height][];
            var offsety = 0;
            for (var y = 0; y < height; y++) {
                matrix[y] = new float[width];
                var offsetx = offsety;
                for (var x = 0; x < width; x++) {
                    var r = data[offsetx + 2];
                    var g = data[offsetx + 1];
                    var b = data[offsetx];
                    var l = GetBrightness(r, g, b);
                    matrix[y][x] = l;
                    offsetx += 3;
                }

                offsety += stride;
            }

            return matrix;
        }
    }
}
