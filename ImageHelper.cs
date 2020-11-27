using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImageBank
{
    public static class ImageHelper
    {
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

        public static Bitmap ResizeBitmap(Bitmap bitmap, int width, int height)
        {
            Contract.Requires(bitmap != null);
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(bitmap, destRect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
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

        public static bool ComputeDescriptors(Image image, out byte[] histogram)
        {
            histogram = null;
            Contract.Requires(image != null);
            Contract.Requires(image.PixelFormat == PixelFormat.Format24bppRgb);

            const int dim = 256;
            byte[] brgs;
            using (var bitmap = ResizeBitmap((Bitmap)image, dim, dim)) {
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData bmpdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                IntPtr ptr = bmpdata.Scan0;
                var bytes = bitmap.Width * bitmap.Height * 3;
                brgs = new byte[bytes];
                Marshal.Copy(ptr, brgs, 0, bytes);
                bitmap.UnlockBits(bmpdata);
            }

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
            var colorhistogram = new ColorDescriptor[dim, dim];
            var M = new byte[dim, dim];
            var dim2 = dim / 2;
            var V = new double[dim2, dim2];
            var S = new double[dim2, dim2];
            var E = new byte[dim2, dim2];
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional

            var offset = 0;
            for (var y = 0; y < dim; y++) {
                for (var x = 0; x < dim; x++) {
                    var blue = brgs[offset++];
                    var green = brgs[offset++];
                    var red = brgs[offset++];
                    colorhistogram[x, y] = new ColorDescriptor(red, green, blue);
                    M[x, y] = colorhistogram[x, y].Index;
                }
            }

            for (var y = 0; y < dim; y += 2) {
                for (var x = 0; x < dim; x += 2) {
                    V[x / 2, y / 2] = (
                        colorhistogram[x, y].V +
                        colorhistogram[x + 1, y].V +
                        colorhistogram[x, y + 1].V +
                        colorhistogram[x + 1, y + 1].V) / 4.0;
                }
            }

            /*
            using (var bm = new Bitmap(dim2, dim2, PixelFormat.Format24bppRgb)) {
                for (var y = 0; y < dim2; y++) {
                    for (var x = 0; x < dim2; x++) {
                        var l = (int)(V[x, y] * 255.0);
                        Debug.Assert(l <= 255);
                        bm.SetPixel(x, y, Color.FromArgb(l, l, l));
                    }
                }

                bm.Save("bm-v.png", ImageFormat.Png);
            }
            */

            for (var y = 1; y < dim2 - 1; y++) {
                for (var x = 1; x < dim2 - 1; x++) {
                    var gx =
                        -V[x - 1, y - 1] - 2.0 * V[x - 1, y] - V[x - 1, y + 1] +
                         V[x + 1, y - 1] + 2.0 * V[x + 1, y] + V[x + 1, y + 1];

                    var gy =
                        -V[x - 1, y - 1] - 2.0 * V[x, y - 1] - V[x + 1, y - 1] +
                         V[x + 1, y + 1] + 2.0 * V[x + 1, y + 1] + V[x + 1, y + 1];

                    S[x, y] = Math.Sqrt((gx * gx) + (gy * gy));
                    if (S[x, y] < 0.0) {
                        S[x, y] = 0.0;
                    }

                    if (S[x, y] > 1.0) {
                        S[x, y] = 1.0;
                    }
                }
            }

            /*
            using (var bm = new Bitmap(dim2, dim2, PixelFormat.Format24bppRgb)) {
                for (var y = 0; y < dim2; y++) {
                    for (var x = 0; x < dim2; x++) {
                        var l = (int)(S[x, y] * 255.0);
                        Debug.Assert(l <= 255);
                        bm.SetPixel(x, y, Color.FromArgb(l, l, l));
                    }
                }

                bm.Save("bm-s.png", ImageFormat.Png);
            }
            */

            for (var y = 1; y < dim2 - 1; y++) {
                for (var x = 1; x < dim2 - 1; x++) {
                    E[x, y] = (byte)(S[x, y] * 15.0);
                }
            }

            /*
            using (var bm = new Bitmap(dim2, dim2, PixelFormat.Format24bppRgb)) {
                for (var y = 0; y < dim2; y++) {
                    for (var x = 0; x < dim2; x++) {
                        var l = (int)(E[x, y] * 16.0);
                        Debug.Assert(l <= 255);
                        bm.SetPixel(x, y, Color.FromArgb(l, l, l));
                    }
                }

                bm.Save("bm-e.png", ImageFormat.Png);
            }
            */

            var dx = new int[] { 1, 1, 0 };
            var dy = new int[] { 0, 1, 1 };
            var P0 = new int[54];
            for (var x = 0; x < dim - 1; x++) {
                for (var y = 0; y < dim - 1; y++) {
                    for (var i = 0; i < 3; i++) {
                        if (M[x, y] == M[x + dx[i], y + dy[i]]) {
                            P0[M[x, y]]++;
                        }
                    }
                }
            }

            var P1 = new int[16];
            for (var x = 1; x < dim2 - 2; x++) {
                for (var y = 1; y < dim2 - 2; y++) {
                    for (var i = 0; i < 3; i++) {
                        if (E[x, y] == E[x + dx[i], y + dy[i]]) {
                            P1[E[x, y]]++;
                        }
                    }
                }
            }

            histogram = new byte[54 + 16];
            for (var i = 0; i < 54; i++) {
                var log = Math.Log(P0[i]);
                histogram[i] = log < 0.0 ? (byte)0 : (byte)log;
            }

            for (var i = 0; i < 16; i++) {
                var log = Math.Log(P1[i]);
                histogram[i + 54] = log < 0.0 ? (byte)0 : (byte)log;
            }

            return true;
        }

        public static float Distance(byte[] hx, byte[] hy)
        {
            Contract.Requires(hx != null);
            Contract.Requires(hy != null);
            Contract.Requires(hx.Length > 0);
            Contract.Requires(hy.Length > 0);
            Contract.Requires(hx.Length == hy.Length);

            var m = 0f;
            var a = 0f;
            var b = 0f;
            for (var i = 0; i < hx.Length; i++) {
                if (Math.Abs(hx[i]) > 0.0001) {
                    m += hx[i] * hy[i];
                    a += hx[i] * hx[i];
                    b += hy[i] * hy[i];
                }
            }

            var distance = (float)(Math.Acos(m / (Math.Sqrt(a) * Math.Sqrt(b))) / Math.PI);
            return distance;
        }
    }
}
