using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageBank
{
    public class MHash
    {
        private readonly ColorLAB[] _averagecolor;

        public MHash(byte[] imagedata)
        {
            if (imagedata.Length < 3 * sizeof(float)) {
                _averagecolor = Array.Empty<ColorLAB>();
                return;
            }

            if (imagedata.Length == 3 * sizeof(float)) {
                var farray = new float[3];
                Buffer.BlockCopy(imagedata, 0, farray, 0, imagedata.Length);
                _averagecolor = new ColorLAB[1];
                _averagecolor[0] = new ColorLAB(farray[0], farray[1], farray[2]);
                return;
            }

            int width;
            int height;
            int stride;
            byte[] data;
            using (var bitmapsource = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmapsource == null) {
                    return;
                }

                using (Bitmap bitmap = new Bitmap(256, 256, PixelFormat.Format24bppRgb)) {
                    using (Graphics g = Graphics.FromImage(bitmap)) {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bitmapsource, 0, 0, 256, 256);
                    }

                    width = bitmap.Width;
                    height = bitmap.Height;
                    BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    stride = bitmapdata.Stride;
                    data = new byte[Math.Abs(bitmapdata.Stride * bitmapdata.Height)];
                    Marshal.Copy(bitmapdata.Scan0, data, 0, data.Length);
                    bitmap.UnlockBits(bitmapdata);
                }
            }

            var lsum = 0f;
            var asum = 0f;
            var bsum = 0f;
            var offsety = 0;
            for (var y = 0; y < height; y++) {
                var offsetx = offsety;
                for (var x = 0; x < width; x++) {
                    var r = data[offsetx + 2];
                    var g = data[offsetx + 1];
                    var b = data[offsetx];
                    var colorRGB = new ColorRGB(r, g, b);
                    var colorLAB = new ColorLAB(colorRGB);
                    lsum += colorLAB.L;
                    asum += colorLAB.A;
                    bsum += colorLAB.B;
                    offsetx += 3;
                }

                offsety += stride;
            }

            lsum /= width * height;
            asum /= width * height;
            bsum /= width * height;
            _averagecolor = new ColorLAB[1];
            _averagecolor[0] = new ColorLAB(lsum, asum, bsum);
        }

        public float ManhattanDistance(MHash other)
        {
            if (_averagecolor.Length == 0 || other._averagecolor.Length == 0) {
                return 1000f;
            }

            var distance = _averagecolor[0].CIEDE2000(other._averagecolor[0]);
            return distance;
        }

        public byte[] ToArray()
        {
            if (_averagecolor.Length == 0) {
                return Array.Empty<byte>();
            }

            var farray = new float[3];
            farray[0] = _averagecolor[0].L;
            farray[1] = _averagecolor[0].A;
            farray[2] = _averagecolor[0].B;
            var array = new byte[3 * sizeof(float)];
            Buffer.BlockCopy(farray, 0, array, 0, array.Length);
            return array;
        }
    }
}
