using System;

namespace ImageBank
{
    /// <summary>
    /// Name |          Blue         |        Green          |           Red         | 
    /// Bit  |00|01|02|03|04|05|06|07|08|09|10|11|12|13|14|15|16|17|18|19|20|21|22|23|
    /// Byte |00000000000000000000000|11111111111111111111111|22222222222222222222222|
    /// </summary>

    public class PixelRGB
    {
        private readonly byte blue;  // 00 - 07
        private readonly byte green; // 08 - 15
        private readonly byte red;   // 16 - 23

        private readonly sbyte l;
        private readonly sbyte a;
        private readonly sbyte b;

        /// <summary>
        /// [0..255]
        /// </summary>
        public int Red { get { return red; } }

        /// <summary>
        /// [0..255]
        /// </summary>
        public int Green { get { return green; } }

        /// <summary>
        /// [0..255]
        /// </summary>
        public int Blue { get { return blue; } }

        /// <summary>
        /// [0..100]
        /// </summary>
        public int L { get { return l; } }

        /// <summary>
        /// [-86..98]
        /// </summary>
        public int A { get { return a; } }

        /// <summary>
        /// [-108..94]
        /// </summary>
        public int B { get { return b; } }

        public PixelRGB(int red, int green, int blue)
        {
            this.red = (byte)red;
            this.green = (byte)green;
            this.blue = (byte)blue;
            ConvertToLAB(red, green, blue, out int ol, out int oa, out int ob);
            l = (sbyte)ol;
            a = (sbyte)oa;
            b = (sbyte)ob;
        }

        public static int ComputeRGBHash(int red, int green, int blue)
        {
            return (red << 16) | (green << 8) | blue;
        }

        public static void ConvertToLAB(int ir, int ig, int ib, out int ol, out int oa, out int ob)
        {
            var r = ir / 255.0;
            var g = ig / 255.0;
            var b = ib / 255.0;

            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            var x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            var y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            var z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

            var dl = (116.0 * y) - 16.0;
            var da = 500.0 * (x - y);
            var db = 200.0 * (y - z);

            ol = (sbyte)Math.Round(dl);
            oa = (sbyte)Math.Round(da);
            ob = (sbyte)Math.Round(db);
        }

        public int RGBHash {
            get { 
                return ComputeRGBHash(red, green, blue); 
            } 
        }

        private static double Deg2rad(double deg)
        {
            return deg * (Math.PI / 180.0);
        }

