using ConvNetCS;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageBank
{
    public static class VggHelper
    {
        private static Network _network;

        public static void LoadNetwork()
        {
            _network = Network.Load(AppConsts.FileVgg);
        }

        public static float[] CalculateVector(Bitmap bitmap)
        {
            const int DIM = 224;
            var vol = new Vol(DIM, DIM, 3, 0.0f);
            using (var b = BitmapHelper.ScaleAndCut(bitmap, DIM, DIM / 16)) {
                //b.Save("bitmap.png", ImageFormat.Png);
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

                        //VGG16 required normalization
                        float red = rbyte - 123.68f;
                        float green = gbyte - 116.779f;
                        float blue = bbyte - 103.939f;

                        vol.Set(x, y, 0, red);
                        vol.Set(x, y, 1, green);
                        vol.Set(x, y, 2, blue);
                    }

                    offsety += stride;
                }
            }

            _network.Forward(vol, true);
            var vector = _network.Layers[32].Output.W;
            return vector;
        }


        public static float GetDistance(float[] x, float[] y)
        {
            if (x.Length == 0 || y.Length == 0 || x.Length != y.Length) {
                return 1f;
            }

            double dot = 0.0;
            double magx = 0.0;
            double magy = 0.0;
            for (int n = 0; n < x.Length; n++) {
                dot += x[n] * y[n];
                magx += x[n] * x[n];
                magy += y[n] * y[n];
            }

            return 1f - (float)(dot / (Math.Sqrt(magx) * Math.Sqrt(magy)));
        }
    }
}
