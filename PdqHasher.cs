using System;
using System.Drawing;

namespace ImageBank
{
    public static class PdqHasher
    {
        private static readonly float DCT_MATRIX_SCALE_FACTOR = (float)Math.Sqrt(2.0 / 64.0);
        private static readonly float[][] DCT_matrix = allocateMatrix(16, 64);

        // From Wikipedia: standard RGB to luminance (the 'Y' in 'YUV').
        private static readonly float LUMA_FROM_R_COEFF = (float)0.299;
        private static readonly float LUMA_FROM_G_COEFF = (float)0.587;
        private static readonly float LUMA_FROM_B_COEFF = (float)0.114;

        // Since PDQ uses 64x64 blocks, 1/64th of the image height/width respectively is
        // a full block. But since we use two passes, we want half that window size per
        // pass. Example: 1024x1024 full-resolution input. PDQ downsamples to 64x64.
        // Each 16x16 block of the input produces a single downsample pixel.  X,Y passes
        // with window size 8 (= 1024/128) average pixels with 8x8 neighbors. The second
        // X,Y pair of 1D box-filter passes accumulate data from all 16x16.
        public static readonly int PDQ_JAROSZ_WINDOW_SIZE_DIVISOR = 128;

        // Wojciech Jarosz 'Fast Image Convolutions' ACM SIGGRAPH 2001:
        // X,Y,X,Y passes of 1-D box filters produces a 2D tent filter.
        public static readonly int PDQ_NUM_JAROSZ_XY_PASSES = 2;

        private static int numRows;
        private static int numCols;

