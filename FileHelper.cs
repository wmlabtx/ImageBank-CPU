using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ImageBank
{
    public static class FileHelper
    {
        const string AllowedChars = "0123456789abcdefghijkmnprstuvxyz"; // 36-4=32 lowq

        public static string GetHash(byte[] imagedata)
        {
            var sb = new StringBuilder();
            using (var sha256 = SHA256.Create()) {
                var raw = sha256.ComputeHash(imagedata);
                for (var i = 1; i <= AppConsts.HashLength; i++) {
                    var c = AllowedChars[raw[i] & 0x1f];
                    sb.Append(c);
                }
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
                /*
                if (File.Exists(filename)) {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    FileSystem.DeleteFile(filename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                */

                if (File.Exists(filename)) {
                    var now = DateTime.Now;
                    File.SetAttributes(filename, FileAttributes.Normal);
                    var name = Path.GetFileNameWithoutExtension(filename);
                    var extension = Path.GetExtension(filename);
                    string deletedfilename;
                    var counter = 0;
                    do {
                        if (counter == 0) {
                            deletedfilename = $"{AppConsts.PathDeProtected}\\{now.Year}-{now.Month:D2}-{now.Day:D2}\\{name}{extension}";
                        }
                        else {
                            deletedfilename = $"{AppConsts.PathDeProtected}\\\\{now.Year}-{now.Month:D2}-{now.Day:D2}\\{name}({counter}){extension}";
                        }

                        counter++;
                    }
                    while (File.Exists(deletedfilename));
                    var directory = Path.GetDirectoryName(deletedfilename);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                    }

                    File.Move(filename, deletedfilename);
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
            var badfilename = $"{AppConsts.PathGbProtected}\\{badname}";
            if (!badfilename.Equals(filename, StringComparison.OrdinalIgnoreCase)) {
                if (File.Exists(badfilename)) {
                    DeleteToRecycleBin(badfilename);
                }

                File.Move(filename, badfilename);
            }
        }
    }
}
