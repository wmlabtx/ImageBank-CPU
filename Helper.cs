﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace ImageBank
{
    public static class Helper
    {
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

        #region CleanupDirectories
        public static void CleanupDirectories(string startLocation, IProgress<string> progress)
        {
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

        #region Buffers

        public static byte[] ArrayFrom16(ushort[] array)
        {
            var buffer = new byte[array.Length * sizeof(ushort)];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public static ushort[] ArrayTo16(byte[] buffer)
        {
            var array = new ushort[buffer.Length / sizeof(ushort)];
            Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
            return array;
        }

        public static byte[] ArrayFrom32(int[] array)
        {
            var buffer = new byte[array.Length * sizeof(int)];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public static int[] ArrayTo32(byte[] buffer)
        {
            var array = new int[buffer.Length / sizeof(int)];
            Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
            return array;
        }

        public static byte[] ArrayFrom64(ulong[] array)
        {
            var buffer = new byte[array.Length * sizeof(ulong)];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public static ulong[] ArrayTo64(byte[] buffer)
        {
            var array = new ulong[buffer.Length / sizeof(ulong)];
            Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
            return array;
        }

        public static byte[] ArrayFromFloat(float[] array)
        {
            var buffer = new byte[array.Length * sizeof(float)];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public static float[] ArrayToFloat(byte[] buffer)
        {
            var array = new float[buffer.Length / sizeof(float)];
            Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
            return array;
        }

        #endregion

        #region Match

        public static int GetMatch(byte[] x, byte[] y)
        {
            if (x == null || x.Length == 0 || y == null || y.Length == 0) {
                return 0;
            }

            var m = 0;
            var i = 0;
            var j = 0;
            while (i < x.Length && j < y.Length) {
                if (x[i] == y[j]) {
                    m++;
                    i++;
                    j++;
                }
                else {
                    if (x[i] < y[j]) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            return m;
        }

        #endregion

        public static System.Windows.Media.SolidColorBrush GetBrush(int id)
        {

            byte rbyte, gbyte, bbyte;
            var array = BitConverter.GetBytes(id);
            using (var md5 = MD5.Create()) {
                var hashmd5 = md5.ComputeHash(array);
                rbyte = (byte)(hashmd5[3] | 0x80);
                gbyte = (byte)(hashmd5[4] | 0x80);
                bbyte = (byte)(hashmd5[5] | 0x80);
            }


                /*
                double theta;
                double radius;
                double l;
                var array = BitConverter.GetBytes(id);
                using (var md5 = MD5.Create()) {
                    var hashmd5 = md5.ComputeHash(array);
                    ulong x = BitConverter.ToUInt64(hashmd5, 5);
                    var f = x / (double)ulong.MaxValue;
                    theta = f * 360.0;
                    if (inv) {
                        theta += 180.0;
                        if (theta >= 360.0) {
                            theta -= 360.0;
                        }
                    }

                    radius = 50.0;
                    l = 75.0;
                }

                var rad = Math.PI * theta / 180.0;
                var lfloat = (float)l;
                var afloat = (float)(radius * Math.Cos(rad));
                var bfloat = (float)(radius * Math.Sin(rad));
                BitmapHelper.ToRGB(lfloat, afloat, bfloat, out byte rbyte, out byte gbyte, out byte bbyte);
                */

                var scb = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(rbyte, gbyte, bbyte));
                return scb;
            }
        }
}
