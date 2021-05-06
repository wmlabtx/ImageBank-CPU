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
        const int MAXDIM = 1024;
        private static readonly AKAZE _akaze = AKAZE.Create();
        private static readonly BFMatcher _bfmatch = new BFMatcher(NormTypes.Hamming);

        private static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
        {
            bitmap = null;
            
            try
            {
                using (var mat = Cv2.ImDecode(data, ImreadModes.AnyColor)) {
                    bitmap = BitmapConverter.ToBitmap(mat);
                }
            }
            catch (ArgumentException) {
                bitmap = null;
                return false;
            }

            return true;
        }

        private static Bitmap RepixelBitmap(Image bitmap)
        {
            var bitmap24BppRgb = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap24BppRgb)) {
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

        public static void ComputeAkazeDescriptors(Bitmap bitmap, out Mat adescriptors)
        {
            adescriptors = null;
            using (var matsource = bitmap.ToMat())
            using (var matcolor = new Mat()) {
                var f = (double)MAXDIM / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _akaze.Detect(mat);
                    if (keypoints.Length > 0) {
                        var akeypoints = keypoints.OrderByDescending(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
                        adescriptors = new Mat();
                        _akaze.Compute(mat, ref akeypoints, adescriptors);
                        if (adescriptors.Rows > 0 && keypoints.Length > 0) {
                            using (var matkeypoints = new Mat()) {
                                Cv2.DrawKeypoints(mat, akeypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                matkeypoints.SaveImage("akeypoints.png");
                            }
                        }
                    }
                }
            }
        }

        public static int ComputeAkazePairs(Mat a1, Mat a2)
        {
            var matches = _bfmatch.KnnMatch(a1, a2, 2);
            using (var mask = new Mat(matches.Length, 1, MatType.CV_8U)) {
                mask.SetTo(new Scalar(255));
                int nonZero = Cv2.CountNonZero(mask);
                VoteForUniqueness(matches, mask);
                nonZero = Cv2.CountNonZero(mask);
                if (nonZero <= 0) {
                    return 0;
                }

                return nonZero;
            }
        }

        public static byte[] AkazeDescriptorsToCentoid(Mat adescriptors)
        {
            var buffer = ArrayFromMat(adescriptors);
            var fc = new int[488];
            var counter = buffer.Length / 61;
            for (var i = 0; i < counter; i++) {
                var off1 = i * 61;
                for (var b = 0; b < 488; b++) {
                    var off2 = off1 + (b >> 3);
                    var mask = 1 << (b & 0x7);
                    if ((buffer[off2] & mask) != 0) {
                        fc[b]++;
                    }
                }
            }

            var centroid = new byte[488];
            for (var i = 0; i < 488; i++) {
                centroid[i] = (byte)(fc[i] * 255f / counter);
            }

            return centroid;
        }

        public static ulong ComputeCentoidDistance(byte[] cx, byte[] cy)
        {
            var sum = 0UL;
            for (var i = 0; i < 488; i++) {
                var delta = cx[i] - cy[i];
                sum += (ulong)(delta * delta);
            }

            return sum;
        }

        public static byte[] ArrayFromMat(Mat mat)
        {
            mat.GetArray(out byte[] array);
            return array;
        }

        public static Mat ArrayToMat(byte[] array)
        {
            var rows = array.Length / AppConsts.DescriptorSize;
            var cols = AppConsts.DescriptorSize;
            var mat = new Mat(rows, cols, MatType.CV_8U);
            mat.SetArray(array);
            return mat;
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

        public static void ComputePerceptiveDescriptors(Bitmap bitmap, out ulong[] perceptivedescriptors)
        {
            perceptivedescriptors = new ulong[4];
            perceptivedescriptors[0] = ComputeHashRotate(bitmap, RotateFlipType.RotateNoneFlipNone);
            perceptivedescriptors[1] = ComputeHashRotate(bitmap, RotateFlipType.Rotate90FlipNone);
            perceptivedescriptors[2] = ComputeHashRotate(bitmap, RotateFlipType.Rotate270FlipNone);
            perceptivedescriptors[3] = ComputeHashRotate(bitmap, RotateFlipType.RotateNoneFlipX);
        }

        public static int ComputePerceptiveDistance(ulong[] x, ulong[] y)
        {
            var mindistance = AppConsts.MaxPerceptiveDistance;
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

        private static void VoteForUniqueness(DMatch[][] matches, Mat mask, float uniqnessThreshold = 0.80f)
        {
            var maskData = new byte[matches.Length];
            var maskHandle = GCHandle.Alloc(maskData, GCHandleType.Pinned);
            using (var m = new Mat(matches.Length, 1, MatType.CV_8U, maskHandle.AddrOfPinnedObject())) {
                mask.CopyTo(m);
                for (int i = 0; i < matches.Length; i++) {
                    if (matches[i].Length >= 2 && (matches[i][0].Distance / matches[i][1].Distance) <= uniqnessThreshold) {
                        maskData[i] = 255;
                    }
                    else {
                        maskData[i] = 0;
                    }
                }

                m.CopyTo(mask);
            }

            maskHandle.Free();
        }
    }
}