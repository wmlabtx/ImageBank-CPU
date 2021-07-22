using OpenCvSharp;
using OpenCvSharp.Extensions;
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
        const int MAXDIM = 500;
        const int KAZESIZE = 64;
        const int MAXCLUSTERS = 16 * 1024;

        private static readonly KAZE _kaze;
        private static readonly BFMatcher _bfmatch;
        private static readonly BOWImgDescriptorExtractor _bow;
        private static readonly Mat _clusters;

        static ImageHelper()
        {
            _kaze = KAZE.Create();
            _bfmatch = new BFMatcher(NormTypes.L2);
            _bow = new BOWImgDescriptorExtractor(_kaze, _bfmatch);
            var data = File.ReadAllBytes(AppConsts.FileKazeClusters);
            var fdata = new float[data.Length / sizeof(float)];
            Buffer.BlockCopy(data, 0, fdata, 0, data.Length);
            _clusters = new Mat(MAXCLUSTERS, KAZESIZE, MatType.CV_32F);
            _clusters.SetArray(fdata);
            _bow.SetVocabulary(_clusters);
        }

        public static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
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

        public static byte[] KpToBuffer(int[] kp)
        {
            var buffer = new byte[kp.Length * sizeof(int)];
            Buffer.BlockCopy(kp, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        public static int[] KpFromBuffer(byte[] buffer)
        {
            var kp = new int[buffer.Length / sizeof(int)];
            Buffer.BlockCopy(buffer, 0, kp, 0, buffer.Length);
            return kp;
        }

        public static void ComputeKpDescriptors(Bitmap bitmap, out int[] kp)
        {
            kp = null;
            using (var matsource = bitmap.ToMat())
            using (var matcolor = new Mat()) {
                var f = (double)MAXDIM / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _kaze.Detect(mat);
                    if (keypoints.Length > 0) {
                        keypoints = keypoints.OrderByDescending(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
                        using (var matdescriptors = new Mat())
                        using (var matbow = new Mat()) {
                            _bow.Compute(mat, ref keypoints, matbow, out var idx, matdescriptors);

                            /*
                            using (var matkeypoints = new Mat()) {
                                Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                matkeypoints.SaveImage("ki500x500.png");
                            }
                            */

                            var ki = new short[keypoints.Length];
                            for (var i = 0; i < idx.Length; i++) {
                                for (var j = 0; j < idx[i].Length; j++) {
                                    var k = idx[i][j];
                                    ki[k] = (short)i;
                                }
                            } 

                            kp = new int[keypoints.Length];
                            for (var i = 0; i < keypoints.Length; i++) {
                                var dmin = float.MaxValue;
                                var jmin = 0;
                                for (var j = 0; j < keypoints.Length; j++) {
                                    if (i == j) {
                                        continue;
                                    }

                                    var xd = keypoints[i].Pt.X - keypoints[j].Pt.X;
                                    var yd = keypoints[i].Pt.Y - keypoints[j].Pt.Y;
                                    var d2 = xd * xd + yd * yd;
                                    if (d2 < dmin) {
                                        dmin = d2;
                                        jmin = j;
                                    }
                                }

                                var kpv = ki[i] <= ki[jmin] ? (ki[i] << 14) | (int)ki[jmin] : (ki[jmin] << 14) | (int)ki[i];
                                kp[i] = kpv;
                            }

                            Array.Sort(kp);
                        }
                    }
                }
            }
        }

        public static void ComputeKpDescriptors(Bitmap bitmap, out int[] kp, out int[] mkp)
        {
            ComputeKpDescriptors(bitmap, out kp);
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                ComputeKpDescriptors(brft, out mkp);
            }
        }

        public static int ComputeKpMatch(int[] cx, int[] cy)
        {
            var match = 0;
            var i = 0;
            var j = 0;
            while (i < cx.Length && j < cy.Length) {
                if (cx[i] == cy[j]) {
                    match++;
                    i++;
                    j++;
                }
                else {
                    if (cx[i] < cy[j]) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            return match;
        }

        public static int ComputeKpMatch(int[] x, int[] y, int[] ym)
        {
            if (x == null || y == null || ym == null) {
                return 0;
            }

            var m1 = ComputeKpMatch(x, y);
            var m2 = ComputeKpMatch(x, ym);
            var m = Math.Max(m1, m2);
            return m;
        }
    }
}