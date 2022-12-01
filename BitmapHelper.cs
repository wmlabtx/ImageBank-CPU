﻿using ImageMagick;
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
    }
}
