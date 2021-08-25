using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageBank
{
    public static class ImageHelper
    {
        //private static readonly CryptoRandom _random;
        private static readonly SIFT _sift;
        private static readonly BFMatcher _bf;

        static ImageHelper()
        {
            //_random = new CryptoRandom();
            _sift = SIFT.Create(nFeatures:AppConsts.MaxDescriptors * 16, contrastThreshold:0.01, edgeThreshold:40.0);
            _bf = new BFMatcher();
        }

        public static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
        {
            bitmap = null;
            
            try
            {
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
            using (var graphics = Graphics.FromImage(destImage))
            {
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

        public static void GetDescriptors(Bitmap bitmap, out Mat descriptors, out KeyPoint[] keypoints, out Mat matkeypoints)
        {
            descriptors = null;
            matkeypoints = null;
            using (var matsource = bitmap.ToMat())
            using (var matcolor = bitmap.ToMat()) {
                var f = 768.0 / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    keypoints = _sift.Detect(mat);
                    if (keypoints.Length > 0) {
                        keypoints = keypoints
                            .OrderByDescending(e => e.Size)
                            .Where(e => e.Size >= AppConsts.MinDescriptorSize)
                            .Take(AppConsts.MaxDescriptors)
                            .ToArray();

                        if (keypoints.Length == 0) {
                            return;
                        }

                        descriptors = new Mat();
                        _sift.Compute(mat, ref keypoints, descriptors);
                        matkeypoints = new Mat();
                        Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                    }
                }
            }
        }

        public static void GetDescriptors(Bitmap bitmap, out Mat[] descriptors, out KeyPoint[][] keypoints, out Mat mat)
        {
            descriptors = new Mat[2];
            keypoints = new KeyPoint[2][];
            GetDescriptors(bitmap, out Mat d1, out KeyPoint[] k1, out mat);
            descriptors[0] = d1;
            keypoints[0] = k1;
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                GetDescriptors(brft, out Mat d2, out KeyPoint[] k2, out Mat m2);
                descriptors[1] = d2;
                keypoints[1] = k2;
                m2.Dispose();
            }
        }

        public static Point2d Point2fToPoint2d(Point2f pf) => new Point2d((int)pf.X, (int)pf.Y);

        public static float GetSim(Mat x, KeyPoint[] kx, Mat y, KeyPoint[] ky)
        {
            var matches = _bf.KnnMatch(x, y, 2);
            var pointsSrc = new List<Point2f>();
            var pointsDst = new List<Point2f>();
            var goodMatches = new List<DMatch>();
            foreach (DMatch[] items in matches.Where(e => e.Length > 1)) {
                if (items[0].Distance < 0.75f * items[1].Distance) {
                    pointsSrc.Add(kx[items[0].QueryIdx].Pt);
                    pointsDst.Add(ky[items[0].TrainIdx].Pt);
                    goodMatches.Add(items[0]);
                }
            }

            /*
            var pSrc = pointsSrc.ConvertAll(new Converter<Point2f, Point2d>(Point2fToPoint2d));
            var pDst = pointsDst.ConvertAll(new Converter<Point2f, Point2d>(Point2fToPoint2d));
            float sim;
            using (var outMask = new Mat()) {
                if (pSrc.Count >= 4 && pDst.Count >= 4) {
                    Cv2.FindHomography(pSrc, pDst, HomographyMethods.Ransac, mask: outMask);
                    var nonZero = Cv2.CountNonZero(outMask);
                    sim = 100f * nonZero / kx.Length;

                }
                else {
                    sim = 0f;
                }
            }
            */

            var sim = 100f * goodMatches.Count / kx.Length;
            return sim;
        }

        public static float GetSim(Mat x, KeyPoint[] kx, Mat[] y, KeyPoint[][] ky)
        {
            var s1 = GetSim(x, kx, y[0], ky[0]);
            var s2 = GetSim(x, kx, y[1], ky[1]);
            var sim = Math.Max(s1, s2);
            return sim;
        }
    }
}