        static PdqHasher()
        {
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 64; j++) {
                    DCT_matrix[i][j] = (float)(DCT_MATRIX_SCALE_FACTOR * Math.Cos(Math.PI / 2 / 64.0 * (i + 1) * (2 * j + 1)));
                }
            }
        }

        public static Hash256 Compute(Bitmap bitmap)
        {
            numRows = bitmap.Height;
            numCols = bitmap.Width;

            float[] buffer1 = allocateMatrixAsRowMajorArray(numRows, numCols);
            float[] buffer2 = allocateMatrixAsRowMajorArray(numRows, numCols);
            float[][] buffer64x64 = allocateMatrix(64, 64);
            float[][] buffer16x64 = allocateMatrix(16, 64);
            float[][] buffer16x16 = allocateMatrix(16, 16);

            Hash256 hash = fromBufferedImage(bitmap, ref buffer1, ref buffer2, buffer64x64, buffer16x64, buffer16x16);
            return hash;
        }

        public static Hash256 fromBufferedImage(Bitmap bitmap, 
            ref float[] buffer1, // image numRows x numCols as row-major array
            ref float[] buffer2, // image numRows x numCols as row-major array
            float[][] buffer64x64,
            float[][] buffer16x64,
            float[][] buffer16x16)
        {
            buffer1 = fillFloatLumaFromBufferImage(bitmap);
            Hash256 hash = pdqHash256FromFloatLuma(ref buffer1, ref buffer2, ref buffer64x64, ref buffer16x64, ref buffer16x16);

            return hash;
        }

        public static float[] fillFloatLumaFromBufferImage(Bitmap bitmap)
        {
            float[] luma = new float[numRows * numCols]; // image numRows x numCols as row-major array

            for (int i = 0; i < numRows; i++) {
                for (int j = 0; j < numCols; j++) {
                    Color rgb = bitmap.GetPixel(j, i); // xxx check semantics of these packed-as-int pixels
                    int r = rgb.R;
                    int g = rgb.G;
                    int b = rgb.B;
                    luma[i * numCols + j] =
                      LUMA_FROM_R_COEFF * r +
                      LUMA_FROM_G_COEFF * g +
                      LUMA_FROM_B_COEFF * b;
                }
            }

            return luma;
        }

        public static Hash256 pdqHash256FromFloatLuma(
          ref float[] fullBuffer1, // image numRows x numCols as row-major array
          ref float[] fullBuffer2, // image numRows x numCols as row-major array
          ref float[][] buffer64x64,
          ref float[][] buffer16x64,
          ref float[][] buffer16x16)
        {
            // Downsample (blur and decimate)
            int windowSizeAlongRows = computeJaroszFilterWindowSize(numCols);
            int windowSizeAlongCols = computeJaroszFilterWindowSize(numRows);

            jaroszFilterFloat(
              ref fullBuffer1,
              ref fullBuffer2,
              windowSizeAlongRows,
              windowSizeAlongCols,
              PDQ_NUM_JAROSZ_XY_PASSES
            );

            decimateFloat(fullBuffer1, numRows, numCols, ref buffer64x64);

            // Quality metric.  Reuse the 64x64 image-domain downsample
            // since we already have it.
            int quality = computePDQImageDomainQualityMetric(buffer64x64);

            // 2D DCT
            dct64To16(ref buffer64x64, ref buffer16x64, ref buffer16x16);

            //  Output bits
            Hash256 hash = pdqBuffer16x16ToBits(buffer16x16);

            return hash;
        }

        public static int computeJaroszFilterWindowSize(int dimension)
        {
            return (dimension + PDQ_JAROSZ_WINDOW_SIZE_DIVISOR - 1) / PDQ_JAROSZ_WINDOW_SIZE_DIVISOR;
        }

        public static void jaroszFilterFloat(
          ref float[] buffer1, // matrix as numRows x numCols in row-major order
          ref float[] buffer2, // matrix as numRows x numCols in row-major order
          int windowSizeAlongRows,
          int windowSizeAlongCols,
          int nreps)
        {
            for (int i = 0; i < nreps; i++) {
                boxAlongRowsFloat(buffer1, ref buffer2, windowSizeAlongRows);
                boxAlongColsFloat(buffer2, ref buffer1, windowSizeAlongCols);
            }
        }

        public static void boxAlongRowsFloat(
          float[] fin, // matrix as numRows x numCols in row-major order
          ref float[] fout, // matrix as numRows x numCols in row-major order
          int windowSize
        )
        {
            for (int i = 0; i < numRows; i++) {
                box1DFloat(fin, i * numCols, ref fout, i * numCols, numCols, 1, windowSize);
            }
        }

        public static void boxAlongColsFloat(
          float[] fin, // matrix as numRows x numCols in row-major order
          ref float[] fout, // matrix as numRows x numCols in row-major order
          int windowSize
        )
        {
            for (int j = 0; j < numCols; j++) {
                box1DFloat(fin, j, ref fout, j, numRows, numCols, windowSize);
            }
        }

        // ----------------------------------------------------------------
        // 7 and 4
        //
        //    0 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1
        //    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
        //
        //    .                                PHASE 1: ONLY ADD, NO WRITE, NO SUBTRACT
        //    . .
        //    . . .
        //
        //  0 * . . .                          PHASE 2: ADD, WRITE, WITH NO SUBTRACTS
        //  1 . * . . .
        //  2 . . * . . .
        //  3 . . . * . . .
        //
        //  4   . . . * . . .                  PHASE 3: WRITES WITH ADD & SUBTRACT
        //  5     . . . * . . .
        //  6       . . . * . . .
        //  7         . . . * . . .
        //  8           . . . * . . .
        //  9             . . . * . . .
        // 10               . . . * . . .
        // 11                 . . . * . . .
        // 12                   . . . * . . .
        //
        // 13                     . . . * . .  PHASE 4: FINAL WRITES WITH NO ADDS
        // 14                       . . . * .
        // 15                         . . . *
        //
        //         = 0                                     =  0   PHASE 1
        //         = 0+1                                   =  1
        //         = 0+1+2                                 =  3
        //
        // out[ 0] = 0+1+2+3                               =  6   PHASE 2
        // out[ 1] = 0+1+2+3+4                             = 10
        // out[ 2] = 0+1+2+3+4+5                           = 15
        // out[ 3] = 0+1+2+3+4+5+6                         = 21
        //
        // out[ 4] =   1+2+3+4+5+6+7                       = 28   PHASE 3
        // out[ 5] =     2+3+4+5+6+7+8                     = 35
        // out[ 6] =       3+4+5+6+7+8+9                   = 42
        // out[ 7] =         4+5+6+7+8+9+10                = 49
        // out[ 8] =           5+6+7+8+9+10+11             = 56
        // out[ 9] =             6+7+8+9+10+11+12          = 63
        // out[10] =               7+8+9+10+11+12+13       = 70
        // out[11] =                 8+9+10+11+12+13+14    = 77
        // out[12] =                   9+10+11+12+13+14+15 = 84
        //
        // out[13] =                     10+11+12+13+14+15 = 75  PHASE 4
        // out[14] =                        11+12+13+14+15 = 65
        // out[15] =                           12+13+14+15 = 54
        // ----------------------------------------------------------------

        // ----------------------------------------------------------------
        // 8 and 5
        //
        //    0 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1
        //    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
        //
        //    .                                PHASE 1: ONLY ADD, NO WRITE, NO SUBTRACT
        //    . .
        //    . . .
        //    . . . .
        //
        //  0 * . . . .                        PHASE 2: ADD, WRITE, WITH NO SUBTRACTS
        //  1 . * . . . .
        //  2 . . * . . . .
        //  3 . . . * . . . .
        //
        //  4   . . . * . . . .                PHASE 3: WRITES WITH ADD & SUBTRACT
        //  5     . . . * . . . .
        //  6       . . . * . . . .
        //  7         . . . * . . . .
        //  8           . . . * . . . .
        //  9             . . . * . . . .
        // 10               . . . * . . . .
        // 11                 . . . * . . . .
        //
        // 12                   . . . * . . .  PHASE 4: FINAL WRITES WITH NO ADDS
        // 13                     . . . * . .
        // 14                       . . . * .
        // 15                         . . . *
        //
        //         = 0                                     =  0   PHASE 1
        //         = 0+1                                   =  1
        //         = 0+1+2                                 =  3
        //         = 0+1+2+3                               =  6
        //
        // out[ 0] = 0+1+2+3+4                             = 10
        // out[ 1] = 0+1+2+3+4+5                           = 15
        // out[ 2] = 0+1+2+3+4+5+6                         = 21
        // out[ 3] = 0+1+2+3+4+5+6+7                       = 28
        //
        // out[ 4] =   1+2+3+4+5+6+7+8                     = 36   PHASE 3
        // out[ 5] =     2+3+4+5+6+7+8+9                   = 44
        // out[ 6] =       3+4+5+6+7+8+9+10                = 52
        // out[ 7] =         4+5+6+7+8+9+10+11             = 60
        // out[ 8] =           5+6+7+8+9+10+11+12          = 68
        // out[ 9] =             6+7+8+9+10+11+12+13       = 76
        // out[10] =               7+8+9+10+11+12+13+14    = 84
        // out[11] =                 8+9+10+11+12+13+14+15 = 92
        //
        // out[12] =                   9+10+11+12+13+14+15 = 84  PHASE 4
        // out[13] =                     10+11+12+13+14+15 = 75  PHASE 4
        // out[14] =                        11+12+13+14+15 = 65
        // out[15] =                           12+13+14+15 = 54
        // ----------------------------------------------------------------

        public static void box1DFloat(
          float[] invec,
          int inStartOffset,
          ref float[] outvec,
          int outStartOffset,
          int vectorLength,
          int stride,
          int fullWindowSize)
        {
            int halfWindowSize = (fullWindowSize + 2) / 2; // 7->4, 8->5

            int phase_1_nreps = halfWindowSize - 1;
            int phase_2_nreps = fullWindowSize - halfWindowSize + 1;
            int phase_3_nreps = vectorLength - fullWindowSize;
            int phase_4_nreps = halfWindowSize - 1;

            int li = 0; // Index of left edge of read window, for subtracts
            int ri = 0; // Index of right edge of read windows, for adds
            int oi = 0; // Index into output vector

            float sum = (float)0.0;
            int currentWindowSize = 0;

            // PHASE 1: ACCUMULATE FIRST SUM NO WRITES
            for (int i = 0; i < phase_1_nreps; i++) {
                sum += invec[inStartOffset + ri];
                currentWindowSize++;
                ri += stride;
            }

            // PHASE 2: INITIAL WRITES WITH SMALL WINDOW
            for (int i = 0; i < phase_2_nreps; i++) {
                sum += invec[inStartOffset + ri];
                currentWindowSize++;
                outvec[outStartOffset + oi] = sum / currentWindowSize;
                ri += stride;
                oi += stride;
            }

            // PHASE 3: WRITES WITH FULL WINDOW
            for (int i = 0; i < phase_3_nreps; i++) {
                sum += invec[inStartOffset + ri];
                sum -= invec[inStartOffset + li];
                outvec[outStartOffset + oi] = sum / currentWindowSize;
                li += stride;
                ri += stride;
                oi += stride;
            }

            // PHASE 4: FINAL WRITES WITH SMALL WINDOW
            for (int i = 0; i < phase_4_nreps; i++) {
                sum -= invec[inStartOffset + li];
                currentWindowSize--;
                outvec[outStartOffset + oi] = sum / currentWindowSize;
                li += stride;
                oi += stride;
            }
        }

        public static void decimateFloat(
          float[] fin, // numRows x numCols in row-major order
          int inNumRows,
          int inNumCols,
          ref float[][] fout) // 64x64
        {
            // target centers not corners:
            for (int i = 0; i < 64; i++) {
                int ini = (int)((i + 0.5) * inNumRows / 64);
                for (int j = 0; j < 64; j++) {
                    int inj = (int)((j + 0.5) * inNumCols / 64);
                    fout[i][j] = fin[ini * inNumCols + inj];
                }
            }
        }

        // This is all heuristic (see the PDQ hashing doc). Quantization matters since
        // we want to count *significant* gradients, not just the some of many small
        // ones. The constants are all manually selected, and tuned as described in the
        // document.
        private static int computePDQImageDomainQualityMetric(float[][] buffer64x64)
        {
            int gradientSum = 0;

            for (int i = 0; i < 63; i++) {
                for (int j = 0; j < 64; j++) {
                    float u = buffer64x64[i][j];
                    float v = buffer64x64[i + 1][j];
                    int d = (int)((u - v) * 100 / 255);
                    gradientSum += (int)Math.Abs(d);
                }
            }
            for (int i = 0; i < 64; i++) {
                for (int j = 0; j < 63; j++) {
                    float u = buffer64x64[i][j];
                    float v = buffer64x64[i][j + 1];
                    int d = (int)((u - v) * 100 / 255);
                    gradientSum += (int)Math.Abs(d);
                }
            }

            // Heuristic scaling factor.
            int quality = gradientSum / 90;
            if (quality > 100) {
                quality = 100;
            }

            return quality;
        }

        // Full 64x64 to 64x64 can be optimized e.g. the Lee algorithm.  But here we
        // only want slots (1-16)x(1-16) of the full 64x64 output. Careful experiments
        // showed that using Lee along all 64 slots in one dimension, then Lee along 16
        // slots in the second, followed by extracting slots 1-16 of the output, was
        // actually slower than the current implementation which is completely
        // non-clever/non-Lee but computes only what is needed.

        private static void dct64To16(
          ref float[][] A, // input: 64x64
          ref float[][] T, // temp buffer: 16x64
          ref float[][] B) // output: 16x16
        {
            float[][] D = DCT_matrix;

            // B = D A Dt
            // B = (D A) Dt ; T = D A
            // T is 16x64;

            // T = D A
            // Tij = sum {k} Dik Akj
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 64; j++) {
                    float sumk = (float)0.0;
                    for (int k = 0; k < 64; k++) {
                        sumk += D[i][k] * A[k][j];
                    }

                    T[i][j] = sumk;
                }
            }

            // B = T Dt
            // Bij = sum {k} Tik Djk
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
                    float sumk = (float)0.0;
                    for (int k = 0; k < 64; k++) {
                        sumk += T[i][k] * D[j][k];
                    }

                    B[i][j] = sumk;
                }
            }
        }

        private static Hash256 pdqBuffer16x16ToBits(float[][] dctOutput16x16)
        {
            Hash256 hash = new Hash256();
            float dctMedian = TorbenMedian(dctOutput16x16, 16, 16);
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
                    if (dctOutput16x16[i][j] > dctMedian) {
                        hash.SetBit(i * 16 + j);
                    }
                }
            }

            return hash;
        }

        public static float TorbenMedian(float[][] m, int numRows, int numCols)
        {
            int n = numRows * numCols;
            int midn = (n + 1) / 2;

            int i, j, less, greater, equal;
            float min, max, guess, maxltguess, mingtguess;

            min = max = m[0][0];
            for (i = 0; i < numRows; i++) {
                for (j = 0; j < numCols; j++) {
                    float v = m[i][j];
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
            }

            while (true) {
                guess = (min + max) / 2;
                less = 0; greater = 0; equal = 0;
                maxltguess = min;
                mingtguess = max;

                for (i = 0; i < numRows; i++) {
                    for (j = 0; j < numCols; j++) {
                        float v = m[i][j];
                        if (v < guess) {
                            less++;
                            if (v > maxltguess) maxltguess = v;
                        }
                        else if (v > guess) {
                            greater++;
                            if (v < mingtguess) mingtguess = v;
                        }
                        else equal++;
                    }
                }

                if (less <= midn && greater <= midn)
                    break;
                else if (less > greater)
                    max = maxltguess;
                else
                    min = mingtguess;
            }
            if (less >= midn) {
                return maxltguess;
            }
            else if (less + equal >= midn) {
                return guess;
            }
            else {
                return mingtguess;
            }
        }

        public static float[][] allocateMatrix(int numRows, int numCols)
        {
            float[][] rv = new float[numRows][];
            for (int i = 0; i < numRows; i++) {
                rv[i] = new float[numCols];
            }

            return rv;
        }

        public static float[] allocateMatrixAsRowMajorArray(int numRows, int numCols)
        {
            return new float[numRows * numCols];
        }
    }
}
