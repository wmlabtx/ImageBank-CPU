using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;

namespace ImageBank
{
    public static class FileHelper
    {
        public static string HashToName(string hash, int iteration)
        {
            var name = hash.Substring(iteration + 2, 10);
            return name;
        }

        public static string NameToFileName(string name)
        {
            if (name == null || name.Length != 10) {
                return null;
            }

            var f = name.Substring(0, 2);
            var n = name.Substring(2, 8);
            return $"{AppConsts.PathHp}\\{f}\\{n}{AppConsts.MzxExtension}";
        }

        public static byte[] ReadData(string filename)
        {
            if (!File.Exists(filename)) {
                return null;
            }

            var earray = File.ReadAllBytes(filename);
            var password = Path.GetFileNameWithoutExtension(filename);
            var imgdata = EncryptionHelper.Decrypt(earray, password);
            return imgdata;
        }

        public static void WriteData(string filename, byte[] imgdata)
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
    }
}
