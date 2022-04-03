using System;
using System.Diagnostics.Contracts;
using System.IO;

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
    }
}
