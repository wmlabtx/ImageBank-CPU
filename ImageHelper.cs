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

        public static void ComputeBlob(Bitmap bitmap, out ulong phash, out ulong[] descriptors)
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
                    using (var phashcalculator = PHash.Create())
                    using (var matphash = new Mat())
                    {
                        phashcalculator.Compute(mat, matphash);
                        matphash.GetArray(out byte[] phashbuffer);
                        phash = BitConverter.ToUInt64(phashbuffer, 0);
                    }

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
                        //keypoints = keypoints.OrderByDescending(e => e.Response).Take(MAXDESCRIPTORS).ToArray();

                        using (var matdescriptors = new Mat())
                        {
                            _orb.Compute(mat, ref keypoints, matdescriptors);
                            if (matdescriptors.Rows > 0 && keypoints.Length > 0)
                            {
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

        public static float CompareBlob(ulong[] x, ulong[] y)
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
            var sum = 0f;
            var w = 1f;
            var sumw = 0f;
            while (m.Count > 0)
            {
                sum += m[0].Item3 * w;
                sumw += w;
                w /= 2f;

                var ix = m[0].Item1;
                var iy = m[0].Item2;
                m.RemoveAll(e => e.Item1 == ix || e.Item2 == iy);
            }

            var f = (float)sum / sumw;
            return f;
        }
    }
}