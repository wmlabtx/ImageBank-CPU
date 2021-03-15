using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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

        public static string ComputeHash(byte[] array)
        {
            using (var md5 = MD5.Create()) {
                var hashmd5 = md5.ComputeHash(array);
                var sb = new StringBuilder();
                for (var i = 0; i < 16; i++) {
                    sb.Append(hashmd5[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static string ComputeFolder(byte[] array)
        {
            using (var md5 = MD5.Create()) {
                var hashmd5 = md5.ComputeHash(array);
                var val = BitConverter.ToUInt64(hashmd5, 0);
                var ifolder = (val % 50) + 1;
                var folder = $"{ifolder:D2}";
                return folder;
            }
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
            str = $"{ksize:F4} Mb";
            return str;
        }

        #endregion

        #region Image

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

        #region FileData

        public static byte[] ReadData(string filename)
        {
            try
            {
                var encdata = File.ReadAllBytes(filename);
                var password = Path.GetFileNameWithoutExtension(filename);
                var imgdata = Decrypt(encdata, password);
                return imgdata;
            }
            catch (DirectoryNotFoundException ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
            catch (FileNotFoundException ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }

        public static void WriteData(string filename, byte[] imgdata)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var password = Path.GetFileNameWithoutExtension(filename);
            var encdata = Encrypt(imgdata, password);
            File.WriteAllBytes(filename, encdata);
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
    }
}
