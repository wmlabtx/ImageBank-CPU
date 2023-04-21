using OpenCvSharp.Dnn;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System;
using OpenCvSharp;

namespace ImageBank
{
    public static class VggHelper
    {
        private static Net _net;

        public static void LoadNet(IProgress<string> progress)
        {
            progress?.Report($"Loading net{AppConsts.CharEllipsis}");
            _net = CvDnn.ReadNetFromOnnx(AppConsts.FileVgg);
        }

        public static byte[] CalculateVector(Bitmap bitmap)
        {
            byte[] vector;
            using (var input = new Mat(new int[] { 1, 3, 224, 224 }, MatType.CV_32F))
            using (var b = BitmapHelper.ScaleAndCut(bitmap, 224, 224 / 16)) {
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

                        float red = (rbyte / 255f  - 0.485f) / 0.229f;
                        float green = (gbyte / 255 - 0.456f) / 0.224f;
                        float blue = (bbyte / 255 - 0.406f) / 0.225f;

                        input.At<float>(0, 0, y, x) = red;
                        input.At<float>(0, 1, y, x) = green;
                        input.At<float>(0, 2, y, x) = blue;
                    }

                    offsety += stride;
                }

                _net.SetInput(input);
                var output = _net.Forward("onnx_node!resnetv27_flatten0_reshape0");
                output.GetArray(out float[] buffer);
                vector = new byte[buffer.Length];
                for (var i = 0; i < buffer.Length; i++) {
                    vector[i] = (byte)Math.Min(255, (int)(buffer[i] * 255.0 / 10.0));
                }
            }

            return vector;
        }

        public static float GetDistance(byte[] x, byte[] y)
        {
            if (x.Length == 0 || y.Length == 0 || x.Length != y.Length) {
                return 1f;
            }

            double dot = 0.0;
            double magx = 0.0;
            double magy = 0.0;
            for (int n = 0; n < x.Length; n++) {
                if (x[n] > 0 || y[n] > 0) {
                    dot += (double)x[n] * y[n] / (255.0 * 255.0);
                    magx += (double)x[n] * x[n] / (255.0 * 255.0);
                    magy += (double)y[n] * y[n] / (255.0 * 255.0);
                }
            }

            return 1f - (float)(dot / (Math.Sqrt(magx) * Math.Sqrt(magy)));
        }
    }
}
