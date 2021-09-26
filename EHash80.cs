using System;

namespace ImageBank
{
    public class EHash80
    {
        private static readonly double[,] QuantTable =
                    {{0.010867, 0.057915, 0.099526, 0.144849, 0.195573, 0.260504, 0.358031, 0.530128},
                    {0.012266, 0.069934, 0.125879, 0.182307, 0.243396, 0.314563, 0.411728, 0.564319},
                    {0.004193, 0.025852, 0.046860, 0.068519, 0.093286, 0.123490, 0.161505, 0.228960},
                    {0.004174, 0.025924, 0.046232, 0.067163, 0.089655, 0.115391, 0.151904, 0.217745},
                    {0.006778, 0.051667, 0.108650, 0.166257, 0.224226, 0.285691, 0.356375, 0.450972}};

        private readonly byte[] v80;

        public EHash80(byte[] buffer, int offset)
        {
            v80 = new byte[80];
            Buffer.BlockCopy(buffer, offset, v80, 0, 80);
        }

        public int ManhattanDistance(EHash80 other)
        {
            int n = 0;
            for (int i = 0; i < 80; i++) {
                n += Math.Abs(v80[i] - other.v80[i]);
            }

            return n;
        }

        public byte[] ToArray()
        {
            return v80;
        }

        public EHash80(float[][] matrix)
        {
            int width = matrix[0].Length;
            int height = matrix.Length;
            double[] ehd = new double[80];
            for (int r = 0; r < 80; r++) {
                ehd[r] = 0.0;
            }

            double f1, f2, f3, f4, f5;
            int possition = 0;
            int W = Convert.ToInt32((double)width / 4);
            int H = Convert.ToInt32((double)height / 4);

            for (int i = 0; i < height - 2; i += 2) {
                for (int j = 0; j < width - 2; j += 2) {
                    f1 = matrix[i][j] * 1 + matrix[i + 1][j] * -1 + matrix[i][j + 1] * 1 + matrix[i + 1][j + 1] * -1;
                    f2 = matrix[i][j] * 1 + matrix[i + 1][j] * 1 + matrix[i][j + 1] * -1 + matrix[i + 1][j + 1] * -1;
                    f3 = matrix[i][j] * Math.Sqrt(2) + matrix[i + 1][j] * 0 + matrix[i][j + 1] * 0 + matrix[i + 1][j + 1] * -Math.Sqrt(2);
                    f4 = matrix[i][j] * 0 + matrix[i + 1][j] * Math.Sqrt(2) + matrix[i][j + 1] * -Math.Sqrt(2) + matrix[i + 1][j + 1] * 0;
                    f5 = matrix[i][j] * 2 + matrix[i + 1][j] * -2 + matrix[i][j + 1] * -2 + matrix[i + 1][j + 1] * 2;

                    double Maximum = Math.Max(f1, Math.Max(f2, Math.Max(f3, Math.Max(f4, f5))));
                    if (Maximum == f1) {
                        possition = 0;
                    }

                    if (Maximum == f2) {
                        possition = 1;
                    }

                    if (Maximum == f3) {
                        possition = 2;
                    }

                    if (Maximum == f4) {
                        possition = 3;
                    }

                    if (Maximum == f5) {
                        possition = 4;
                    }

                    if (Maximum >= 0.0) {
                        ehd[5 * (4 * (j / W) + i / H) + possition]++;
                    }
                }
            }

            double NormSum = 0;
            for (int i = 0; i < 80; i++) {
                NormSum += ehd[i];
            }

            if (NormSum > 0) {
                for (int i = 0; i < 80; i++) {
                    ehd[i] = Math.Sqrt(ehd[i] / NormSum);
                }
            }

            v80 = new byte[80];
            for (int i = 0; i < 80; i++) {
                v80[i] = (byte)(ehd[i] * 255.0);
            }
        }
    }
}
