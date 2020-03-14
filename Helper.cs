using ImageMagick;
using Microsoft.VisualBasic.FileIO;
using OpenCvSharp;
using System;
using System.Diagnostics;
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

        #region Base36

        public static string ComputeBase36(int value)
        {
            var digits = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
            if (digits.Length != 36) {
                throw new IndexOutOfRangeException();
            }

            var sb = new StringBuilder();
            do {
                sb.Append(digits[value % digits.Length]);
                value /= digits.Length;
            }
            while (value != 0);

            return sb.ToString();
        }

        public static int DecodeBase36(string value)
        {
            Contract.Requires(value != null);
            var digits = "0123456789abcdefghijklmnopqrstuvwxyz";
            int decoded = digits.IndexOf(value[0]);
            if (decoded < 0) {
                return 0;
            }

            var k = digits.Length;
            for (var i = 1; i < value.Length; ++i) {
                var digit = digits.IndexOf(value[i]);
                if (digit < 0) {
                    return 0;
                }

                decoded += digit * k;
                k *= digits.Length;
            }

            return decoded;
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

        public static string GetName(int id)
        {
            return string.Concat(AppConsts.Prefix, ComputeBase36(id));
        }

        public static string GetFolder(int id)
        {
            var ifolder = id % 100;
            return $"{ifolder:D2}";
        }

        public static string GetFileName(string name, string folder)
        {
            return $"{AppConsts.PathCollection}{folder}\\{name}{AppConsts.MzxExtension}";
        }

        public static int GetId(string filename)
        {
            var name = Path.GetFileNameWithoutExtension(filename);
            if (name.Length < AppConsts.Prefix.Length) {
                return 0;
            }

            if (!name.StartsWith(AppConsts.Prefix, StringComparison.OrdinalIgnoreCase)) {
                return 0;
            }

            name = name.Substring(AppConsts.Prefix.Length);
            if (name.Length == 0) {
                return 0;
            }

            return DecodeBase36(name);
        }

        public static bool IsNativePath(string filename)
        {
            var fullpath = Path.GetDirectoryName(filename);
            if (fullpath.Length < AppConsts.PathCollection.Length) {
                return false;
            }

            var path = fullpath.Substring(AppConsts.PathCollection.Length);
            if (path.Length == 0) {
                return false;
            }

            if (!int.TryParse(path, out var id)) {
                return false;
            }

            if (id < 0 || id > 99) {
                return false;
            }

            return true;
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

        public static Bitmap RepixelBitmap(Bitmap bitmap)
        {
            Contract.Requires(bitmap != null);
            var bitmap24bppRgb = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap24bppRgb)) {
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }

            return bitmap24bppRgb;
        }

        public static bool GetBitmapFromImgData(byte[] imgdata, out Bitmap bitmap)
        {
            try {
                using (var image = new MagickImage(imgdata)) {
                    bitmap = image.ToBitmap();
                    return true;
                }
            }
            catch (MagickException) {
                bitmap = null;
                return false;
            }
        }

        public static string GetHashFromBitmap(Bitmap bitmap)
        {
            var imageconverter = new ImageConverter();
            var array = (byte[])imageconverter.ConvertTo(bitmap, typeof(byte[]));
            for (var i = 0; i < array.Length; i++) {
                array[i] >>= 4;
            }

            return ComputeHash3250(array);
        }

        public static Bitmap GetThumpFromBitmap(Bitmap bitmap)
        {
            Contract.Requires(bitmap != null);
            int width;
            int heigth;
            if (bitmap.Width > bitmap.Height) {
                heigth = 256;
                width = (int)(bitmap.Width * 256f / bitmap.Height);
            }
            else {
                width = 256;
                heigth = (int)(bitmap.Height * 256f / bitmap.Width);
            }

            return ResizeBitmap(bitmap, width, heigth);
        }

        public static bool GetImgDataFromBitmap(Bitmap bitmap, out byte[] imgdata)
        {
            try {
                using (var image = new MagickImage(bitmap)) {
                    image.Format = MagickFormat.Jpeg;
                    image.Quality = 95;
                    using (var ms = new MemoryStream()) {
                        image.Write(ms);
                        imgdata = ms.ToArray();
                        return true;
                    }
                }
            }
            catch (MagickException) {
                imgdata = null;
                return false;
            }
        }

        public static bool GetFlifFromBitmap(Bitmap bitmap, out byte[] imgdata)
        {
            try {
                using (var image = new MagickImage(bitmap)) {
                    image.Format = MagickFormat.Flif;
                    using (var ms = new MemoryStream()) {
                        image.Write(ms);
                        imgdata = ms.ToArray();
                        return true;
                    }
                }
            }
            catch (MagickException) {
                imgdata = null;
                return false;
            }
        }

        public static bool GetPerceptualHash(Bitmap bitmap, out string phash)
        {
            try {
                using (var image = new MagickImage(bitmap)) {
                    var perceptualhash = image.PerceptualHash();
                    phash = perceptualhash.ToString();
                    return true;
                }
            }
            catch (MagickException) {
                phash = null;
                return false;
            }
        }

        public static double GetPerceptualHashDistance(string x, string y)
        {
            var px = new PerceptualHash(x);
            var py = new PerceptualHash(y);
            return px.SumSquaredDistance(py);
        }

        public static bool GetImageDataFromFile(
            string filename,
            out byte[] imgdata,
            out Bitmap bitmap,
            out string checksum,
            out string message)
        {
            imgdata = null;
            bitmap = null;
            checksum = null;
            message = null;
            if (!File.Exists(filename)) {
                message = "missing file";
                return false;
            }

            var extension = GetExtension(filename);
            if (string.IsNullOrEmpty(extension)) {
                message = "no extention";
                return false;
            }

            if (
                !extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.PngExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.BmpExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.WebpExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpgExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpegExtension, StringComparison.OrdinalIgnoreCase)
                ) {
                message = "unknown extention";
                return false;
            }

            imgdata = File.ReadAllBytes(filename);
            if (imgdata == null || imgdata.Length == 0) {
                message = "imgdata == null || imgdata.Length == 0";
                return false;
            }

            if (extension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase)) {
                var password = Path.GetFileNameWithoutExtension(filename);
                imgdata = DecryptDat(imgdata, password);
                if (imgdata == null) {
                    message = "cannot be decrypted";
                    return false;
                }
            }

            if (extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                var password = Path.GetFileNameWithoutExtension(filename);
                imgdata = Decrypt(imgdata, password);
                if (imgdata == null) {
                    message = "cannot be decrypted";
                    return false;
                }
            }

            if (!GetBitmapFromImgData(imgdata, out bitmap)) {
                message = "bad image";
                return false;
            }

            var bitmapchanged = false;
            const float fmax = 6000f * 4000f;
            var fx = (float)Math.Sqrt(fmax / (bitmap.Width * bitmap.Height));
            if (fx < 1f) {
                bitmap = ResizeBitmap(bitmap, (int)(bitmap.Width * fx), (int)(bitmap.Height * fx));
                bitmapchanged = true;
            }

            if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                bitmap = RepixelBitmap(bitmap);
                bitmapchanged = true;
            }

            if (bitmapchanged) {
                if (!GetImgDataFromBitmap(bitmap, out imgdata)) {
                    message = "encode error";
                    return false;
                }
            }

            checksum = ComputeHash3250(imgdata);
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
            try {
                var encdata = File.ReadAllBytes(filename);
                var password = Path.GetFileNameWithoutExtension(filename);
                var imgdata = Decrypt(encdata, password);
                return imgdata;
            }
            catch (DirectoryNotFoundException ex) {
                Trace.WriteLine(ex);
                return null;
            }
            catch (FileNotFoundException ex) {
                Trace.WriteLine(ex);
                return null;
            }
            catch (Exception ex) {
                Trace.WriteLine(ex);
                throw;
            }
        }

        public static void WriteData(string filename, byte[] imgdata)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var password = Path.GetFileNameWithoutExtension(filename);
            var encdata = Encrypt(imgdata, password);
            File.WriteAllBytes(filename, encdata);
        }

        #endregion

        #region Pack

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

        #endregion

        #region Encryption

        private const string PasswordSole = "{mzx}";
        private static readonly byte[] AES_IV = {
            0xE1, 0xD9, 0x94, 0xE6, 0xE6, 0x43, 0x39, 0x34,
            0x33, 0x0A, 0xCC, 0x9E, 0x7D, 0x66, 0x97, 0x16
        };

        private static Aes CreateAes(string password)
        {
            using (var hash256 = SHA256.Create()) {
                var passwordwithsole = string.Concat(password, PasswordSole);
                var passwordbuffer = Encoding.ASCII.GetBytes(passwordwithsole);
                var passwordkey256 = hash256.ComputeHash(passwordbuffer);
                var aes = Aes.Create();
                aes.KeySize = 256;
                aes.Key = passwordkey256;
                aes.BlockSize = 128;
                aes.IV = AES_IV;
                aes.Mode = CipherMode.CBC;
                return aes;
            }
        }

        public static byte[] Encrypt(byte[] array, string password)
        {
            Contract.Requires(array != null);
            Contract.Requires(password != null);
            var aes = CreateAes(password);
            try {
                using (var ms = new MemoryStream()) {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                        cs.Write(array, 0, array.Length);
                    }

                    return ms.ToArray();
                }
            }
            finally {
                aes.Dispose();
            }
        }

        public static byte[] Decrypt(byte[] array, string password)
        {
            Contract.Requires(array != null);
            Contract.Requires(password != null);
            var aes = CreateAes(password);
            try {

                try {
                    using (var ms = new MemoryStream(array)) {
                        byte[] decoded;
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read)) {
                            var count = cs.Read(array, 0, array.Length);
                            decoded = new byte[count];
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.Read(decoded, 0, count);
                        }

                        return decoded;
                    }
                }
                catch (CryptographicException ex) {
                    Trace.WriteLine(ex);
                    return null;
                }
            }
            finally {
                aes.Dispose();
            }
        }

        private static readonly byte[] SaltBytes = { 0xFF, 0x15, 0x20, 0xD5, 0x24, 0x1E, 0x12, 0xAA, 0xCC, 0xFF };
        private const int Interations = 1000;

        public static byte[] DecryptDat(byte[] bytesToBeDecrypted, string password)
        {
            Contract.Requires(bytesToBeDecrypted != null);
            byte[] decryptedBytes = null;

            try {
                using (var ms = new MemoryStream())
                using (var aes = new RijndaelManaged()) {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    var passwordBytes = Encoding.ASCII.GetBytes(password);
#pragma warning disable CA5379 // Do Not Use Weak Key Derivation Function Algorithm
                    using (var key = new Rfc2898DeriveBytes(passwordBytes, SaltBytes, Interations)) {
#pragma warning restore CA5379 // Do Not Use Weak Key Derivation Function Algorithm
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);
                        aes.Mode = CipherMode.CBC;
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write)) {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Flush();
                        }

                        decryptedBytes = ms.ToArray();
                    }
                }
            }
            catch (CryptographicException) {
            }

            return decryptedBytes;
        }

        #endregion

        #region Mat

        public static byte[] ConvertMatToBuffer(Mat mat)
        {
            Contract.Requires(mat != null);
            mat.GetArray<byte>(out var buffer);
            return buffer;
        }

        public static Mat ConvertBufferToMat(byte[] buffer)
        {
            Contract.Requires(buffer != null);
            if (buffer.Length < 32) {
                return new Mat();
            }

            var mat = new Mat(buffer.Length / 32, 32, MatType.CV_8U);
            mat.SetArray(buffer);
            return mat;
        }

        #endregion
    }
}
