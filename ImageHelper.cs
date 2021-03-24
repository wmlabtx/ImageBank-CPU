using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.ImgHash;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImageBank
{
    public static class ImageHelper
    {
        const int MAXDIM = 768;
        const int MAXDESCRIPTORS = 100;
        private static readonly ORB _orb = ORB.Create(1000);

        private static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
        {
            bitmap = null;
            
            try
            {
                using (var mat = Cv2.ImDecode(data, ImreadModes.AnyColor))
                {
                    bitmap = BitmapConverter.ToBitmap(mat);
                }
            }
            catch (ArgumentException)
            {
                bitmap = null;
                return false;
            }

            return true;
        }

        private static Bitmap RepixelBitmap(Image bitmap)
        {
            var bitmap24BppRgb = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap24BppRgb))
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }

            return bitmap24BppRgb;
        }

        public static Bitmap ResizeBitmap(Image bitmap, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(bitmap, destRect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static MagicFormat GetMagicFormat(IReadOnlyList<byte> imagedata)
        {
            // https://en.wikipedia.org/wiki/List_of_file_signatures

            if (imagedata[0] == 0xFF && imagedata[1] == 0xD8 && imagedata[2] == 0xFF)
            {
                return MagicFormat.Jpeg;
            }

            if (imagedata[0] == 0x52 && imagedata[1] == 0x49 && imagedata[2] == 0x46 && imagedata[3] == 0x46 &&
                imagedata[8] == 0x57 && imagedata[9] == 0x45 && imagedata[10] == 0x42 && imagedata[11] == 0x50)
            {
                if (imagedata[15] == ' ')
                {
                    return MagicFormat.WebP;
                }

                if (imagedata[15] == 'L')
                {
                    return MagicFormat.WebPLossLess;
                }

                return MagicFormat.Unknown;
            }

            if (imagedata[0] == 0x89 && imagedata[1] == 0x50 && imagedata[2] == 0x4E && imagedata[3] == 0x47)
            {
                return MagicFormat.Png;
            }

            if (imagedata[0] == 0x42 && imagedata[1] == 0x4D)
            {
                return MagicFormat.Bmp;
            }

            return MagicFormat.Unknown;
        }

        public static bool GetImageDataFromBitmap(Bitmap bitmap, out byte[] imagedata)
        {
            try
            {
                using (var mat = bitmap.ToMat())
                {
                    var iep = new ImageEncodingParam(ImwriteFlags.JpegQuality, 95);
                    Cv2.ImEncode(AppConsts.JpgExtension, mat, out imagedata, iep);
                    return true;
                }
            }
            catch (ArgumentException)
            {
                imagedata = null;
                return false;
            }
        }

        public static bool GetImageDataFromFile(
            string filename,
            out byte[] imagedata,
            out Bitmap bitmap,
            out string message)
        {
            imagedata = null;
            bitmap = null;
            message = null;
            if (!File.Exists(filename))
            {
                message = "missing file";
                return false;
            }

            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension))
            {
                message = "no extention";
                return false;
            }

            if (
                !extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.DbxExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.PngExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.BmpExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.WebpExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpgExtension, StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(AppConsts.JpegExtension, StringComparison.OrdinalIgnoreCase)
                )
            {
                message = "unknown extention";
                return false;
            }

            imagedata = File.ReadAllBytes(filename);
            if (imagedata == null || imagedata.Length == 0)
            {
                message = "imgdata == null || imgdata.Length == 0";
                return false;
            }

            if (extension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase))
            {
                var password = Path.GetFileNameWithoutExtension(filename);
                imagedata = Helper.DecryptDat(imagedata, password);
                if (imagedata == null)
                {
                    message = "cannot be decrypted";
                    return false;
                }
            }

            if (extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase))
            {
                var password = Path.GetFileNameWithoutExtension(filename);
                imagedata = Helper.Decrypt(imagedata, password);
                if (imagedata == null)
                {
                    message = "cannot be decrypted";
                    return false;
                }
            }

            if (!GetBitmapFromImageData(imagedata, out bitmap))
            {
                message = "bad image";
                return false;
            }

            var bitmapchanged = false;

            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                bitmap = RepixelBitmap(bitmap);
                bitmapchanged = true;
            }

            var magicformat = GetMagicFormat(imagedata);
            if (magicformat != MagicFormat.Jpeg)
            {
                bitmapchanged = true;
            }

            if (bitmapchanged)
            {
                if (!GetImageDataFromBitmap(bitmap, out imagedata))
                {
                    message = "encode error";
                    return false;
                }

                File.WriteAllBytes(filename, imagedata);
            }

            return true;
        }

        public static ulong[] ArrayTo64(byte[] array)
        {
            var buffer = new ulong[array.Length / sizeof(ulong)];
            Buffer.BlockCopy(array, 0, buffer, 0, array.Length);
            return buffer;
        }

        public static byte[] ArrayFrom64(ulong[] array)
        {
            var buffer = new byte[array.Length * sizeof(ulong)];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public static void ComputeBlob(Bitmap bitmap, out ulong[] descriptors)
        {
            descriptors = null;
            using (var matsource = bitmap.ToMat())
            using (var matcolor = new Mat())
            {
                var f = (double)MAXDIM / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat())
                {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _orb.Detect(mat);
                    if (keypoints.Length > 0)
                    {
                        /*
                        using (var matkeypoints = new Mat())
                        {
                            Cv2.DrawKeypoints(matcolor, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                            matkeypoints.SaveImage($"matkeypoints_{keypoints.Length}.png");
                        }
                        */

                        var grid = new List<KeyPoint>[100];
                        foreach (var e in keypoints)
                        {
                            var xbin = (int)(e.Pt.X * 10f / mat.Width);
                            var ybin = (int)(e.Pt.Y * 10f / mat.Height);
                            var bin = ybin * 10 + xbin;
                            if (grid[bin] == null)
                            {
                                grid[bin] = new List<KeyPoint>();
                            }

                            grid[bin].Add(e);
                        }

                        var lkeypoints = new List<KeyPoint>();
                        foreach (var g in grid)
                        {
                            if (g != null && g.Count > 0)
                            {
                                var keypoint = g.OrderByDescending(e => e.Octave).ThenByDescending(e => e.Response).FirstOrDefault();
                                lkeypoints.Add(keypoint);
                            }
                        }

                        keypoints = lkeypoints.OrderByDescending(e => e.Octave).ThenByDescending(e => e.Response).Take(MAXDESCRIPTORS).ToArray();
                        using (var matdescriptors = new Mat())
                        {
                            _orb.Compute(mat, ref keypoints, matdescriptors);
                            if (matdescriptors.Rows > 0 && keypoints.Length > 0) {
                                /*
                                using (var matkeypoints = new Mat())
                                {
                                    Cv2.DrawKeypoints(matcolor, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                    matkeypoints.SaveImage("matkeypoints.png");
                                }
                                */

                                matdescriptors.GetArray(out byte[] array);
                                descriptors = ArrayTo64(array);
                            }
                        }
                    }
                }
            }
        }

        private static void ConvertToLAB(int ir, int ig, int ib, out double dl, out double da, out double db)
        {
            var r = ir / 255.0;
            var g = ig / 255.0;
            var b = ib / 255.0;

            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            var x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            var y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            var z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

            dl = (116.0 * y) - 16.0;
            da = 500.0 * (x - y);
            db = 200.0 * (y - z);
        }

        public static void ComputeHist(Bitmap bitmap, out byte[] hist)
        {           
            byte[] brgs;
            using (var bitmap256x256 = ResizeBitmap(bitmap, 256, 256)) {
                var rect = new Rectangle(0, 0, 256, 256);
                BitmapData bmpdata = bitmap256x256.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                IntPtr ptr = bmpdata.Scan0;
                var bytes = 256 * 256 * 3;
                brgs = new byte[bytes];
                Marshal.Copy(ptr, brgs, 0, bytes);
                bitmap256x256.UnlockBits(bmpdata);
            }

            var inthist = new int[1024];
            var offset = 0;
            while (offset < 256 * 256 * 3) {
                var blue = brgs[offset++];
                var green = brgs[offset++];
                var red = brgs[offset++];
                ConvertToLAB(red, green, blue, out var dl, out var da, out var db);
                
                // L 0.0 100.0 = 100.0
                // A -86.18 98.25 = 184.43
                // B -107.86 94.48 = 202.34

                var ol = (int)(dl / 25.06); // [0..100] -> 0..3 // 2 bit
                var oa = (int)((da + 87.0) / 11.57); // [-86..98] +87 [0..185]  -> 0..15 // 4 bit
                var ob = (int)((db + 108.0) / 12.63); // [-108..94] +108 [0..202] -> 0..15 // 4 bit
                var bin = (short)((oa << 6) | (ob << 2) | ol);
                inthist[bin]++;
            }

            hist = new byte[1024];
            offset = 0;
            while (offset < 1024) {
                hist[offset] = (byte)Math.Sqrt(inthist[offset]);
                offset++;
            }
        }

        public static float ComputeDistance(byte[] hx, byte[] hy)
        {
            var sum = 0f;
            var offset = 0;
            while (offset < 1024) {
                sum += (hx[offset] - hy[offset]) * (hx[offset] - hy[offset]);
                offset++;
            }

            sum /= 1024f;
            return sum;
        }

        private static ulong ComputeHashRotate(Bitmap bitmap, RotateFlipType rft)
        {
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(rft);
                using (var matsource = brft.ToMat())
                    using (var matcolor = new Mat()) {
                    Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(32, 32), 0, 0, InterpolationFlags.Area);
                    using (var mat = new Mat()) {
                        Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                        using (var phash = PHash.Create())
                        using (var matphash = new Mat()) {
                            phash.Compute(mat, matphash);
                            matphash.GetArray(out byte[] phasharray);
                            var hash = BitConverter.ToUInt64(phasharray, 0);
                            return hash;
                        }
                    }
                }
            }
        }

        public static void ComputePBlob(Bitmap bitmap, out ulong[] hashes)
        {
            hashes = new ulong[4];
            hashes[0] = ComputeHashRotate(bitmap, RotateFlipType.RotateNoneFlipNone);
            hashes[1] = ComputeHashRotate(bitmap, RotateFlipType.Rotate90FlipNone);
            hashes[2] = ComputeHashRotate(bitmap, RotateFlipType.Rotate270FlipNone);
            hashes[3] = ComputeHashRotate(bitmap, RotateFlipType.RotateNoneFlipX);
        }

        public static int ComputeDistance(ulong[] x, ulong[] y)
        {
            var mindistance = 256;
            for (var i = 0; i < x.Length; i++) {
                for (var j = 0; j < y.Length; j++) {
                    var d = Intrinsic.PopCnt(x[i] ^ y[j]);
                    if (d < mindistance){
                        mindistance = d;
                    }
                }
            }

            return mindistance;
        }

        public static byte[] ComputeDiff(ulong[] x, ulong[] y)
        {
            var m = new List<Tuple<int, int, int>>();
            for (var i = 0; i < x.Length; i += 4)
            {
                for (var j = 0; j < y.Length; j += 4)
                {
                    var d =
                        Intrinsic.PopCnt(x[i + 0] ^ y[j + 0]) +
                        Intrinsic.PopCnt(x[i + 1] ^ y[j + 1]) +
                        Intrinsic.PopCnt(x[i + 2] ^ y[j + 2]) +
                        Intrinsic.PopCnt(x[i + 3] ^ y[j + 3]);

                    m.Add(new Tuple<int, int, int>(i, j, d));
                }
            }

            m.Sort((a1, a2) => a1.Item3.CompareTo(a2.Item3));
            var diff = new List<byte>();
            while (m.Count > 0)
            {
                diff.Add((byte)m[0].Item3);
                var ix = m[0].Item1;
                var iy = m[0].Item2;
                m.RemoveAll(e => e.Item1 == ix || e.Item2 == iy);
            }

            return diff.ToArray();
        }

        public static string ShowDiff(byte[] diff)
        {
            for (var i = 0; i < diff.Length; i++) {
                if (diff[i] != 0) {
                    return $"{i}:{diff[i]}";
                }
            }

            return $"EQ";
        }

        public static int CompareDiff(byte[] x, byte[] y)
        {
            for (var i = 0; i < x.Length; i++) {
                var c = x[i].CompareTo(y[i]);
                if (c != 0) {
                    return c;
                }
            }

            return 0;
        }

        /*
        public static string ComputeFolder(Bitmap bitmap)
        {
            using (var matsource = bitmap.ToMat())
            using (var matcolor = new Mat()) {
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(256, 256), 0, 0, InterpolationFlags.Area);
                using (var matcolumn = matcolor.Reshape(3, matcolor.Rows * matcolor.Cols))
                using (var mat = new Mat()) {
                    matcolumn.ConvertTo(mat, MatType.CV_32FC3);
                    using (var matbestlabels = new Mat())
                    using (Mat matcenters = new Mat()) {
                        
                        Cv2.Kmeans(
                            mat,
                            8,
                            matbestlabels,
                            new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 10, 1.0),
                            3,
                            KMeansFlags.PpCenters,
                            matcenters);

                        matbestlabels.GetArray(out int[] bestlabels);
                        matcenters.GetArray(out float[] centers);
                        var hl = new int[8];
                        foreach (var l in bestlabels) {
                            hl[l]++;
                        }

                        using (var pbitmap = new Bitmap(64 * 8, 64, PixelFormat.Format24bppRgb))
                        using (var graphics = Graphics.FromImage(pbitmap))
                        {
                            for (var i = 0; i < 8; i++) {
                                using (var myBrush = new SolidBrush(Color.FromArgb((int)centers[i * 3 + 2], (int)centers[i * 3 + 1], (int)centers[i * 3 + 0])))
                                {
                                    graphics.FillRectangle(myBrush, new Rectangle(i * 64, 0, 64, 64));
                                }
                            }

                            pbitmap.Save("palette8.png", ImageFormat.Png);
                        }

                        var bc = 0;
                        var bcv = 0;
                        for (var i = 0; i < hl.Length; i++) {
                            if (hl[i] > bcv) {
                                bc = i;
                                bcv = hl[i];
                            }
                        }

                        var rcolor = (int)Math.Floor(centers[2]);
                        var gcolor = (int)Math.Floor(centers[1]);
                        var bcolor = (int)Math.Floor(centers[0]);

                        ///

                        Cv2.Kmeans(
                            mat,
                            1,
                            matbestlabels,
                            new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 10, 1.0),
                            3,
                            KMeansFlags.PpCenters,
                            matcenters);

                        matbestlabels.GetArray(out int[] bestlabels);
                        matcenters.GetArray(out float[] centers);
                        

                        var rcolor = (int)Math.Floor(centers[2]);
                        var gcolor = (int)Math.Floor(centers[1]);
                        var bcolor = (int)Math.Floor(centers[0]);

                        ///
                        
                        using (var pbitmap = new Bitmap(256, 256, PixelFormat.Format24bppRgb))
                        using (var graphics = Graphics.FromImage(pbitmap))
                        using (var myBrush = new SolidBrush(Color.FromArgb(rcolor, gcolor, bcolor))) {
                            graphics.FillRectangle(myBrush, new Rectangle(0, 0, 256, 256));
                            pbitmap.Save("palette.png", ImageFormat.Png);
                        }

                        var r = rcolor / 255.0;
                        var g = gcolor / 255.0;
                        var b = bcolor / 255.0;
                        var r2 = rcolor >> 6;
                        var g2 = gcolor >> 6;
                        var b2 = bcolor >> 6;
                        var irgb = (byte)((r2 << 4) | (g2 << 2) | b2);

                        r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
                        g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
                        b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

                        var x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
                        var y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
                        var z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

                        x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
                        y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
                        z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

                        var lfloat = (float)((116.0 * y) - 16.0);
                        var afloat = (float)(500.0 * (x - y));
                        var bfloat = (float)(200.0 * (y - z));
                    }
                }

            }
            
            return null;
        }
        */

        /*
        public static int CompareMap(byte[] x, byte[] y)
        {
            var i = 0;
            var j = 0;
            var m = 0;
            while (i < x.Length && j < y.Length) {
                if (x[i] == y[j]) {
                    i++;
                    j++;
                    m++;
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
        */
    }
}