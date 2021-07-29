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
        const int MAXDIM = 750;
        const int KAZESIZE = 64;
        const int MAXCLUSTERS = 16 * 1024;

        private static readonly KAZE _kaze;
        private static readonly BFMatcher _bfmatch;
        private static readonly BOWImgDescriptorExtractor _bow;
        private static readonly Mat _clusters;
        private static readonly CryptoRandom _random;

        static ImageHelper()
        {
            _kaze = KAZE.Create(threshold: 0.0001f);
            _bfmatch = new BFMatcher(NormTypes.L2);
            _bow = new BOWImgDescriptorExtractor(_kaze, _bfmatch);
            var data = File.ReadAllBytes(AppConsts.FileKazeClusters);
            var fdata = new float[data.Length / sizeof(float)];
            Buffer.BlockCopy(data, 0, fdata, 0, data.Length);
            _clusters = new Mat(MAXCLUSTERS, KAZESIZE, MatType.CV_32F);
            _clusters.SetArray(fdata);
            _bow.SetVocabulary(_clusters);
            _random = new CryptoRandom();
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

        public static void ComputeKazeDescriptors(Bitmap bitmap, out short[] ki, out short[] kx, out short[] ky)
        {
            ki = null;
            kx = null;
            ky = null;
            using (var matsource = bitmap.ToMat())
            using (var matcolor = new Mat()) {
                var f = (double)MAXDIM / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _kaze.Detect(mat);
                    if (keypoints.Length > 0) {
                        keypoints = keypoints.OrderByDescending(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
                        if (keypoints.Length < AppConsts.MinDescriptors) {
                            return;
                        }

                        using (var matdescriptors = new Mat())
                        using (var matbow = new Mat()) {
                            _bow.Compute(mat, ref keypoints, matbow, out var idx, matdescriptors);

                            /*
                            using (var matkeypoints = new Mat()) {
                                Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                matkeypoints.SaveImage("mat.png");
                            }
                            */

                            var kazapoints = new Tuple<short, short, short>[keypoints.Length];
                            for (var i = 0; i < idx.Length; i++) {
                                for (var j = 0; j < idx[i].Length; j++) {
                                    var k = idx[i][j];
                                    kazapoints[k] = new Tuple<short, short, short>((short)i, (short)Math.Round(keypoints[k].Pt.X), (short)Math.Round(keypoints[k].Pt.Y));
                                }
                            }

                            var kp = kazapoints.OrderBy(e => e.Item1).ToArray();
                            ki = kp.Select(e => e.Item1).ToArray();
                            kx = kp.Select(e => e.Item2).ToArray();
                            ky = kp.Select(e => e.Item3).ToArray();
                        }
                    }
                }
            }
        }

        public static void ComputeKazeDescriptors(Bitmap bitmap, out short[] ki, out short[] kx, out short[] ky, out short[] kimirror, out short[] kxmirror, out short[] kymirror)
        {
            ComputeKazeDescriptors(bitmap, out ki, out kx, out ky);
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                ComputeKazeDescriptors(brft, out kimirror, out kxmirror, out kymirror);
            }
        }

        public static int GetMatch(short[] k1, short[] k2)
        {
            var m = 0;
            var i = 0;
            var j = 0;
            while (i < k1.Length && j < k2.Length) {
                if (k1[i] == k2[j]) {
                    m++;
                    i++;
                    j++;
                }
                else {
                    if (k1[i] < k2[j]) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            return m;
        }

        public static short[] GetRandomVector(short[] ki, short[] kx, short[] ky)
        {
            if (ki.Length < AppConsts.MinDescriptors) {
                throw new ArgumentOutOfRangeException(nameof(ki));
            }

            var xmin = kx.Min();
            var xmax = kx.Max();
            var ymin = ky.Min();
            var ymax = ky.Max();
            short x1, x2, y1, y2;
            var list = new List<short>();
            do {
                x1 = _random.NextShort(xmin, xmax);
                x2 = _random.NextShort(xmin, xmax);
                if (x1 > x2) {
                    var temp = x1;
                    x1 = x2;
                    x2 = temp;
                }

                y1 = _random.NextShort(ymin, ymax);
                y2 = _random.NextShort(ymin, ymax);
                if (y1 > y2) {
                    var temp = y1;
                    y1 = y2;
                    y2 = temp;
                }

                list.Clear();
                for (var i = 0; i < ki.Length; i++) {
                    if (kx[i] < x1 || kx[i] > x2 || ky[i] < y1 || ky[i] > y2) {
                        continue;
                    }

                    list.Add(ki[i]);
                }
            }
            while (list.Count < AppConsts.MinDescriptors);
            return list.ToArray();
        }

        public static float GetSim(
            short[] ki1, short[] kx1, short[] ky1,
            short[] ki2, short[] kx2, short[] ky2)
        {
            var matchmax = GetMatch(ki1, ki2);
            var simmax = (float)matchmax / ki1.Length;
            var v1 = GetRandomVector(ki1, kx1, ky1);
            if (v1.Length < AppConsts.MinDescriptors) {
                return simmax;
            }

            for (var i = 0; i < AppConsts.MaxAttempts; i++) {
                var v2 = GetRandomVector(ki2, kx2, ky2);
                if (v2.Length < AppConsts.MinDescriptors) {
                    continue;
                }

                var match = GetMatch(v1, v2);
                var sim = (float)match / ki1.Length;
                if (sim > simmax) {
                    simmax = sim;
                }
            }

            return simmax;
        }

        public static float GetSim(
            short[] ki1, short[] kx1, short[] ky1,
            short[] ki2, short[] kx2, short[] ky2,
            short[] ki2mirror, short[] kx2mirror, short[] ky2mirror)
        {
            var sim1 = GetSim(ki1, kx1, ky1, ki2, kx2, ky2);
            var sim1mirror = GetSim(ki1, kx1, ky1, ki2mirror, kx2mirror, ky2mirror);
            var sim = Math.Max(sim1, sim1mirror);
            return sim;
        }
    }
}