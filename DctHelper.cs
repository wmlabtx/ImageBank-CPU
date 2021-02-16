using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Linq;

namespace ImageBank
{
    public static class DctHelper
    {
        private static readonly double[,] arraycosin = {
            { 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01, 3.535534e-01 },
            { 4.903926e-01, 4.157348e-01, 2.777851e-01, 9.754516e-02, -9.754516e-02, -2.777851e-01, -4.157348e-01, -4.903926e-01 },
            { 4.619398e-01, 1.913417e-01, -1.913417e-01, -4.619398e-01, -4.619398e-01, -1.913417e-01, 1.913417e-01, 4.619398e-01 },
            { 4.157348e-01, -9.754516e-02, -4.903926e-01, -2.777851e-01, 2.777851e-01, 4.903926e-01, 9.754516e-02, -4.157348e-01 },
            { 3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01, 3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01 },
            { 2.777851e-01, -4.903926e-01, 9.754516e-02, 4.157348e-01, -4.157348e-01, -9.754516e-02, 4.903926e-01, -2.777851e-01 },
            { 1.913417e-01, -4.619398e-01, 4.619398e-01, -1.913417e-01, -1.913417e-01, 4.619398e-01, -4.619398e-01, 1.913417e-01 },
            { 9.754516e-02, -2.777851e-01, 4.157348e-01, -4.903926e-01, 4.903926e-01, -4.157348e-01, 2.777851e-01, -9.754516e-02 }
        };

        private static int Yquant(int i)
        {
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

            return j;
        }

        private static int Cquant(int i)
        {
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

            return j;
        }

        private static readonly int[] zigzag = { 0, 1, 8, 16 };
        private static int[,] weight = null;

        private static byte[] Block8x8ToVector(int ch, Mat mat64)
        {
            mat64.GetArray(out byte[] shape64);
            if (shape64.Length != 64)
            {
                throw new Exception("shape64.Length != 64");
            }

            var dct = new double[64];
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var s = 0.0;
                    for (var k = 0; k < 8; k++)
                    {
                        s += arraycosin[j, k] * shape64[8 * i + k];
                    }

                    dct[8 * i + j] = s;
                }
            }

            var matrix = new int[64];
            for (var j = 0; j < 8; j++)
            {
                for (var i = 0; i < 8; i++)
                {
                    var s = 0.0;
                    for (var k = 0; k < 8; k++)
                    {
                        s += arraycosin[i, k] * dct[8 * k + j];
                    }

                    matrix[8 * i + j] = (int)Math.Floor(s + 0.499999);
                }
            }

            var vector = new byte[4];
            if (ch == 0)
            {
                vector[0] = (byte)(Yquant(matrix[0] >> 3) >> 1);
                for (var i = 1; i < 4; i++)
                {
                    vector[i] = (byte)((Yquant(matrix[zigzag[i]]) >> 1) >> 3);
                }
            }
            else
            {
                vector[0] = (byte)Cquant(matrix[0] >> 3);
                for (var i = 1; i < 4; i++)
                {
                    vector[i] = (byte)(Cquant(matrix[zigzag[i]]) >> 3);
                }
            }

            return vector;
        }

        public static byte[] ComputeVector(Bitmap bitmap)
        {
            var ac = new byte[3 * 16 * 64];
            using (var matrgb = bitmap.ToMat())
            using (var matycrcb = new Mat())
            {
                Cv2.CvtColor(matrgb, matycrcb, ColorConversionCodes.BGR2YCrCb);
                var match = matycrcb.Split();
                match[0].SaveImage("match0.png");
                match[1].SaveImage("match1.png");
                match[2].SaveImage("match2.png");
                var offset = 0;
                for (var ch = 0; ch < 3; ch++)
                {
                    using (var mat8x8 = match[ch].Resize(new OpenCvSharp.Size(8, 8), 0, 0, InterpolationFlags.Area))
                    {
                        mat8x8.SaveImage($"mat8x8-{ch}.png");
                        var vector = Block8x8ToVector(ch, mat8x8);
                        Buffer.BlockCopy(vector, 0, ac, offset, 4);
                        offset += 4;
                    }
                }
            }

            return ac;
        }

        public static float CompareVectors(byte[] v1, byte[] v2)
        {
            if (weight == null)
            {
                weight = new int[3, 4];
                weight[0, 0] = 2; weight[0, 1] = 2; weight[0, 2] = 2; weight[0, 3] = 1;
                weight[1, 0] = 2; weight[1, 1] = 1; weight[1, 2] = 1; weight[1, 3] = 1;
                weight[2, 0] = 4; weight[2, 1] = 2; weight[2, 2] = 2; weight[2, 3] = 1;
            }

            var offset = 0;
            var sum = 0f;
            for (var ch = 0; ch < 3; ch++)
            {
                for (var block = 0; block < 16; block++)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        var diff = v1[offset] - v2[offset];
                        sum += weight[ch, k] * diff * diff;
                        offset++;
                    }
                }
            }

            var distance = (float)Math.Sqrt(sum);
            return distance;
        }
    }
}
