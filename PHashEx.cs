using System;

namespace ImageBank
{
    public class PHashEx
    {
        private static float[][] DCT_MATRIX;
        private static readonly float DCT_MATRIX_SCALE_FACTOR = (float)Math.Sqrt(2.0 / 64.0);
        public static readonly int PDQ_JAROSZ_WINDOW_SIZE_DIVISOR = 128;
        public static readonly int PDQ_NUM_JAROSZ_XY_PASSES = 2;

        private readonly PHash64[] hashes;

        public PHashEx(byte[] buffer, int offset)
        {
            InitalizeDctMatrix();
            hashes = new PHash64[4];
            var offsetex = offset;
            for (var i = 0; i < hashes.Length; i++) {
                hashes[i] = new PHash64(buffer, offsetex);
                offsetex += 32;
            }
        }

        public int HammingDistance(PHashEx other)
        {
            var mind = 256 * 4;
            for (var i = 0; i < 4; i++) {
                for (var j = 0; j < 4; j++) {
                    var d = hashes[i].HammingDistance(other.hashes[j]);
                    if (d < mind) {
                        mind = d;
                        if (mind == 0) {
                            return mind;
                        }
                    }
                }
            }

            return mind;
        }

        private static void InitalizeDctMatrix()
        {
            if (DCT_MATRIX == null) {
                DCT_MATRIX = AllocateMatrix(16, 64);
                for (int i = 0; i < 16; i++) {
                    for (int j = 0; j < 64; j++) {
                        DCT_MATRIX[i][j] = (float)(DCT_MATRIX_SCALE_FACTOR * Math.Cos((Math.PI / 2 / 64.0) * (i + 1) * (2 * j + 1)));
                    }
                }
            }
        }

        private static float[][] AllocateMatrix(int numRows, int numCols)
        {
            float[][] rv = new float[numRows][];
            for (int i = 0; i < numRows; i++) {
                rv[i] = new float[numCols];
            }

            return rv;
        }

        private static int ComputeJaroszFilterWindowSize(int dimension)
        {
            return (dimension + PDQ_JAROSZ_WINDOW_SIZE_DIVISOR - 1) / PDQ_JAROSZ_WINDOW_SIZE_DIVISOR;
        }

        public static void Box1DFloat(float[] invec, int inStartOffset, ref float[] outvec, int outStartOffset, int vectorLength, int stride, int fullWindowSize)
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

        private static void BoxAlongRowsFloat(float[] fin, ref float[] fout, int numRows, int numCols, int windowSize)
        {
            for (int i = 0; i < numRows; i++) {
                Box1DFloat(fin, i * numCols, ref fout, i * numCols, numCols, 1, windowSize);
            }
        }

        public static void BoxAlongColsFloat(float[] fin, ref float[] fout, int numRows, int numCols, int windowSize)
        {
            for (int j = 0; j < numCols; j++) {
                Box1DFloat(fin, j, ref fout, j, numRows, numCols, windowSize);
            }
        }

        private static void JaroszFilterFloat(ref float[] buffer1, ref float[] buffer2, int numRows, int numCols, int windowSizeAlongRows, int windowSizeAlongCols, int nreps)
        {
            for (int i = 0; i < nreps; i++) {
                BoxAlongRowsFloat(buffer1, ref buffer2, numRows, numCols, windowSizeAlongRows);
                BoxAlongColsFloat(buffer2, ref buffer1, numRows, numCols, windowSizeAlongCols);
            }
        }

        private static void DecimateFloat(float[] fin, int inNumRows, int inNumCols, ref float[][] fout)
        {
            for (int i = 0; i < 64; i++) {
                int ini = (int)(((i + 0.5) * inNumRows) / 64);
                for (int j = 0; j < 64; j++) {
                    int inj = (int)((j + 0.5) * inNumCols / 64);
                    fout[i][j] = fin[ini * inNumCols + inj];
                }
            }
        }

        private static void Dct64To16(float[][] A, ref float[][] T, ref float[][] B)
        {
            float[][] D = DCT_MATRIX;

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

        private static PHash64 Buffer16x16ToBits(float[][] dctOutput16x16)
        {
            var hash = new PHash64();
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

        private static float[][] Dct16OriginalToRotate90(float[][] a)
        {
            var b = AllocateMatrix(16, 16);
            for (var i = 0; i < 16; i++) {
                for (var j = 0; j < 16; j++) {
                    if ((j & 0x1) != 0) {
                        b[j][i] = a[i][j];
                    } else {
                        b[j][i] = -a[i][j];
                    }
                }
            }

            return b;
        }

        private static float[][] Dct16OriginalToRotate270(float[][] a)
        {
            var b = AllocateMatrix(16, 16);
            for (var i = 0; i < 16; i++) {
                for (var j = 0; j < 16; j++) {
                    if ((i & 0x01) != 0) {
                        b[j][i] = a[i][j];
                    } else {
                        b[j][i] = -a[i][j];
                    }
                }
            }

            return b;
        }

        private static float[][] Dct16OriginalToFlipY(float[][] a)
        {
            var b = AllocateMatrix(16, 16);
            for (var i = 0; i < 16; i++) {
                for (var j = 0; j < 16; j++) {
                    if ((j & 0x01) != 0) {
                        b[i][j] = a[i][j];
                    } else {
                        b[i][j] = -a[i][j];
                    }
                }
            }

            return b;
        }

        public PHashEx(float[][] matrix)
        {
            InitalizeDctMatrix();
            hashes = new PHash64[4];

            int numRows = matrix.Length;
            int numCols = matrix[0].Length;

            float[] buffer1 = new float[numRows * numCols];
            float[] buffer2 = new float[numRows * numCols];
            float[][] buffer64x64 = AllocateMatrix(64, 64);
            float[][] buffer16x64 = AllocateMatrix(16, 64);
            float[][] buffer16x16 = AllocateMatrix(16, 16);

            for (int i = 0; i < numRows; i++) {
                for (int j = 0; j < numCols; j++) {
                    buffer1[i * numCols + j] = matrix[i][j];
                }
            }

            int windowSizeAlongRows = ComputeJaroszFilterWindowSize(numCols);
            int windowSizeAlongCols = ComputeJaroszFilterWindowSize(numRows);
            JaroszFilterFloat(ref buffer1, ref buffer2, numRows, numCols, windowSizeAlongRows, windowSizeAlongCols, PDQ_NUM_JAROSZ_XY_PASSES);

            DecimateFloat(buffer1, numRows, numCols, ref buffer64x64);
            Dct64To16(buffer64x64, ref buffer16x64, ref buffer16x16);

            hashes[0] = Buffer16x16ToBits(buffer16x16);

            var buffer16x16Aux = Dct16OriginalToRotate90(buffer16x16);
            hashes[1] = Buffer16x16ToBits(buffer16x16Aux);

            buffer16x16Aux = Dct16OriginalToRotate270(buffer16x16);
            hashes[2] = Buffer16x16ToBits(buffer16x16Aux);

            buffer16x16Aux = Dct16OriginalToFlipY(buffer16x16);
            hashes[3] = Buffer16x16ToBits(buffer16x16Aux);
        }

        public byte[] ToArray()
        {
            var array = new byte[128];
            for (var i = 0; i < 4; i++) {
                Buffer.BlockCopy(hashes[i].ToArray(), 0, array, i * 32, 32);
            }

            return array;
        }
    }
}
