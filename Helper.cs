using Microsoft.VisualBasic.FileIO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
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
        private static readonly RNGCryptoServiceProvider _random = new RNGCryptoServiceProvider();

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

        #region Random

        /// <returns>xxx-xx-xxx</returns>
        public static string RandomName()
        {
            var d0 = "bcdfghjklmnpqrstvwxyz".ToCharArray();
            var d1 = "aeiou".ToCharArray();
            var d2 = "0123456789".ToCharArray();
            var sb = new StringBuilder("xxx-xx-xxx");
            var br = new byte[32];
            _random.GetBytes(br);
            var bv = new byte[1];
            _random.GetBytes(bv);
            if ((bv[0] & 1) == 0) {
                sb[0] = d0[BitConverter.ToUInt32(br, 0) % d0.Length];
                sb[1] = d1[BitConverter.ToUInt32(br, 4) % d1.Length];
                sb[2] = d0[BitConverter.ToUInt32(br, 8) % d0.Length];
            }
            else {
                sb[0] = d1[BitConverter.ToUInt32(br, 0) % d1.Length];
                sb[1] = d0[BitConverter.ToUInt32(br, 4) % d0.Length];
                sb[2] = d1[BitConverter.ToUInt32(br, 8) % d1.Length];
            }

            sb[3] = '-';
            sb[4] = d2[BitConverter.ToUInt32(br, 12) % d2.Length];
            sb[5] = d2[BitConverter.ToUInt32(br, 16) % d2.Length];
            sb[6] = '-';

            if ((bv[0] & 2) == 0) {
                sb[7] = d1[BitConverter.ToUInt32(br, 20) % d1.Length];
                sb[8] = d0[BitConverter.ToUInt32(br, 24) % d0.Length];
                sb[9] = d1[BitConverter.ToUInt32(br, 28) % d1.Length];
            }
            else {
                sb[7] = d0[BitConverter.ToUInt32(br, 20) % d0.Length];
                sb[8] = d1[BitConverter.ToUInt32(br, 24) % d1.Length];
                sb[9] = d0[BitConverter.ToUInt32(br, 28) % d0.Length];
            }

            return sb.ToString();
        }

        public static string RandomFamily()
        {
            var d0 = "bcdfghjklmnpqrstvwxyz".ToCharArray();
            var d1 = "aeiou".ToCharArray();
            var sb = new StringBuilder("01234");
            var br = new byte[20];
            _random.GetBytes(br);
            sb[0] = d0[BitConverter.ToUInt32(br, 0) % d0.Length];
            sb[1] = d1[BitConverter.ToUInt32(br, 4) % d1.Length];
            sb[2] = d0[BitConverter.ToUInt32(br, 8) % d0.Length];
            sb[3] = d1[BitConverter.ToUInt32(br, 12) % d1.Length];
            sb[4] = d0[BitConverter.ToUInt32(br, 16) % d0.Length];
            return sb.ToString();
        }

        #endregion

        #region Hash

        public static ulong ComputeHash(byte[] array)
        {
            using (var sha256 = SHA256.Create()) {
                var hash256 = sha256.ComputeHash(array);
                var hash64 = BitConverter.ToUInt64(hash256, 0);
                return hash64;
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

        #region Strings

        public static string GetFileName(string name, int folder)
        {
            return $"{AppConsts.PathHp}{folder:D2}\\{name}{AppConsts.MzxExtension}";
        }

        public static string GetShortName(Img img)
        {
            Contract.Requires(img != null);
            return $"{img.Folder:D2}\\{img.Name}";
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

        public static bool GetImageDataFromBitmap(Bitmap bitmap, out byte[] imagedata)
        {
            try {
                using (var mat = BitmapConverter.ToMat(bitmap)) {

                    //var iep = new ImageEncodingParam(ImwriteFlags.WebPQuality, 101);
                    //Cv2.ImEncode(AppConsts.WebpExtension, mat, out imagedata, iep);

                    //var iep = new ImageEncodingParam(ImwriteFlags.PngCompression, 9);
                    //Cv2.ImEncode(AppConsts.PngExtension, mat, out imagedata, iep);

                    var iep = new ImageEncodingParam(ImwriteFlags.JpegQuality, 95);
                    Cv2.ImEncode(AppConsts.JpgExtension, mat, out imagedata, iep);
                    return true;
                }
            }
            catch (ArgumentException) {
                imagedata = null;
                return false;
            }
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

        public static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
        {
            try {
                using (var mat = Cv2.ImDecode(data, ImreadModes.AnyColor)) {
                    if (mat == null) {
                        bitmap = null;
                        return false;
                    }

                    bitmap = BitmapConverter.ToBitmap(mat);
                    return true;
                }
            }
            catch (ArgumentException) {
                bitmap = null;
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
                !extension.Equals(AppConsts.WebpExtension, StringComparison.OrdinalIgnoreCase) &&
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
                imagedata = DecryptDat(imagedata, password);
                if (imagedata == null) {
                    message = "cannot be decrypted";
                    return false;
                }
            }

            if (extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase)) {
                var password = Path.GetFileNameWithoutExtension(filename);
                imagedata = Decrypt(imagedata, password);
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

            if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
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

            hash = ComputeHash(imagedata);
            
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

        #region Descriptors

        public static ulong[] BufferToDescriptors(byte[] buffer)
        {
            Contract.Requires(buffer != null);
            var descriptors = new ulong[buffer.Length / sizeof(ulong)];
            Buffer.BlockCopy(buffer, 0, descriptors, 0, buffer.Length);
            return descriptors;
        }

        public static byte[] DescriptorsToBuffer(ulong[] descriptors)
        {
            Contract.Requires(descriptors != null);
            var buffer = new byte[descriptors.Length * sizeof(ulong)];
            Buffer.BlockCopy(descriptors, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        #endregion
    }
}
