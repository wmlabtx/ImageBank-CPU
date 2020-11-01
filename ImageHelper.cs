using System;
using System.Collections.Generic;
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

        public static bool ComputeDescriptors(Image image, out short[] descriptors)
        {
            Contract.Requires(image != null);
            Contract.Requires(image.PixelFormat == PixelFormat.Format24bppRgb);

            descriptors = null;
            const int dim = 64;
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
            var colordescriptors = new ColorDescriptor[dim, dim];
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional

            var offset = 0;
            for (var y = 0; y < dim; y++) {
                for (var x = 0; x < dim; x++) {
                    var blue = brgs[offset++];
                    var green = brgs[offset++];
                    var red = brgs[offset++];
                    colordescriptors[x, y] = new ColorDescriptor(red, green, blue);
                }
            }

            /*
            using (var bm = new Bitmap(dim, dim, PixelFormat.Format24bppRgb)) {
                for (var y = 0; y < dim; y++) {
                    for (var x = 0; x < dim; x++) {
                        bm.SetPixel(x, y, Color.FromArgb(colordescriptors[x, y].Red, colordescriptors[x, y].Green, colordescriptors[x, y].Blue));
                    }
                }

                bm.Save("bm0.png", ImageFormat.Png);
            }
            */

            var listhashes = new SortedList<short, object>();
            for (var y = 0; y < dim; y++) {
                for (var x = 0; x < dim; x++) {
                    if (!listhashes.ContainsKey(colordescriptors[x, y].H)) {
                        listhashes.Add(colordescriptors[x, y].H, null);
                    }
                }
            }

            const int MaxClusters = 4000;
            descriptors = listhashes
                .ToList()
                .Take(MaxClusters)
                .OrderBy(e => e.Key)
                .Select(e => e.Key)
                .ToArray();

            return true;
        }

        public static short[] BufferToDescriptors(byte[] buffer)
        {
            Contract.Requires(buffer != null);
            var descriptors = new short[buffer.Length / sizeof(short)];
            Buffer.BlockCopy(buffer, 0, descriptors, 0, buffer.Length);
            return descriptors;
        }

        public static byte[] DescriptorsToBuffer(short[] descriptors)
        {
            Contract.Requires(descriptors != null);
            var buffer = new byte[descriptors.Length * sizeof(short)];
            Buffer.BlockCopy(descriptors, 0, buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
