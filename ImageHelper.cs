using OpenCvSharp;
using OpenCvSharp.Dnn;
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
        private static readonly CryptoRandom _random;
        private static readonly Net _model;

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
            _model = CvDnn.ReadNetFromOnnx(AppConsts.FileModel);
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

        public static void ComputeVector(Bitmap bitmap, out float[] vector)
        {
            vector = null;
            using (var matsource = bitmap.ToMat()) {
                var size = new OpenCvSharp.Size(224, 224);
                using (var blob = CvDnn.BlobFromImage(image: matsource, size: size, crop: true)) {
                    _model.SetInput(blob);
                    using (var result = _model.Forward("resnetv27_flatten0_reshape0")) {
                        result.GetArray<float>(out var rawvector);
                        vector = rawvector;
                    }
                }
            }
        }

        public static void ComputeVector(Bitmap bitmap, out float[] vector, out float[] vectormirror)
        {
            ComputeVector(bitmap, out vector);
            using (var brft = new Bitmap(bitmap)) {
                brft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                using (var brft24 = RepixelBitmap(brft)) {
                    ComputeVector(brft24, out vectormirror);
                }
            }
        }

        public static float GetCosineSimilarity(float[] v1, float[] v2)
        {
            var dot = 0f;
            var mag1 = 0f;
            var mag2 = 0f;
            for (var i = 0; i < v1.Length; i++) {
                dot += v1[i] * v2[i];
                mag1 += (float)Math.Pow(v1[i], 2);
                mag2 += (float)Math.Pow(v2[i], 2);
            }

            return (float)(dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2)));
        }

        public static float GetCosineSimilarity(float[] v1, float[] v2, float[] v2mirror)
        {
            var sim1 = GetCosineSimilarity(v1, v2);
            var sim1mirror = GetCosineSimilarity(v1, v2mirror);
            var sim = Math.Max(sim1, sim1mirror);
            return sim;
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
                                            Y = (short)Math.Round(keypoints[k].Pt.Y),
                                            Angle = (short)Math.Round(keypoints[k].Angle),
                                            Size = (short)Math.Round(keypoints[k].Size)
                                        };
                                }
                            }
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

        public static RandomVector GetRandomVector(FeaturePoint[] fp)
        {
            if (fp.Length < AppConsts.MinDescriptors) {
                throw new ArgumentOutOfRangeException(nameof(fp));
            }

            var j = _random.NextShort(0, (short)(fp.Length - 1));
            var list = new List<Tuple<FeaturePoint, long>>();
            for (var i = 0; i < fp.Length; i++) {
                var dx = (long)(fp[i].X - fp[j].X);
                var dy = (long)(fp[i].Y - fp[j].Y);
                var distance = (dx * dx) + (dy * dy);
                list.Add(new Tuple<FeaturePoint, long>(fp[i], distance));
            }

            var randompoints = (int)_random.NextShort((short)(fp.Length / 2), (short)(fp.Length - 1));
            var maxpoints = Math.Max(AppConsts.MinDescriptors, randompoints);
            var rv = new RandomVector {
                Vector = list.OrderBy(e => e.Item2).Take(maxpoints).Select(e => e.Item1.Id).OrderBy(e => e).ToArray()
            };

            return rv;
        }

        public static int GetMatch(short[] ki1, short[] ki2)
        {
            var m = 0;
            var i = 0;
            var j = 0;
            while (i < ki1.Length && j < ki2.Length) {
                if (ki1[i] == ki2[j]) {
                    m++;
                    i++;
                    j++;
                }
                else {
                    if (ki1[i] < ki2[j]) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            return m;
        }

        public static short CalculateDifferenceBetweenAngles(short a, short b)
        {
            int phi = Math.Abs(b - a) % 360;
            int distance = phi > 180 ? 360 - phi : phi;
            return (short)distance;
        }

        public static int GetMatch(FeaturePoint[] a, FeaturePoint[] b)
        {
            var match = 0;
            for (var i = 0; i < a.Length; i++) {
                for (var j = 0; j < b.Length; j++) {
                    if (a[i].Id != b[j].Id) {
                        continue;
                    }

                    var diff = CalculateDifferenceBetweenAngles(a[i].Angle, b[j].Angle);
                    if (diff > 36) {
                        continue;
                    }

                    match++;
                    break;
                }
            }

            return match;
        }

        public static float GetSim(RandomVector rv1, RandomVector rv2)
        {
            if (rv1.Vector == null || rv2.Vector == null || rv1.Vector.Length == 0 || rv2.Vector.Length == 0) {
                return 0f;
            }

            var match = GetMatch(rv1.Vector, rv2.Vector);
            var sim = (float)match / rv1.Vector.Length;
            return sim;
        }

        public static float GetSim(RandomVector rv1, RandomVector rv2, RandomVector rv2mirror)
        {
            var sim1 = GetSim(rv1, rv2);
            var sim1mirror = GetSim(rv1, rv2mirror);
            var sim = Math.Max(sim1, sim1mirror);
            return sim;
        }

        public static FeaturePoint[] ToFeaturePoints(short[] ki, short[] kx, short[] ky, short[] ka, short[] ks)
        {
            var fp = new FeaturePoint[ki.Length];
            for (var i = 0; i < ki.Length; i++) {
                fp[i] = new FeaturePoint() { Id = ki[i], X = kx[i], Y = ky[i], Angle = ka[i], Size = ks[i] };
            }

            return fp;
        }

        public static void FromFeaturePoints(FeaturePoint[] fp, out short[] ki, out short[] kx, out short[] ky, out short[] ka, out short[] ks)
        {
            ki = fp.Select(e => e.Id).ToArray();
            kx = fp.Select(e => e.X).ToArray();
            ky = fp.Select(e => e.Y).ToArray();
            ka = fp.Select(e => e.Angle).ToArray();
            ks = fp.Select(e => e.Size).ToArray();
        }
    }
}