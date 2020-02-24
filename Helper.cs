using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageBank
{
    public static class Helper
    {
        #region DeleteToRecycleBin

        public static void DeleteToRecycleBin(string filename)
        {
            try {
                if (File.Exists(filename)) {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    FileSystem.DeleteFile(filename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }
            catch (UnauthorizedAccessException) {
            }
            catch (IOException) {
            }
        }

        #endregion

        #region Hash

        public static string ComputeHash3250(byte[] array)
        {
            var digits = "abcdefghijklmnopqrstuvwxyz234567".ToCharArray();
            if (digits.Length != 32) {
                throw new IndexOutOfRangeException();
            }

            var sb = new StringBuilder(50);
            using (var sha256 = SHA256.Create()) {
                var hash = sha256.ComputeHash(array);
                var index = 0;
                var posbyte = hash.Length - 1;
                var posbit = 0;
                for (var poschar = 0; poschar < 50; poschar++) {
                    index = 0;
                    for (var i = 0; i < 5; i++) {
                        if ((hash[posbyte] & (1 << posbit)) != 0) {
                            index |= 1 << i;
                        }

                        posbit++;
                        if (posbit >= 8) {
                            posbyte--;
                            posbit = 0;
                        }
                    }

                    sb.Append(digits[index]);
                }
            }

            return sb.ToString();
        }

        #endregion

        #region TimeIntervalToString
        public static string TimeIntervalToString(TimeSpan ts)
        {
            if (ts.TotalDays >= 2.0)
                return $"{ts.TotalDays:F0} days";

            if (ts.TotalDays >= 1.0)
                return $"{ts.TotalDays:F0} day";

            if (ts.TotalHours >= 2.0)
                return $"{ts.TotalHours:F0} hours";

            if (ts.TotalHours >= 1.0)
                return $"{ts.TotalHours:F0} hour";

            if (ts.TotalMinutes >= 2.0)
                return $"{ts.TotalMinutes:F0} minutes";

            if (ts.TotalMinutes >= 1.0)
                return $"{ts.TotalMinutes:F0} minute";

            return $"{ts.TotalSeconds:F0} seconds";
        }

        #endregion

        #region SizeToString

        public static string SizeToString(long size)
        {
            var str = $"{size} b";
            if (size < 1024)
                return str;

            var ksize = (double)size / 1024;
            str = $"{ksize:F1} Kb";
            if (ksize < 1024)
                return str;

            ksize /= 1024;
            str = $"{ksize:F2} Mb";
            return str;
        }

        #endregion

        #region Strings

        public static string GetFileName(string name, string path)
        {
            var filename = $"{AppConsts.PathCollection}{path}\\{name}{AppConsts.JpgExtension}";
            return filename;
        }

        public static string GetName(string filename)
        {
            var name = Path.GetFileNameWithoutExtension(filename);
            return name;
        }

        public static string GetPath(string filename)
        {
            var path = Path
                .GetDirectoryName(filename)
                .Substring(AppConsts.PathCollection.Length);

            return path;
        }

        public static string GetExtension(string filename)
        {
            var name = Path.GetExtension(filename);
            return name;
        }

        #endregion

        #region Image

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

        public static byte[] ReencodeBitmap(Bitmap bitmap)
        {
            Contract.Requires(bitmap != null);
            var myImageCodecInfo = GetEncoderInfo("image/jpeg");
            using (var parameters = new EncoderParameters(2)) {
                var quality = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                var colorDepth = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 24L);
                parameters.Param[0] = quality;
                parameters.Param[1] = colorDepth;
                using (var ms = new MemoryStream()) {
                    bitmap.Save(ms, myImageCodecInfo, parameters);
                    var imgdata = ms.ToArray();
                    return imgdata;
                }
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

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (var j = 0; j < encoders.Length; j++) {
                if (encoders[j].MimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase)) {
                    return encoders[j];
                }
            }

            return null;
        }

        public static bool GetImageDataFromFile(
            string filename,
            out byte[] imgdata,
            out Bitmap bitmap,
            out string checksum,
            out bool needwrite)
        {
            imgdata = null;
            bitmap = null;
            checksum = null;
            needwrite = false;
            if (!File.Exists(filename)) {
                return false;
            }

            var extension = GetExtension(filename);
            if (string.IsNullOrEmpty(extension)) {
                return false;
            }

            if (
                !extension.Equals(AppConsts.PngExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.BmpExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.WebpExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpgExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpegExtension, StringComparison.OrdinalIgnoreCase)
                ) {
                return false;
            }

            imgdata = File.ReadAllBytes(filename);
            if (imgdata == null || imgdata.Length == 0) {
                return false;
            }

            using (var ms = new MemoryStream(imgdata)) {
                try {
                    var image = Image.FromStream(ms);
                    bitmap = (Bitmap)image;

                    if (!extension.Equals(AppConsts.JpgExtension, StringComparison.OrdinalIgnoreCase) &&
                        !extension.Equals(AppConsts.JpegExtension, StringComparison.OrdinalIgnoreCase)) { 
                        needwrite = true;
                    }

                    const float fmax = 6000f * 4000f;
                    var fx = (float)Math.Sqrt(fmax / (image.Width * image.Height));
                    if (fx < 1f) {
                        bitmap = ResizeBitmap(bitmap, (int)(image.Width * fx), (int)(image.Height * fx));
                        needwrite = true;
                    }

                    if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                        bitmap = RepixelBitmap(bitmap);
                        needwrite = true;
                    }

                    if (needwrite) {
                        imgdata = ReencodeBitmap(bitmap);
                    }

                    checksum = ComputeHash3250(imgdata);
                }
                catch {
                    imgdata = null;
                    bitmap = null;
                    checksum = null;
                    needwrite = false;
                    return false;
                }
            }

            return true;
        }

        public static ImageSource ImageSourceFromBitmap(Bitmap bitmap)
        {
            Contract.Requires(bitmap != null);
            var handle = bitmap.GetHbitmap();
            try {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally {
                NativeMethods.DeleteObject(handle);
            }
        }

        #endregion

        #region CleanupDirectories
        public static void CleanupDirectories(string startLocation, IProgress<string> progress)
        {
            Contract.Requires(progress != null);
            foreach (var directory in Directory.GetDirectories(startLocation)) {
                Helper.CleanupDirectories(directory, progress);
                if (Directory.GetFiles(directory).Length != 0 || Directory.GetDirectories(directory).Length != 0) {
                    continue;
                }

                progress.Report($"{directory} deleting...");
                try {
                    Directory.Delete(directory, false);
                }
                catch (IOException) {
                }
            }
        }

        #endregion

        #region EncryptedData

        public static byte[] ReadData(string filename)
        {
            if (!File.Exists(filename)) {
                return null;
            }

            var imgdata = File.ReadAllBytes(filename);
            if (imgdata == null || imgdata.Length == 0) {
                return null;
            }

            return imgdata;
        }

        public static void WriteData(string filename, byte[] imgdata)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filename, imgdata);
        }

        #endregion

        #region Vectors

        public static bool GetVector(Bitmap bitmap, out float[] vector)
        {
            Contract.Requires(bitmap != null);
            vector = null;
            if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                return false;
            }

            vector = HelperMl.GetVector(bitmap);
            if (vector == null || vector.Length != 4032) {
                return false;
            }

            return true;
        }

        public static float VectorDistance(float[] x, float[] y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);
            Contract.Requires(x.Length > 0);
            Contract.Requires(y.Length > 0);
            Contract.Requires(x.Length == y.Length);
            var m = 0f;
            var a = 0f;
            var b = 0f;
            for (var i = 0; i < x.Length; i++) {
                if (Math.Abs(x[i]) > 0.0001) {
                    m += x[i] * y[i];
                    a += x[i] * x[i];
                    b += y[i] * y[i];
                }
            }

            var distance = (float)(Math.Acos(m / (Math.Sqrt(a) * Math.Sqrt(b))) / Math.PI);
            return distance;
        }

        private static byte[] PackArray(byte[] writeData)
        {
            byte[] buffer;
            using (var inner = new MemoryStream()) {
                using (var stream2 = new GZipStream(inner, CompressionMode.Compress)) {
                    stream2.Write(writeData, 0, writeData.Length);
                }

                buffer = inner.ToArray();
            }

            return buffer;
        }

        private static byte[] UnpackArray(byte[] compressedData)
        {
            using (var inner = new MemoryStream(compressedData)) {
                using (var stream2 = new MemoryStream()) {
                    using (var stream3 = new GZipStream(inner, CompressionMode.Decompress)) {
                        var buffer = new byte[1024 * 16];
                        int count;
                        while ((count = stream3.Read(buffer, 0, buffer.Length)) > 0) {
                            stream2.Write(buffer, 0, count);
                        }
                    }

                    return stream2.ToArray();
                }
            }
        }

        public static byte[] VectorToBuffer(float[] vector)
        {
            Contract.Requires(vector != null);
            var buffer = new byte[vector.Length * sizeof(float)];
            Buffer.BlockCopy(vector, 0, buffer, 0, buffer.Length);
            var packbuffer = PackArray(buffer);
            return packbuffer;
        }

        public static float[] BufferToVector(byte[] buffer)
        {
            Contract.Requires(buffer != null);
            var unpackbuffer = UnpackArray(buffer);
            var vector = new float[unpackbuffer.Length / sizeof(float)];
            Buffer.BlockCopy(unpackbuffer, 0, vector, 0, unpackbuffer.Length);
            return vector;
        }

        #endregion
    }
}
