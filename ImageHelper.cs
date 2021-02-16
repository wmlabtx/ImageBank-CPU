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
        const int MAXDESCRIPTORS = 250;
        const int PIESES = 4;
        private static readonly FastFeatureDetector _fast = FastFeatureDetector.Create();
        private static readonly ORB _orb = ORB.Create();

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

        public static void ComputeBlob(Bitmap bitmap, out ulong phash, out byte[] map, out ulong[] descriptors)
        {
            descriptors = null;
            map = null;
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

                    var keypoints = _fast.Detect(mat);
                    if (keypoints.Length > 0)
                    {
                        keypoints = keypoints.OrderByDescending(e => e.Octave).ThenByDescending(e => e.Response).Take(MAXDESCRIPTORS).ToArray();
                        using (var matdescriptors = new Mat())
                        {
                            _orb.Compute(mat, ref keypoints, matdescriptors);
                            if (matdescriptors.Rows > 0 && keypoints.Length > 0)
                            {
                                using (var matkeypoints = new Mat())
                                {
                                    Cv2.DrawKeypoints(matcolor, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                                    matkeypoints.SaveImage("matkeypoints.png");
                                }

                                matdescriptors.GetArray(out byte[] array);
                                descriptors = ImageHelper.ArrayTo64(array);
                                map = new byte[keypoints.Length];
                                for (var i = 0; i < keypoints.Length; i++)
                                {
                                    var ix = (int)(keypoints[i].Pt.X * PIESES / mat.Width);
                                    var iy = (int)(keypoints[i].Pt.Y * PIESES / mat.Height);
                                    map[i] = (byte)(iy * PIESES + ix);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static float CompareBlob(byte[] m1, ulong[] d1, byte[] m2, ulong[] d2)
        {
            const int MAXDISTANCE = 256;
            var minhamming = new int[PIESES * PIESES];
            for (var i = 0; i < minhamming.Length; i++)
            {
                minhamming[i] = MAXDISTANCE;
            }

            for (var i = 0; i < m1.Length; i++)
            {
                for (var j = 0; j < m2.Length; j++)
                {
                    if (m1[i] != m2[j])
                    {
                        continue;
                    }

                    var hamming = 0;
                    for (var b = 0; b < 4; b++)
                    {
                        hamming += Intrinsic.PopCnt(d1[i * 4 + b] ^ d2[j * 4 + b]);
                    }

                    if (hamming < minhamming[m1[i]])
                    {
                        minhamming[m1[i]] = hamming;
                    }
                }
            }

            var distances = minhamming.Where(e => e != MAXDISTANCE).OrderBy(e => e).ToArray();
            if (distances.Length == 0)
            {
                return AppConsts.MaxDistance;
            }

            var bestcount = Math.Max(1, distances.Length / 4);
            var p = 0;
            var sum1 = 0f;
            var cnt1 = 0;
            while(p < bestcount)
            {
                sum1 += distances[p];
                cnt1++;
                p++;
            }

            var sum2 = 0f;
            var cnt2 = 0;
            while(p < distances.Length)
            {
                sum2 += distances[p];
                cnt2++;
                p++;
            }

            var distance = 0f;
            if (cnt2 == 0)
            {
                distance = sum1 / cnt1;
                
            }
            else
            {
                distance = ((sum1 * 9f / cnt1) + (sum2 / cnt2)) / 10f;
            }
            
            return distance;
        }

        public static int CompareGray(byte[] g1, byte[] g2)
        {
            var diff = new int[16];
            for (var i = 0; i < 16; i++)
            {
                diff[i] = Math.Abs((g1[i] >> 6) - (g2[i] >> 6));
            }

            var sum = diff.OrderBy(e => e).Take(4).Sum();
            return sum;
        }
    }
}
