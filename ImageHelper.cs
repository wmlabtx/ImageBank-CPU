using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;
//using OpenCvSharp.ImgHash;
using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageBank
{
    public static class ImageHelper
    {
        private static readonly BFMatcher _bfmatcher;
        private static readonly SIFT _sift;

        static ImageHelper()
        {
            //_cmh = ColorMomentHash.Create();
            //_akaze = AKAZE.Create(threshold: 0.0001f);
            _bfmatcher = new BFMatcher();
            _sift = SIFT.Create(nFeatures: 10000);
        }

        public static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
        {
            bitmap = null;

            try {
                using (var mat = Cv2.ImDecode(data, ImreadModes.AnyColor)) {
                    bitmap = BitmapConverter.ToBitmap(mat);
                    if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                        bitmap = RepixelBitmap(bitmap);
                    }
                }
            }
            catch (ArgumentException) {
                bitmap = null;
                return false;
            }

            return true;
        }

        public static Bitmap RepixelBitmap(Image bitmap)
        {
            var bitmap24BppRgb = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap24BppRgb)) {
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }

            return bitmap24BppRgb;
        }

        public static Bitmap ResizeBitmap(Image bitmap, int width, int height)
        {
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

        public static MagicFormat GetMagicFormat(byte[] imagedata)
        {
            // https://en.wikipedia.org/wiki/List_of_file_signatures

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

        public static void SaveCorruptedFile(string filename)
        {
            var badname = Path.GetFileName(filename);
            var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
            Helper.DeleteToRecycleBin(badfilename);
            File.Move(filename, badfilename);
        }

        public static string SaveCorruptedImage(string filename, byte[] imagedata)
        {
            var baddir = $"{AppConsts.PathHp}\\{AppConsts.CorruptedExtension}";
            if (!Directory.Exists(baddir)) {
                Directory.CreateDirectory(baddir);
            }

            var badname = Path.GetFileName(filename);
            var badfilename = $"{baddir}\\{badname}";
            Helper.DeleteToRecycleBin(badfilename);
            File.WriteAllBytes(badfilename, imagedata);
            Helper.DeleteToRecycleBin(filename);
            return badfilename;
        }

        public static bool GetImageDataFromBitmap(Bitmap bitmap, out byte[] imagedata)
        {
            try {
                using (var mat = bitmap.ToMat()) {
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

        /*
        public static Mat GetColorMomentHash(Bitmap bitmap)
        {
            Mat mathash;
            using (var matsource = bitmap.ToMat()) {
                mathash = bitmap.ToMat();
                _cmh.Compute(matsource, mathash);
            }

            return mathash;
        }

        public static double GetDistance(Mat x, Mat y)
        {
            var distance = Cv2.Norm(x, y, NormTypes.L2);
            return distance;
        }
        */

        public static byte[] GetColorHistogram(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                bitmap = RepixelBitmap(bitmap);
            }

            byte[] histogram;
            using (var bitmap256x256 = ResizeBitmap(bitmap, 256, 256)) {
                var rect = new Rectangle(0, 0, bitmap256x256.Width, bitmap256x256.Height);
                BitmapData bmpdata = bitmap256x256.LockBits(rect, ImageLockMode.ReadWrite, bitmap256x256.PixelFormat);
                IntPtr ptr = bmpdata.Scan0;
                var bytes = bitmap256x256.Width * bitmap256x256.Height * 3;
                byte[] buffer = new byte[bytes];
                Marshal.Copy(ptr, buffer, 0, bytes);
                bitmap256x256.UnlockBits(bmpdata);
                var ihistogram = new int[4096];
                for (var i = 0; i < buffer.Length; i += 3) {
                    var red = (buffer[i + 2]) >> 4; // 4
                    var green = (buffer[i + 1]) >> 3; // 5
                    var blue = (buffer[i]) >> 5; // 3
                    var colorIndex = (red << 7) | (green << 3) | blue; // 12 bit
                    ihistogram[colorIndex]++;
                }

                histogram = new byte[ihistogram.Length];
                for (var i = 0; i < histogram.Length; i++) {
                    histogram[i] = (byte)Math.Sqrt(ihistogram[i]);
                }
            }

            return histogram;
        }

        public static long GetDistance(byte[] x, byte[] y)
        {
            long sum = 0;
            for (var i = 0; i < x.Length; i++) {
                var diff = x[i] - y[i];
                sum += diff * diff;
            }

            return sum;
        }

        public static Mat GetSiftDescriptors(Bitmap bitmap)
        {
            Mat descriptors = null;
            using (var matsource = bitmap.ToMat())
            using (var matcolor = bitmap.ToMat()) {
                var f = 768.0 / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) { 
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _sift.Detect(mat);
                    if (keypoints.Length > 0) {
                        descriptors = new Mat();
                        _sift.Compute(matcolor, ref keypoints, descriptors);

                        for (int i = 0; i < descriptors.Rows; i++) {
                            Cv2.Normalize(descriptors.Row(i), descriptors.Row(i), 1.0, 0.0, NormTypes.L1);
                        }
                        
                        Cv2.Sqrt(descriptors, descriptors);
                        using (var matkeypoints = new Mat()) {
                            Cv2.DrawKeypoints(matcolor, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                            matkeypoints.SaveImage("test.png");
                        }
                    }
                }
            }

            return descriptors;
        }

        public static Mat[] GetSift2Descriptors(Bitmap bitmap)
        {
            var descriptors = new Mat[2];
            descriptors[0] = GetSiftDescriptors(bitmap);
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                descriptors[1] = GetSiftDescriptors(brft);
            }

            return descriptors;
        }

        public static float GetDistance(Mat x, Mat y)
        {
            var matches = _bfmatcher.KnnMatch(x, y, 2);
            var goodMatches = 0;
            var sum = 0.0;
            foreach (DMatch[] items in matches.Where(e => e.Length > 1)) {
                if (items[0].Distance < 0.75f * items[1].Distance) {
                    goodMatches++;
                    sum += items[0].Distance;
                }
            }

            if (goodMatches > 0) {
                var distance = sum / goodMatches;
                return (float)distance;
            }
            else {
                return 1000f;
            }
        }

        public static float GetDistance(Mat x, Mat[] y)
        {
            var s1 = GetDistance(x, y[0]);
            var s2 = GetDistance(x, y[1]);
            var sim = Math.Min(s1, s2);
            return sim;
        }
    }
}