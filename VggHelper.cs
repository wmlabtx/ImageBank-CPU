using ConvNetCS;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageBank
{
    public static class VggHelper
    {
        private static readonly float[] _quantvector = new float[] {
            -135.9650f,-44.0282f,-39.5161f,-36.8418f,-34.9228f,-33.4218f,-32.1846f,-31.1279f,-30.2048f,-29.3827f,-28.6422f,-27.9676f,-27.3475f,-26.7724f,-26.2370f,-25.7355f,-25.2633f,-24.8168f,-24.3937f,-23.9907f,-23.6054f,-23.2368f,-22.8828f,-22.5436f,-22.2174f,-21.9017f,-21.5972f,-21.3033f,-21.0178f,-20.7404f,-20.4718f,-20.2095f,-19.9549f,-19.7067f,-19.4648f,-19.2286f,-18.9981f,-18.7728f,-18.5520f,-18.3361f,-18.1246f,-17.9176f,-17.7142f,-17.5148f,-17.3191f,-17.1266f,-16.9374f,-16.7517f,-16.5689f,-16.3889f,-16.2117f,-16.0370f,-15.8651f,-15.6958f,-15.5290f,-15.3643f,-15.2019f,-15.0417f,-14.8834f,-14.7270f,-14.5728f,-14.4202f,-14.2692f,-14.1199f,-13.9724f,-13.8264f,-13.6821f,-13.5394f,-13.3982f,-13.2586f,-13.1203f,-12.9829f,-12.8472f,-12.7124f,-12.5791f,-12.4471f,-12.3160f,-12.1862f,-12.0575f,-11.9298f,-11.8031f,-11.6777f,-11.5527f,-11.4289f,-11.3062f,-11.1841f,-11.0627f,-10.9422f,-10.8224f,-10.7036f,-10.5856f,-10.4684f,-10.3519f,-10.2362f,-10.1211f,-10.0066f,-9.8927f,-9.7795f,-9.6671f,-9.5550f,-9.4436f,-9.3330f,-9.2226f,-9.1126f,-9.0031f,-8.8942f,-8.7859f,-8.6780f,-8.5707f,-8.4637f,-8.3572f,-8.2511f,-8.1453f,-8.0400f,-7.9351f,-7.8306f,-7.7261f,-7.6224f,-7.5190f,-7.4158f,-7.3128f,-7.2102f,-7.1079f,-7.0057f,-6.9039f,-6.8023f,-6.7010f,-6.5998f,-6.4991f,-6.3984f,-6.2979f,-6.1974f,-6.0972f,-5.9972f,-5.8974f,-5.7975f,-5.6979f,-5.5985f,-5.4991f,-5.3998f,-5.3004f,-5.2014f,-5.1023f,-5.0034f,-4.9043f,-4.8056f,-4.7068f,-4.6079f,-4.5090f,-4.4100f,-4.3112f,-4.2121f,-4.1131f,-4.0141f,-3.9150f,-3.8158f,-3.7165f,-3.6170f,-3.5173f,-3.4177f,-3.3177f,-3.2177f,-3.1174f,-3.0170f,-2.9166f,-2.8161f,-2.7153f,-2.6142f,-2.5129f,-2.4111f,-2.3091f,-2.2069f,-2.1042f,-2.0013f,-1.8981f,-1.7945f,-1.6904f,-1.5859f,-1.4814f,-1.3763f,-1.2705f,-1.1643f,-1.0574f,-0.9501f,-0.8423f,-0.7340f,-0.6249f,-0.5154f,-0.4053f,-0.2941f,-0.1827f,-0.0702f,0.0429f,0.1568f,0.2715f,0.3871f,0.5037f,0.6213f,0.7397f,0.8596f,0.9804f,1.1021f,1.2252f,1.3494f,1.4752f,1.6022f,1.7305f,1.8601f,1.9911f,2.1243f,2.2589f,2.3953f,2.5335f,2.6737f,2.8162f,2.9606f,3.1074f,3.2568f,3.4086f,3.5629f,3.7204f,3.8810f,4.0445f,4.2116f,4.3824f,4.5572f,4.7361f,4.9195f,5.1078f,5.3016f,5.5009f,5.7057f,5.9179f,6.1364f,6.3628f,6.5976f,6.8428f,7.0976f,7.3640f,7.6439f,7.9387f,8.2507f,8.5814f,8.9338f,9.3125f,9.7215f,10.1682f,10.6609f,11.2121f,11.8381f,12.5678f,13.4462f,14.5595f,16.0949f,18.6595f
        };

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

        public static float GetVectorHash(float[] x)
        {
            var hash = 0f;
            for (var i = 0; i < 4096; i++) {
                hash += x[i];
            }

            hash /= 4096f;

            /*
            var doffset = 0;
            var soffset = 0;
            while (doffset < x.Length) {
                hash[soffset] = 0f;
                for (var i = 0; i < 16; i++) {
                    hash[soffset] += x[doffset + i];
                }

                hash[soffset] /= 16;
                soffset++;
                doffset += 16;
            }
            */

            return hash;
        }


        public static byte[] QuantVector(float[] x)
        {
            var quant = new byte[x.Length];
            for (var i = 0; i < x.Length; i++) {
                byte q;
                if (x[i] >= _quantvector[254]) {
                    q = 0xFF;
                }
                else {
                    var j = 0;
                    while (x[i] >= _quantvector[j]) {
                        j++;
                    }

                    q = (byte)j;
                }

                quant[i] = q;
            }

            return quant;
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

        /*
        public static int GetDistance(byte[] x, byte[] y)
        {
            if (x.Length == 0 || y.Length == 0 || x.Length != y.Length) {
                return x.Length * 256;
            }

            var diff = 0;
            for (int n = 0; n < x.Length; n++) {
                diff += Math.Abs(x[n] - y[n]);
            }

            return diff;
        }
        */

        public static float GetDistance(byte[] x, byte[] y)
        {
            if (x.Length == 0 || y.Length == 0 || x.Length != y.Length) {
                return 1f;
            }

            double dot = 0.0;
            double magx = 0.0;
            double magy = 0.0;
            for (int n = 0; n < x.Length; n++) {
                dot += (double)x[n] * y[n] / (255.0 * 255.0);
                magx += (double)x[n] * x[n] / (255.0 * 255.0);
                magy += (double)y[n] * y[n] / (255.0 * 255.0);
            }

            return 1f - (float)(dot / (Math.Sqrt(magx) * Math.Sqrt(magy)));
        }
    }
}
