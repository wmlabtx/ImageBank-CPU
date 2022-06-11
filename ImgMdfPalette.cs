using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private const int DIM = 320;
        private const int BORDER = 32;
        private const float EV = 0.000001f;

        private static float[] _palette;

        public static void InitPalette()
        {
            _palette = new float[256 * 3];
            for (var i = 0; i < 256; i++) {
                BitmapHelper.ToLAB((byte)i, (byte)i, (byte)i, out float lfloat, out float afloat, out float bfloat);
                _palette[i * 3] = lfloat;
                _palette[i * 3 + 1] = afloat;
                _palette[i * 3 + 2] = bfloat;
            }

            DrawPalette();
            SavePalette();
        }

        public static void LearnPalette()
        {
            InitPalette();
            var counter = 0;
            foreach (var img in _imgList) {
                var filename = FileHelper.NameToFileName(img.Value.Name);
                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    continue;
                }

                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    if (bitmap == null) {
                        continue;
                    }

                    ComputePalette(bitmap);
                }

                counter++;
                Debug.WriteLine(counter);
            }
        }

        public static float GetDistance2(int p, float[] vector)
        {
            var dl = _palette[p * 3] - vector[0];
            var da = _palette[p * 3 + 1] - vector[1];
            var db = _palette[p * 3 + 2] - vector[2];
            return dl * dl + da * da + db * db;
        }

        public static void MoveToward(int p, float[] destination, float k)
        {
            for (var i = 0; i < 3; i++) {
                _palette[p * 3 + i] = (_palette[p * 3 + i] * (1 - k)) + (destination[i] * k);
            }
        }

        private static void FindColors(float[] vector, out int v, out float vd, out int w, out float wd)
        {
            v = 0;
            vd = float.MaxValue;
            w = 0;
            wd = float.MaxValue;
            for (var i = 0; i < 256; i++) {
                var distance = GetDistance2(i, vector);
                if (distance < vd) {
                    w = v;
                    wd = vd;
                    v = i;
                    vd = distance;
                }
                else {
                    if (distance < wd) {
                        w = i;
                        wd = distance;
                    }
                }
            }
        }

        public static void LearnPalette(float[] vector)
        {
            FindColors(vector, out int v, out float vd, out int w, out float wd);
            MoveToward(v, vector, EV);
            var ew = EV * vd * vd / (wd * wd);
            MoveToward(w, vector, ew);
        }

        public static void DrawPalette()
        {
            using (Bitmap bitmap = new Bitmap(16 * 16, 16 * 16, PixelFormat.Format24bppRgb))
            using (Graphics g = Graphics.FromImage(bitmap)) {
                for (var y = 0; y < 16; y++) {
                    for (var x = 0; x < 16; x++) {
                        var p = y * 16 + x;
                        BitmapHelper.ToRGB(_palette[p * 3], _palette[p * 3 + 1], _palette[p * 3 + 2], out byte rbyte, out byte gbyte, out byte bbyte);
                        var rect = new Rectangle(x * 16, y * 16, 16, 16);
                        var color = Color.FromArgb(rbyte, gbyte, bbyte);
                        using (var brush = new SolidBrush(color)) {
                            g.FillRectangle(brush, rect);
                        }

                        p += 3;
                    }
                }

                bitmap.Save(AppConsts.FilePalette, ImageFormat.Png);
            }
        }

        public static float[] ComputePalette(Bitmap bitmap)
        {
            var hist = new float[256];
            using (var b = BitmapHelper.ScaleAndCut(bitmap, DIM, BORDER)) {
                //b.Save("b.png", ImageFormat.Png);
                var bitmapdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var stride = bitmapdata.Stride;
                var data = new byte[Math.Abs(bitmapdata.Stride * bitmapdata.Height)];
                Marshal.Copy(bitmapdata.Scan0, data, 0, data.Length);
                b.UnlockBits(bitmapdata);
                var offsety = 0;
                for (var y = 0; y < b.Height; y++) {
                    var offsetx = offsety;
                    for (var x = 0; x < b.Width; x++) {
                        var rbyte = data[offsetx + 2];
                        var gbyte = data[offsetx + 1];
                        var bbyte = data[offsetx];
                        offsetx += 3;
                        BitmapHelper.ToLAB(rbyte, gbyte, bbyte, out float lfloat, out float afloat, out float bfloat);
                        var vector = new float[3] { lfloat, afloat, bfloat };
                        LearnPalette(vector);
                        FindColors(vector, out int v, out float vd, out int w, out float wd);
                        if (vd < 0.75f * wd) {
                            hist[v]++;
                        }
                    }

                    offsety += stride;
                }
            }

            DrawPalette();
            SavePalette();

            var sum = hist.Sum();
            for (var i = 0; i < 256; i++) {
                hist[i] = (float)Math.Sqrt(hist[i] / sum);
            }

            return hist;
        }

        public static float GetDistance(float[] x, float[] y)
        {
            if (x.Length == 0 || y.Length == 0 || x.Length != y.Length) {
                return 1f;
            }

            var sum = 0f;
            for (var i = 0; i < x.Length; i++) {
                sum += x[i] * y[i];
            }

            var sim = (float)Math.Sqrt(1f - sum);
            return sim;
        }
    }
}
