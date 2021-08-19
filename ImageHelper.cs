using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private static readonly CryptoRandom _random;
        //private static readonly SIFT _sift;

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
            //_sift = SIFT.Create(nFeatures:AppConsts.MaxDescriptors);
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

        public static void ComputeFeaturePoints(Bitmap bitmap, out FeaturePoint[] fp)
        {
            fp = null;
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

                            fp = new FeaturePoint[keypoints.Length];
                            for (var i = 0; i < idx.Length; i++) {
                                for (var j = 0; j < idx[i].Length; j++) {
                                    var k = idx[i][j];
                                    fp[k] =
                                        new FeaturePoint() {
                                            Id = (short)i,
                                            X = (short)Math.Round(keypoints[k].Pt.X),
                                            Y = (short)Math.Round(keypoints[k].Pt.Y)
                                        };
                                }
                            }

                            fp = fp.OrderBy(e => e.Id).ToArray();
                        }
                    }
                }
            }
        }

        private static byte Quant(float fval)
        {
            if (fval < -0.0356f) return 0;
            if (fval < -0.0060f) return 1;
            if (fval < 0.0096f) return 2;
            if (fval < 0.0265f) return 3;
            if (fval < 0.0265f) return 4;
            if (fval < 0.0456f) return 5;
            if (fval < 0.0693f) return 6;
            if (fval < 0.1015f) return 7;
            if (fval < 0.1455f) return 8;
            if (fval < 0.2134f) return 9;
            return 10;
        }

        public static void ComputeFeaturePoints2(Bitmap bitmap, out FeaturePoint2[] fp)
        {
            fp = null;
            using (var matsource = bitmap.ToMat())
            using (var matcolor = new Mat()) {
                var f = 480f / Math.Max(matsource.Width, matsource.Height);
                Cv2.Resize(matsource, matcolor, new OpenCvSharp.Size(0, 0), f, f, InterpolationFlags.Area);
                using (var mat = new Mat()) {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _kaze.Detect(mat);
                    if (keypoints.Length > 0) {
                        keypoints = keypoints.OrderByDescending(e => e.Response).Take(AppConsts.MaxDescriptors).ToArray();
                        if (keypoints.Length < AppConsts.MinDescriptors) {
                            return;
                        }

                        using (var matdescriptors = new Mat()) { 
                            _kaze.Compute(mat, ref keypoints, matdescriptors);
                            //_bow.Compute(mat, ref keypoints, matbow, out var idx, matdescriptors);

                            using (var matkeypoints = new Mat()) {
                                Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                matkeypoints.SaveImage("mat.png");
                            }

                            matdescriptors.GetArray<float>(out var fdescriptors);
                            fp = new FeaturePoint2[keypoints.Length];
                            var bdescriptor = new byte[_kaze.DescriptorSize];
                            using (var md5 = MD5.Create()) {
                                for (var i = 0; i < keypoints.Length; i++) {
                                    for (var j = 0; j < _kaze.DescriptorSize; j++) {
                                        var offset = i * _kaze.DescriptorSize + j;
                                        var fval = fdescriptors[offset];
                                        var bval = Quant(fval);
                                        bdescriptor[j] = bval;
                                    }

                                    var hash = md5.ComputeHash(bdescriptor);
                                    var cdescriptor = BitConverter.ToUInt32(hash, 4);
                                    fp[i] =
                                        new FeaturePoint2() {
                                            Id = cdescriptor,
                                            X = (short)Math.Round(keypoints[i].Pt.X),
                                            Y = (short)Math.Round(keypoints[i].Pt.Y)
                                        };
                                }
                            }

                            /*
                            Array.Sort(fdescriptors);
                            var fmin = fdescriptors[0];
                            var fmax = fdescriptors[fdescriptors.Length - 1];
                            var step = fdescriptors.Length / 10;
                            for (var i = step; i < fdescriptors.Length; i += step) {
                                Debug.WriteLine($"{fdescriptors[i]:F4}");
                            }
                            */

                            /*
                            for (var i = 0; i < idx.Length; i++) {
                                for (var j = 0; j < idx[i].Length; j++) {
                                    var k = idx[i][j];
                                    fp[k] =
                                        new FeaturePoint() {
                                            Id = (short)i,
                                            X = (short)Math.Round(keypoints[k].Pt.X),
                                            Y = (short)Math.Round(keypoints[k].Pt.Y)
                                        };
                                }
                            }
                            */

                            fp = fp.OrderBy(e => e.Id).ToArray();
                        }
                    }
                }
            }
        }

        public static void ComputeFeaturePoints(Bitmap bitmap, out FeaturePoint[] fp, out FeaturePoint[] fpmirror)
        {
            ComputeFeaturePoints(bitmap, out fp);
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                ComputeFeaturePoints(brft, out fpmirror);
            }
        }

        public static void ComputeFeaturePoints2(Bitmap bitmap, out FeaturePoint2[] fp, out FeaturePoint2[] fpmirror)
        {
            ComputeFeaturePoints2(bitmap, out fp);
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                ComputeFeaturePoints2(brft, out fpmirror);
            }
        }

        public static short[] GetRandomVector(FeaturePoint[] fp)
        {
            var xmin = fp.Min(e => e.X);
            var xmax = fp.Max(e => e.X);
            var ymin = fp.Min(e => e.Y);
            var ymax = fp.Max(e => e.Y);
            var list = new List<short>();
            while (list.Count < AppConsts.MinDescriptors) {
                list.Clear();
                var x1 = _random.NextShort(xmin, xmax);
                var x2 = _random.NextShort(xmin, xmax);
                if (x2 < x1) {
                    var xtemp = x1;
                    x1 = x2;
                    x2 = xtemp;
                }

                var y1 = _random.NextShort(ymin, ymax);
                var y2 = _random.NextShort(ymin, ymax);
                if (y2 < y1) {
                    var ytemp = y1;
                    y1 = y2;
                    y2 = ytemp;
                }

                foreach (var e in fp) {
                    if (e.X >= x1 && e.Y >= y1 && e.X <= x2 && e.Y <= y2) {
                        list.Add(e.Id);
                    }
                }
            }

            var rv = list.ToArray();
            return rv;
        }

        public static float GetSim(short[] x, FeaturePoint[] fp)
        {
            var m = 0;
            var i = 0;
            var j = 0;
            while (i < x.Length && j < fp.Length) {
                if (x[i] ==fp[j].Id) {
                    m++;
                    i++;
                    j++;
                }
                else {
                    if (x[i] < fp[j].Id) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            var sim = m * 100f / x.Length;
            return sim;
        }

        public static float GetSim2(FeaturePoint2[] x, FeaturePoint2[] y)
        {
            var m = 0;
            var i = 0;
            var j = 0;
            while (i < x.Length && j < y.Length) {
                if (x[i].Id == y[j].Id) {
                    m++;
                    i++;
                    j++;
                }
                else {
                    if (x[i].Id < y[j].Id) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            var sim = m * 100f / x.Length;
            return sim;
        }

        public static float GetSim2(FeaturePoint2[] fp1, FeaturePoint2[] fp2, FeaturePoint2[] fp2mirror)
        {
            var sim1 = GetSim2(fp1, fp2);
            var sim1mirror = GetSim2(fp1, fp2mirror);
            var sim = Math.Max(sim1, sim1mirror);
            return sim;
        }


        public static float GetSim(short[] rv1, FeaturePoint[] fp2, FeaturePoint[] fp2mirror)
        {
            var sim1 = GetSim(rv1, fp2);
            var sim1mirror = GetSim(rv1, fp2mirror);
            var sim = Math.Max(sim1, sim1mirror);
            return sim;
        }

        public static FeaturePoint[] ToFeaturePoints(short[] ki, short[] kx, short[] ky)
        {
            var fp = new FeaturePoint[ki.Length];
            for (var i = 0; i < ki.Length; i++) {
                fp[i] = new FeaturePoint() { Id = ki[i], X = kx[i], Y = ky[i] };
            }

            return fp;
        }

        public static void FromFeaturePoints(FeaturePoint[] fp, out short[] ki, out short[] kx, out short[] ky)
        {
            ki = fp.Select(e => e.Id).ToArray();
            kx = fp.Select(e => e.X).ToArray();
            ky = fp.Select(e => e.Y).ToArray();
        }

        public static short[] Unique(short[] v)
        {
            if (v == null || v.Length == 0) {
                throw new ArgumentNullException(nameof(v));
            }

            var id = new int[16 * 1024];
            for (var i = 0; i < v.Length; i++) {
                id[v[i]]++;
            }

            var u = new List<short>();
            for (var i = 0; i < v.Length; i++) {
                if (id[v[i]] == 1) {
                    u.Add(v[i]);
                }
            }

            return u.ToArray();
        }

        public static FeaturePoint[] Unique(FeaturePoint[] v)
        {
            if (v == null || v.Length == 0) {
                throw new ArgumentNullException(nameof(v));
            }

            var id = new int[16*1024];
            for (var i = 0; i < v.Length; i++) {
                id[v[i].Id]++;
            }

            var u = new List<FeaturePoint>();
            for (var i = 0; i < v.Length; i++) {
                if (id[v[i].Id] == 1) {
                    u.Add(v[i]);
                }
            }

            return u.ToArray();


            /*

            var u = new List<FeaturePoint> {
                v[0]
            };

            for (var i = 1; i < v.Length; i++) {
                if (v[i].Id == v[i - 1].Id) {
                    if (u.Count > 0) {
                        var j = u.Count - 1;
                        if (u[j].Id == v[i].Id) {
                            u.RemoveAt(j);
                        }
                    }
                }
                else {
                    u.Add(v[i]);
                }
            }

            if (u.Count == 0) {
                throw new Exception();
            }

            return u.ToArray();
            */
        }
    }
}