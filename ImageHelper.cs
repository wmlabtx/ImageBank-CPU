using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ImageBank
{
    public static class ImageHelper
    {
        private static readonly ORB _orb = ORB.Create(AppConsts.MaxDescriptorsInImage);
        private static bool GetBitmapFromImageData(byte[] data, out Bitmap bitmap)
        {
            try
            {
                using (var mat = Cv2.ImDecode(data, ImreadModes.AnyColor))
                {
                    bitmap = mat.ToBitmap();
                    return true;
                }
            }
            catch (ArgumentException)
            {
                bitmap = null;
                return false;
            }
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

        private static Bitmap ResizeBitmap(Image bitmap, int width, int height)
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

        public static byte[] GetMatrixFromBitmap(Bitmap bitmap)
        {
            using (var mat = BitmapConverter.ToMat(bitmap))
            {
                mat.GetArray(out byte[] array);
                return array;
            }
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

        public static bool ComputeDescriptors(Bitmap bitmap, out byte[] blob)
        {
            blob = null;

            if (bitmap.Width >= bitmap.Height && bitmap.Height > AppConsts.MaxDim)
            {
                var k = bitmap.Height * 1.0 / AppConsts.MaxDim;
                var width = (int)(bitmap.Width / k);
                bitmap = ResizeBitmap(bitmap, width, AppConsts.MaxDim);
            }
            else
            {
                if (bitmap.Width < bitmap.Height && bitmap.Width > AppConsts.MaxDim)
                {
                    var k = bitmap.Width * 1.0 / AppConsts.MaxDim;
                    var height = (int)(bitmap.Height / k);
                    bitmap = ResizeBitmap(bitmap, AppConsts.MaxDim, height);
                }
            }

            using (var matcolor = bitmap.ToMat())
            {
                using (var mat = new Mat())
                {
                    Cv2.CvtColor(matcolor, mat, ColorConversionCodes.BGR2GRAY);
                    var keypoints = _orb.Detect(mat);
                    using (var matdescriptors = new Mat())
                    {
                        if (keypoints.Length > AppConsts.MaxDescriptorsInImage)
                        {
                            keypoints = keypoints.ToList().Take(AppConsts.MaxDescriptorsInImage).ToArray();
                        }

                        _orb.Compute(mat, ref keypoints, matdescriptors);
                        if (matdescriptors.Rows == 0)
                        {
                            return false;
                        }

                        matdescriptors.GetArray(out byte[] array);
                        var maxblobsize = AppConsts.MaxDescriptorsInImage * _orb.DescriptorSize;
                        var size = Math.Min(maxblobsize, array.Length);
                        blob = new byte[size];
                        Buffer.BlockCopy(array, 0, blob, 0, size);
                    }
                }
            }

            return true;
        }

        public static ulong[] ComputeDescriptors(byte[] blob)
        {
            var length = blob.Length * 4 / _orb.DescriptorSize;
            var descriptors = new ulong[length];
            Buffer.BlockCopy(blob, 0, descriptors, 0, blob.Length);
            return descriptors;
        }

        public static float GetDistance(ulong[] x, ulong[] y, int count = int.MaxValue / 4)
        {
            float distance;
            var m = new List<Tuple<int, int, int>>();
            var xl = Math.Min(x.Length, count * 4);
            var yl = Math.Min(y.Length, count * 4);
            for (var i = 0; i < xl; i += 4)
            {
                for (var j = 0; j < yl; j += 4)
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

            var isum = 0;
            var icount = 0;
            var top = Math.Max(1, count / 4);
            while (icount < top && m.Count > 0)
            {
                isum += m[0].Item3;
                var ix = m[0].Item1;
                var iy = m[0].Item2;
                icount++;
                m.RemoveAll(e => e.Item1 == ix || e.Item2 == iy);
            }

            distance = (float)isum / icount;
            return distance;
        }

        /*
        public static int GetMatch(ushort[] x, ushort[] y)
        {
            var i = 0;
            var j = 0;
            var m = 0;
            while (i < x.Length && j < y.Length)
            {
                if (x[i] == y[j])
                {
                    i++;
                    j++;
                    m++;
                }
                else
                {
                    if (x[i] < y[j])
                    {
                        i++;
                    }
                    else
                    {
                        j++;
                    }
                }
            }

            return m;
        }
        */
    }
}