        public static double Distanse(sbyte l1, sbyte a1, sbyte b1, sbyte l2, sbyte a2, sbyte b2)
        {
            const double kL = 1.0, kC = 1.0, kH = 1.0;
            const double pow25To7 = 6103515625.0; // pow(25, 7)
            var deg360InRad = Deg2rad(360.0);
            var deg180InRad = Deg2rad(180.0);

            /* Equation 2 */
            double C1 = Math.Sqrt((a1 * a1) + (b1 * b1));
            double C2 = Math.Sqrt((a2 * a2) + (b2 * b2));

            /* Equation 3 */
            double barC = (C1 + C2) / 2.0;

            /* Equation 4 */
            double G = 0.5 * (1.0 - Math.Sqrt(Math.Pow(barC, 7.0) / (Math.Pow(barC, 7.0) + pow25To7)));

            /* Equation 5 */
            double a1Prime = (1.0 + G) * a1;
            double a2Prime = (1.0 + G) * a2;

            /* Equation 6 */
            double CPrime1 = Math.Sqrt((a1Prime * a1Prime) + (b1 * b1));
            double CPrime2 = Math.Sqrt((a2Prime * a2Prime) + (b2 * b2));

            /* Equation 7 */
            double hPrime1;
            if (Math.Abs(b1) < 0.000001 && Math.Abs(a1Prime) < 0.000001) {
                hPrime1 = 0.0;
            }
            else {
                hPrime1 = Math.Atan2(b1, a1Prime);
                /* 
                 * This must be converted to a hue angle in degrees between 0 
                 * and 360 by addition of 20 to negative hue angles.
                 */
                if (hPrime1 < 0) {
                    hPrime1 += deg360InRad;
                }
            }

            double hPrime2;
            if (Math.Abs(b2) < 0.000001 && Math.Abs(a2Prime) < 0.000001) {
                hPrime2 = 0.0;
            }
            else {
                hPrime2 = Math.Atan2(b2, a2Prime);
                /* 
                 * This must be converted to a hue angle in degrees between 0 
                 * and 360 by addition of 2 to negative hue angles.
                 */
                if (hPrime2 < 0) {
                    hPrime2 += deg360InRad;
                }
            }

            /* Equation 8 */
            double deltaLPrime = l2 - l1;

            /* Equation 9 */
            double deltaCPrime = CPrime2 - CPrime1;

            /* Equation 10 */
            double deltahPrime;
            double CPrimeProduct = CPrime1 * CPrime2;
            if (Math.Abs(CPrimeProduct) < 0.000001) {
                deltahPrime = 0.0;
            }
            else {
                /* Avoid the Math.Abs() call */
                deltahPrime = hPrime2 - hPrime1;
                if (deltahPrime < -deg180InRad) {
                    deltahPrime += deg360InRad;
                }
                else {
                    if (deltahPrime > deg180InRad) {
                        deltahPrime -= deg360InRad;
                    }
                }
            }

            /* Equation 11 */
            double deltaHPrime = 2.0 * Math.Sqrt(CPrimeProduct) * Math.Sin(deltahPrime / 2.0);

            /* Equation 12 */
            double barLPrime = (l1 + l2) / 2.0;

            /* Equation 13 */
            double barCPrime = (CPrime1 + CPrime2) / 2.0;

            /* Equation 14 */
            double barhPrime, hPrimeSum = hPrime1 + hPrime2;
            if (Math.Abs(CPrime1 * CPrime2) < 0.000001) {
                barhPrime = hPrimeSum;
            }
            else {
                if (Math.Abs(hPrime1 - hPrime2) <= deg180InRad)
                    barhPrime = hPrimeSum / 2.0;
                else {
                    if (hPrimeSum < deg360InRad) {
                        barhPrime = (hPrimeSum + deg360InRad) / 2.0;
                    }
                    else {
                        barhPrime = (hPrimeSum - deg360InRad) / 2.0;
                    }
                }
            }

            /* Equation 15 */
            double T = 1.0 - (0.17 * Math.Cos(barhPrime - Deg2rad(30f))) +
                (0.24 * Math.Cos(2.0 * barhPrime)) +
                (0.32 * Math.Cos((3.0 * barhPrime) + Deg2rad(6f))) -
                (0.20 * Math.Cos((4.0 * barhPrime) - Deg2rad(63f)));

            /* Equation 16 */
            double deltaTheta = Deg2rad(30f) * Math.Exp(-Math.Pow((barhPrime - Deg2rad(275f)) / Deg2rad(25f), 2.0));

            /* Equation 17 */
            double R_C = 2.0 * Math.Sqrt(Math.Pow(barCPrime, 7.0) / (Math.Pow(barCPrime, 7.0) + pow25To7));

            /* Equation 18 */
            double S_L = 1 + ((0.015 * Math.Pow(barLPrime - 50.0, 2.0)) / Math.Sqrt(20 + Math.Pow(barLPrime - 50.0, 2.0)));

            /* Equation 19 */
            double S_C = 1 + (0.045 * barCPrime);

            /* Equation 20 */
            double S_H = 1 + (0.015 * barCPrime * T);

            /* Equation 21 */
            double R_T = (-Math.Sin(2.0 * deltaTheta)) * R_C;

            /* Equation 22 */
            double deltaE = Math.Sqrt(
                Math.Pow(deltaLPrime / (kL * S_L), 2.0) +
                Math.Pow(deltaCPrime / (kC * S_C), 2.0) +
                Math.Pow(deltaHPrime / (kH * S_H), 2.0) +
                (R_T * (deltaCPrime / (kC * S_C)) * (deltaHPrime / (kH * S_H))));

            return deltaE;
        }
    }
}
