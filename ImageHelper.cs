using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using System;
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
        private static readonly BFMatcher _bfmatcher;
        private static readonly AKAZE _akaze;
        private static readonly CryptoRandom _random;
        private static readonly MSER _mser;
        private static readonly SIFT _sift;
        private static readonly ORB _orb;

        static ImageHelper()
        {
            _bfmatcher = new BFMatcher(NormTypes.Hamming);
            _akaze = AKAZE.Create();
            _random = new CryptoRandom();
            _mser = MSER.Create();
            _sift = SIFT.Create();
            _orb = ORB.Create();
        }

        public static ulong Random => _random.GetRandom64();

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
            FileHelper.DeleteToRecycleBin(badfilename);
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
            FileHelper.DeleteToRecycleBin(badfilename);
            File.WriteAllBytes(badfilename, imagedata);
            FileHelper.DeleteToRecycleBin(filename);
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

        public static Mat[] GetAkaze2Descriptors(Bitmap bitmap)
        {
            var descriptors = new Mat[2];
            using (var matsource = bitmap.ToMat())
            using (var matcolor = bitmap.ToMat()) {
                var f = 512.0 / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _akaze.Detect(mat);
                    if (keypoints.Length < AppConsts.NumDescriptors) {
                        return null;
                    }

                    keypoints = keypoints.OrderByDescending(e => e.Size).Take(AppConsts.NumDescriptors).ToArray();

                    descriptors[0] = new Mat();
                    _akaze.Compute(mat, ref keypoints, descriptors[0]);
                    if (keypoints.Length != AppConsts.NumDescriptors) {
                        return null;
                    }

                    using (var matkeypoints = new Mat()) {
                        Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                        matkeypoints.SaveImage("test0.png");
                    }

                    using (var matflip = new Mat()) {
                        Cv2.Flip(mat, matflip, FlipMode.Y);

                        keypoints = _akaze.Detect(matflip);
                        if (keypoints.Length < AppConsts.NumDescriptors) {
                            return null;
                        }

                        keypoints = keypoints.OrderByDescending(e => e.Size).Take(AppConsts.NumDescriptors).ToArray();

                        descriptors[1] = new Mat();
                        _akaze.Compute(matflip, ref keypoints, descriptors[1]);
                        if (keypoints.Length != AppConsts.NumDescriptors) {
                            return null;
                        }

                        /*
                        using (var matkeypoints = new Mat()) {
                            Cv2.DrawKeypoints(matflip, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                            matkeypoints.SaveImage("test1.png");
                        }
                        */
                    }
                }
            }

            return descriptors;
        }


        public static float GetDistance(Mat x, Mat y)
        {
            var dmatch = _bfmatcher.Match(x, y);
            var bestdmatch = dmatch.OrderBy(e => e.Distance).Take(AppConsts.NumDescriptors / 10);
            var distance = bestdmatch.Average(e => e.Distance);
            return distance;
        }

        public static float GetDistance(Mat x, Mat[] y)
        {
            var d0 = GetDistance(x, y[0]);
            var d1 = GetDistance(x, y[1]);
            var d = (float)Math.Min(d0, d1);
            return d;
        }

        public static Mat GetAkazeDescriptors(Bitmap bitmap)
        {
            var descriptors = new Mat();
            using (var matsource = bitmap.ToMat())
            using (var matcolor = bitmap.ToMat()) {
                var f = 512.0 / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _akaze.Detect(mat);
                    if (keypoints.Length < AppConsts.NumDescriptors) {
                        return null;
                    }

                    keypoints = keypoints.OrderByDescending(e => e.Size).Take(AppConsts.NumDescriptors).ToArray();

                    descriptors = new Mat();
                    _akaze.Compute(mat, ref keypoints, descriptors);
                    if (keypoints.Length != AppConsts.NumDescriptors) {
                        return null;
                    }

                    using (var matkeypoints = new Mat()) {
                        Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                        matkeypoints.SaveImage("test0.png");
                    }
                }
            }

            return descriptors;
        }

        public static Mat GetSiftDescriptors(Bitmap bitmap)
        {
            var descriptors = new Mat();
            using (var matsource = bitmap.ToMat())
            using (var matcolor = bitmap.ToMat()) {
                var f = 512.0 / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _mser.Detect(mat);
                    if (keypoints.Length < AppConsts.NumDescriptors) {
                        return null;
                    }

                    keypoints = keypoints.OrderByDescending(e => e.Size).Take(AppConsts.NumDescriptors).ToArray();

                    descriptors = new Mat();
                    _sift.Compute(mat, ref keypoints, descriptors);
                    if (keypoints.Length != AppConsts.NumDescriptors) {
                        return null;
                    }

                    using (var matkeypoints = new Mat()) {
                        Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                        matkeypoints.SaveImage("test0.png");
                    }
                }
            }

            return descriptors;
        }

        public static Mat[] Get2Descriptors(Bitmap bitmap)
        {
            var descriptors = new Mat[2];
            using (var matsource = bitmap.ToMat())
            using (var matcolor = bitmap.ToMat()) {
                var f = 512.0 / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var mserkeypoints = _mser.Detect(mat);
                    if (mserkeypoints.Length < AppConsts.NumDescriptors) {
                        return null;
                    }

                    descriptors[0] = new Mat();
                    //_akaze.Compute(mat, ref keypoints, descriptors[0]);


                    _orb.Compute(mat, ref mserkeypoints, descriptors[0]);
                    if (mserkeypoints.Length < AppConsts.NumDescriptors) {
                        return null;
                    }

                    using (var matkeypoints = new Mat()) {
                        Cv2.DrawKeypoints(mat, mserkeypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                        matkeypoints.SaveImage("test0.png");
                    }

                    using (var matflip = new Mat()) {
                        Cv2.Flip(mat, matflip, FlipMode.Y);

                        mserkeypoints = _mser.Detect(matflip);
                        if (mserkeypoints.Length < AppConsts.NumDescriptors) {
                            return null;
                        }

                        //mserkeypoints = mserkeypoints.OrderByDescending(e => e.Size).Take(AppConsts.NumDescriptors).ToArray();

                        descriptors[1] = new Mat();
                        _orb.Compute(matflip, ref mserkeypoints, descriptors[1]);
                        if (mserkeypoints.Length < AppConsts.NumDescriptors) {
                            return null;
                        }

                        //mserkeypoints = mserkeypoints.OrderByDescending(e => e.Size).Take(AppConsts.NumDescriptors).ToArray();

                        using (var matkeypoints = new Mat()) {
                            Cv2.DrawKeypoints(matflip, mserkeypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                            matkeypoints.SaveImage("test1.png");
                        }
                    }
                }
            }

            return descriptors;
        }
    }
}