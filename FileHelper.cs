﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;

namespace ImageBank
{
    public static class FileHelper
    {
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

        public static void WriteData(string filename, byte[] imgdata)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filename, imgdata);
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
