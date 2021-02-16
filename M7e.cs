using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public static class M7e
    {
        private static double[,] QuantTable =
                        {{0.010867, 0.057915, 0.099526, 0.144849, 0.195573, 0.260504, 0.358031, 0.530128},
                    {0.012266, 0.069934, 0.125879, 0.182307, 0.243396, 0.314563, 0.411728, 0.564319},
                    {0.004193, 0.025852, 0.046860, 0.068519, 0.093286, 0.123490, 0.161505, 0.228960},
                    {0.004174, 0.025924, 0.046232, 0.067163, 0.089655, 0.115391, 0.151904, 0.217745},
                    {0.006778, 0.051667, 0.108650, 0.166257, 0.224226, 0.285691, 0.356375, 0.450972}};

        private static int treshold = 10;

        public static int BIN_COUNT = 80;
        private static int[] bins = new int[80];

        private static int width;
        private static int height;
        private static int num_block = 1100;


        private static int NoEdge = 0;
        private static int vertical_edge = 1;
        private static int horizontal_edge = 2;
        private static int non_directional_edge = 3;
        private static int diagonal_45_degree_edge = 4;
        private static int diagonal_135_degree_edge = 5;


        private static double[,] grey_level;


        private static double[] Local_Edge_Histogram = new double[80];
        private static int blockSize = -1;

        public static double[] Quant(double[] Local_Edge_Histogram)
        {
            double[] Edge_HistogramElement = new double[Local_Edge_Histogram.Length];
            double iQuantValue = 0;

            for (int i = 0; i < Local_Edge_Histogram.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Edge_HistogramElement[i] = j;
                    if (j < 7)
                        iQuantValue = (QuantTable[i % 5, j] + QuantTable[i % 5, j + 1]) / 2.0;
                    else
                        iQuantValue = 1.0;
                    if (Local_Edge_Histogram[i] <= iQuantValue)
                    {
                        break;
                    }
                }

            }
            return Edge_HistogramElement;
        }
        public static double[] Apply(Bitmap srcImg)
        {

            double[] EDHTable = new double[80];

            width = srcImg.Width;
            height = srcImg.Height;

            PixelFormat fmt = (srcImg.PixelFormat == PixelFormat.Format8bppIndexed) ?
                      PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;

            BitmapData srcData = srcImg.LockBits(
               new Rectangle(0, 0, width, height),
               ImageLockMode.ReadOnly, fmt);



            grey_level = new double[width, height];



            int offset = srcData.Stride - ((fmt == PixelFormat.Format8bppIndexed) ? width : width * 3);

            unsafe
            {
                byte* src = (byte*)srcData.Scan0.ToPointer();


                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src += 3)
                    {

                        double YY = (0.114 * src[0] + 0.587 * src[1] + 0.299 * src[2]) / 256.0;
                        int mean = (int)(219.0 * YY + 16.5);

                        grey_level[x, y] = mean;

                    }

                    src += offset;


                }

            }

            srcImg.UnlockBits(srcData);



            EDHTable = extractFeature();
            return (EDHTable);
        }

        private static int getblockSize()
        {
            if (blockSize < 0)
            {
                double a = (int)(Math.Sqrt((width * height) / num_block));
                blockSize = (int)(Math.Floor((a / 2)) * 2);
                if (blockSize == 0)
                    blockSize = 2;
            }
            return blockSize;
        }

        private static double getFirstBlockAVG(int i, int j)
        {
            double average_brightness = 0;
            if (grey_level[i, j] != 0)
            {

                for (int m = 0; m <= (getblockSize() >> 1) - 1; m++)
                {
                    for (int n = 0; n <= (getblockSize() >> 1) - 1; n++)
                    {
                        average_brightness = average_brightness + grey_level[i + m, j + n];
                    }
                }
            }
            else
            {

            }
            double bs = getblockSize() * getblockSize();
            double div = 4 / bs;
            average_brightness = average_brightness * div;
            return average_brightness;
        }

        private static double getSecondBlockAVG(int i, int j)
        {
            double average_brightness = 0;
            if (grey_level[i, j] != 0)

                for (int m = (int)(getblockSize() >> 1); m <= getblockSize() - 1; m++)
                {
                    for (int n = 0; n <= (getblockSize() >> 1) - 1; n++)
                    {
                        average_brightness += grey_level[i + m, j + n];


                    }
                }
            else
            {

            }
            double bs = getblockSize() * getblockSize();
            double div = 4 / bs;
            average_brightness = average_brightness * div;
            return average_brightness;
        }

        private static double getThirdBlockAVG(int i, int j)
        {
            double average_brightness = 0;
            if (grey_level[i, j] != 0)
            {

                for (int m = 0; m <= (getblockSize() >> 1) - 1; m++)
                {
                    for (int n = (int)(getblockSize() >> 1); n <= getblockSize() - 1; n++)
                    {
                        average_brightness += grey_level[i + m, j + n];
                    }
                }
            }
            else
            {

            }
            double bs = getblockSize() * getblockSize();
            double div = 4 / bs;
            average_brightness = average_brightness * div;
            return average_brightness;
        }

        private static double getFourthBlockAVG(int i, int j)
        {
            double average_brightness = 0;

            for (int m = (int)(getblockSize() >> 1); m <= getblockSize() - 1; m++)
            {
                for (int n = (int)(getblockSize() >> 1); n <= getblockSize() - 1; n++)
                {
                    average_brightness += grey_level[i + m, j + n];
                }
            }
            double bs = getblockSize() * getblockSize();
            double div = 4 / bs;
            average_brightness = average_brightness * div;
            return average_brightness;
        }

        private static int getEdgeFeature(int i, int j)
        {
            double[] average = {getFirstBlockAVG(i, j), getSecondBlockAVG(i, j),
                getThirdBlockAVG(i, j), getFourthBlockAVG(i, j)};
            double th = treshold;
            double[,] edge_filter = {{1.0, -1.0, 1.0, -1.0},
                {1.0, 1.0, -1.0, -1.0},
                {Math.Sqrt(2), 0.0, 0.0, -Math.Sqrt(2)},
                {0.0, Math.Sqrt(2), -Math.Sqrt(2), 0.0},
                {2.0, -2.0, -2.0, 2.0}};
            double[] strengths = new double[5];
            int e_index;

            for (int e = 0; e < 5; e++)
            {
                for (int k = 0; k < 4; k++)
                {
                    strengths[e] += average[k] * edge_filter[e, k];
                }
                strengths[e] = Math.Abs(strengths[e]);
            }
            double e_max = strengths[0];
            e_index = vertical_edge;
            if (strengths[1] > e_max)
            {
                e_max = strengths[1];
                e_index = horizontal_edge;
            }
            if (strengths[2] > e_max)
            {
                e_max = strengths[2];
                e_index = diagonal_45_degree_edge;
            }
            if (strengths[3] > e_max)
            {
                e_max = strengths[3];
                e_index = diagonal_135_degree_edge;
            }
            if (strengths[4] > e_max)
            {
                e_max = strengths[4];
                e_index = non_directional_edge;
            }
            if (e_max < th)
            {
                e_index = NoEdge;
            }

            return (e_index);
        }

        public static double[] extractFeature()
        {
            int sub_local_index = 0;
            int EdgeTypeOfBlock = 0;
            int[] count_local = new int[16];

            for (int i = 0; i < 16; i++)
            {
                count_local[i] = 0;
            }

            for (int j = 0; j <= height - getblockSize(); j += getblockSize())
                for (int i = 0; i <= width - getblockSize(); i += getblockSize())
                {
                    sub_local_index = (int)((i << 2) / width) + ((int)((j << 2) / height) << 2);
                    count_local[sub_local_index]++;

                    EdgeTypeOfBlock = getEdgeFeature(i, j);

                    switch (EdgeTypeOfBlock)
                    {
                        case (0): break;
                        case (1): Local_Edge_Histogram[sub_local_index * 5]++; break;
                        case (2): Local_Edge_Histogram[sub_local_index * 5 + 1]++; break;
                        case (4): Local_Edge_Histogram[sub_local_index * 5 + 2]++; break;
                        case (5): Local_Edge_Histogram[sub_local_index * 5 + 3]++; break;
                        case (3): Local_Edge_Histogram[sub_local_index * 5 + 4]++; break;
                    }

                }//for(i)

            for (int k = 0; k < 80; k++)
            {
                Local_Edge_Histogram[k] /= count_local[(int)k / 5];
            }
            return (Local_Edge_Histogram);
        }

        public static double CalculateDistance(double[] edgeHistogramA, double[] edgeHistogramB)
        {

            double result = 0;

            /*
            for (int i = 0; i < edgeHistogramA.Length; i++)
            {
                result += Math.Abs((float)QuantTable[i % 5, (int)edgeHistogramA[i]] - (float)QuantTable[i % 5, (int)edgeHistogramB[i]]);
            }
            */
            
                        for (int i = 0; i <= 4; i++)
                        {
                            result += 5f * Math.Abs((float)edgeHistogramA[i] - (float)edgeHistogramB[i]);
                        }
                        for (int i = 5; i < 80; i++)
                        {
                            result += Math.Abs((float)edgeHistogramA[i] - (float)edgeHistogramB[i]);
                        }
            
            return result;
        }
    }
}
