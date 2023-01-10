using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Text;

namespace ImageBank
{
    public static class FileHelper
    {
        const string AllowedChars = "0123456789abcdefghijkmnprstuvxyz"; // 36-4=32 lowq

        public static string NameToFileName(string hash, string name)
        {
            return $"{AppConsts.PathHp}\\{hash[0]}\\{hash[1]}\\{name}{AppConsts.MzxExtension}";
        }

        public static string HashToName(string hash, int length)
        {
            var sb = new StringBuilder();
            for (var i = 1; i <= length; i++) {
                var hex = hash.Substring(i * 2, 2);
                var val = int.Parse(hex, System.Globalization.NumberStyles.HexNumber); 
                var c = AllowedChars[val % AllowedChars.Length];
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static byte[] ReadEncryptedFile(string filename)
        {
            if (!File.Exists(filename)) {
                return null;
            }

            var earray = File.ReadAllBytes(filename);
            var password = Path.GetFileNameWithoutExtension(filename);
            var imgdata = EncryptionHelper.Decrypt(earray, password);
            return imgdata;
        }

        public static byte[] ReadFile(string filename)
        {
            if (!File.Exists(filename)) {
                return null;
            }

            var imgdata = File.ReadAllBytes(filename);
            return imgdata;
        }

        public static void WriteFile(string filename, byte[] imgdata)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filename, imgdata);
        }

        public static void WriteEncryptedFile(string filename, byte[] imgdata)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            var password = Path.GetFileNameWithoutExtension(filename);
            var earray = EncryptionHelper.Encrypt(imgdata, password);
            File.WriteAllBytes(filename, earray);
        }

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

        public static void MoveCorruptedFile(string filename)
        {
            var badname = Path.GetFileName(filename);
            var badfilename = $"{AppConsts.PathGb}\\{badname}";
            if (!badfilename.Equals(filename, StringComparison.OrdinalIgnoreCase)) {
                if (File.Exists(badfilename)) {
                    DeleteToRecycleBin(badfilename);
                }

                File.Move(filename, badfilename);
            }
        }
    }
}
