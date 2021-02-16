using System;
using System.Drawing;

namespace ImageBank
{
    public static class HcHelper
    {
        /*
        private static readonly double[,] COSIN = 
            {
                { 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01 },
                { 4.903926e-01, 4.157348e-01, 2.777851e-01, 9.754516e-02, -9.754516e-02, -2.777851e-01, -4.157348e-01, -4.903926e-01 },
                { 4.619398e-01, 1.913417e-01, -1.913417e-01, -4.619398e-01, -4.619398e-01, -1.913417e-01, 1.913417e-01, 4.619398e-01 },
                { 4.157348e-01, -9.754516e-02, -4.903926e-01, -2.777851e-01, 2.777851e-01, 4.903926e-01, 9.754516e-02, -4.157348e-01 },
                { 3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01, 3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01 },
                { 2.777851e-01, -4.903926e-01, 9.754516e-02, 4.157348e-01, -4.157348e-01, -9.754516e-02, 4.903926e-01, -2.777851e-01 },
                { 1.913417e-01, -4.619398e-01, 4.619398e-01, -1.913417e-01, -1.913417e-01, 4.619398e-01, -4.619398e-01, 1.913417e-01 },
                { 9.754516e-02, -2.777851e-01, 4.157348e-01, -4.903926e-01, 4.903926e-01, -4.157348e-01, 2.777851e-01, -9.754516e-02 }
            };


        private static readonly int[] zigzag = {
            0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
            12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
        };

        private static byte Yquant(float f)
        {
            var i = (int)f;
            int j;
            if (i > 192)
            {
                j = 112 + ((i - 192) >> 2);
            }
            else if (i > 160)
            {
                j = 96 + ((i - 160) >> 1);
            }
            else if (i > 96)
            {
                j = 32 + (i - 96);
            }
            else if (i > 64)
            {
                j = 16 + ((i - 64) >> 1);
            }
            else
            {
                j = i >> 2;
            }

            return (byte)(j >> 1);
        }

        private static byte Cquant(float f)
        {
            var i = (int)f;
            int j;
            if (i > 191)
            {
                j = 63;
            }
            else if (i > 160)
            {
                j = 56 + ((i - 160) >> 2);
            }
            else if (i > 144)
            {
                j = 48 + ((i - 144) >> 1);
            }
            else if (i > 112)
            {
                j = 16 + (i - 112);
            }
            else if (i > 96)
            {
                j = 8 + ((i - 96) >> 1);
            }
            else if (i > 64)
            {
                j = (i - 64) >> 2;
            }
            else
            {
                j = 0;
            }

            return (byte)j;
        }

        private static byte Aquant(float f)
        {
            var i = (int)f;
            int j;
            if (i > 255)
            {
                i = 255;
            }

            if (i < -256)
            {
                i = -256;
            }

            if ((Math.Abs(i)) > 127)
            {
                j = 64 + ((Math.Abs(i)) >> 2);
            }
            else if ((Math.Abs(i)) > 63)
            {
                j = 32 + ((Math.Abs(i)) >> 1);
            }
            else
            {
                j = Math.Abs(i);
            }

            j = (i < 0) ? -j : j;
            j += 128;

            return (byte)(j >> 3);
        }

        public static byte[] Compute(Bitmap bitmap)
        {
            var hc = new byte[64 * 3];
            using (var matsource = bitmap.ToMat())
            using (var mat10x10 = matsource.Resize(new OpenCvSharp.Size(10, 10), 0, 0, InterpolationFlags.Cubic))
            using (var matycc = new Mat())
            {
                Cv2.CvtColor(mat10x10, matycc, ColorConversionCodes.BGR2YCrCb);
                Cv2.Split(matycc, out var matc);
                var offset = 0;
                for (var channel = 0; channel < 3; channel++)
                {
                    using (var mat8x8 = matc[channel].Clone(new Rect(1, 1, 8, 8)))
                    using (var mat8x8f = new Mat())
                    {
                        mat8x8.ConvertTo(mat8x8f, MatType.CV_32F);
                        float[] fdata = null;
                        using (var dct = mat8x8f.Dct())
                        {
                            dct.SaveImage("dct.png");
                            dct.GetArray(out fdata);
                        }

                        if (channel == 0)
                        {
                            hc[offset + 0] = Yquant(fdata[0] / 8);
                        }
                        else
                        {
                            hc[offset + 0] = Cquant(fdata[0] / 8);
                        }

                        for (var i = 1; i < 64; i++)
                        {
                            if (channel == 0)
                            {
                                hc[offset + i] = Aquant(fdata[zigzag[i]] / 2);
                            }
                            else
                            {
                                hc[offset + i] = Aquant(fdata[zigzag[i]]);
                            }
                        }
                    }

                    offset += 64;
                }
            }


            var shape = new int[3, 64];
            const int DIM = 10;
            using (var b10x10 = ImageHelper.ResizeBitmap(bitmap, DIM, DIM))
            using (var matcolor = b10x10.ToMat())
            {



                matcolor.GetArray(out byte[] matrix);
                var offset = 0;
                var doffset = 0;
                for (var y = 0; y < DIM; y++)
                {
                    for (var x = 0; x < DIM; x++)
                    {
                        if (y != 0 && y != DIM - 1 && x != 0 && x != DIM - 1)
                        {
                            var b = matrix[offset];
                            var r = matrix[offset + 1];
                            var g = matrix[offset + 2];
                            var cy = (0.299 * r) + (0.587 * g) + (0.114 * b);
                            var cb = (b - cy) * 0.564 + 128.0;
                            var cr = (r - cy) * 0.713 + 128.0;
                            shape[0, doffset] = (int)Math.Floor(cy);
                            shape[1, doffset] = (int)Math.Floor(cb);
                            shape[2, doffset] = (int)Math.Floor(cr);
                            doffset++;
                        }

                        offset += 3;
                    }
                }
            }

            

            var dstage = new int[3, 64];
            for (var channel = 0; channel < 3; channel++)
            {
                var dct = new double[64];
                for (var i = 0; i < 8; i++)
                {
                    for (var j = 0; j < 8; j++)
                    {
                        var s = 0.0;
                        for (var k = 0; k < 8; k++)
                        {
                            s += COSIN[j, k] * shape[channel, 8 * i + k];
                        }
                            
                        dct[8 * i + j] = s;
                    }
                }

                for (var j = 0; j < 8; j++)
                {
                    for (var i = 0; i < 8; i++)
                    {
                        var s = 0.0;
                        for (var k = 0; k < 8; k++)
                        {
                            s += COSIN[i, k] * dct[8 * k + j];
                        }

                        dstage[channel, 8 * i + j] = (int)Math.Floor(s + 0.4999);
                    }
                }
            }


            return hc;
        }

        public static float Distance(byte[] x, byte[] y)
        {
            var sum = new int[3];
            int diff;
            int weight;
            for (var i = 0; i < 64; i++)
            {
                diff = x[i] - y[i];
                weight = 1;
                if (i <= 2)
                {
                    weight = 2;
                }

                sum[0] += weight * diff * diff;
            }

            for (var i = 64; i < 128; i++)
            {
                diff = x[i] - y[i];
                weight = 1;
                if (i == 0)
                {
                    weight = 2;
                }

                sum[1] += weight * diff * diff;
            }

            for (var i = 128; i < 192; i++)
            {
                diff = x[i] - y[i];
                weight = 1;
                if (i == 0)
                {
                    weight = 4;
                }
                else
                {
                    if (i <= 2)
                    {
                        weight = 2;
                    }
                }

                sum[2] += weight * diff * diff;
            }

            return (float)(Math.Sqrt(sum[0]) + Math.Sqrt(sum[1]) + Math.Sqrt(sum[2]));
        }
    */
    }
}
