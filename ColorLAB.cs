using System;
using System.Diagnostics.Contracts;

namespace ImageBank
{
    public class ColorLAB
    {
        public float L { get; set; }
        public float A { get; set; }
        public float B { get; set; }
        public byte IRGB { get; set; }

        public ColorLAB(ColorRGB colorRGB)
        {
            Contract.Requires(colorRGB != null);
            var lab = ToLAB(colorRGB);
            L = lab.L;
            A = lab.A;
            B = lab.B;
            IRGB = lab.IRGB;
        }

        public ColorLAB(float l, float a, float b, byte irgb)
        {
            L = l;
            A = a;
            B = b;
            IRGB = irgb;
        }

        private ColorLAB ToLAB(ColorRGB colorRGB)
        {
            var r = colorRGB.R / 255.0;
            var g = colorRGB.G / 255.0;
            var b = colorRGB.B / 255.0;
            var r2 = colorRGB.R >> 6;
            var g2 = colorRGB.G >> 6;
            var b2 = colorRGB.B >> 6;
            var irgb = (byte)((r2 << 4) | (g2 << 2) | b2);

            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            var x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            var y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            var z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

            var lab = new ColorLAB(
                (float)((116.0 * y) - 16.0),
                (float)(500.0 * (x - y)),
                (float)(200.0 * (y - z)),
                irgb
                );

            return lab;
        }

        public ColorRGB ToRGB()
        {
            var y = (L + 16.0) / 116.0;
            var x = A / 500.0 + y;
            var z = y - B / 200.0;

            x = 0.95047 * ((x * x * x > 0.008856) ? x * x * x : (x - 16.0 / 116.0) / 7.787);
            y = 1.00000 * ((y * y * y > 0.008856) ? y * y * y : (y - 16.0 / 116.0) / 7.787);
            z = 1.08883 * ((z * z * z > 0.008856) ? z * z * z : (z - 16.0 / 116.0) / 7.787);

            var r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            var g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            var b = x * 0.0557 + y * -0.2040 + z * 1.0570;

            r = (r > 0.0031308) ? (1.055 * Math.Pow(r, 1.0 / 2.4) - 0.055) : 12.92 * r;
            g = (g > 0.0031308) ? (1.055 * Math.Pow(g, 1.0 / 2.4) - 0.055) : 12.92 * g;
            b = (b > 0.0031308) ? (1.055 * Math.Pow(b, 1.0 / 2.4) - 0.055) : 12.92 * b;

            var rgb = new ColorRGB(
                (byte)(Math.Max(0, Math.Min(1.0, r)) * 255),
                (byte)(Math.Max(0, Math.Min(1.0, g)) * 255),
                (byte)(Math.Max(0, Math.Min(1.0, b)) * 255)
                );

            return rgb;
        }

        private static float Deg2rad(float deg)
        {
            return (float)(deg * (Math.PI / 180.0));
        }

        public float CIEDE2000(ColorLAB other)
        {
            Contract.Requires(other != null);

            const double kL = 1.0, kC = 1.0, kH = 1.0;
            const double pow25To7 = 6103515625.0; // pow(25, 7)
            var deg360InRad = Deg2rad(360f);
            var deg180InRad = Deg2rad(180f);

            /* Equation 2 */
            double C1 = Math.Sqrt((A * A) + (B * B));
            double C2 = Math.Sqrt((other.A * other.A) + (other.B * other.B));

            /* Equation 3 */
            double barC = (C1 + C2) / 2.0;

            /* Equation 4 */
            double G = 0.5 * (1.0 - Math.Sqrt(Math.Pow(barC, 7.0) / (Math.Pow(barC, 7.0) + pow25To7)));

            /* Equation 5 */
            double a1Prime = (1.0 + G) * A;
            double a2Prime = (1.0 + G) * other.A;

            /* Equation 6 */
            double CPrime1 = Math.Sqrt((a1Prime * a1Prime) + (B * B));
            double CPrime2 = Math.Sqrt((a2Prime * a2Prime) + (other.B * other.B));

            /* Equation 7 */
            double hPrime1;
            if (Math.Abs(B) < 0.000001 && Math.Abs(a1Prime) < 0.000001)
            {
                hPrime1 = 0.0;
            }
            else
            {
                hPrime1 = Math.Atan2(B, a1Prime);
                /* 
                 * This must be converted to a hue angle in degrees between 0 
                 * and 360 by addition of 20 to negative hue angles.
                 */
                if (hPrime1 < 0)
                {
                    hPrime1 += deg360InRad;
                }
            }

            double hPrime2;
            if (Math.Abs(other.B) < 0.000001 && Math.Abs(a2Prime) < 0.000001)
            {
                hPrime2 = 0.0;
            }
            else
            {
                hPrime2 = Math.Atan2(other.B, a2Prime);
                /* 
                 * This must be converted to a hue angle in degrees between 0 
                 * and 360 by addition of 2 to negative hue angles.
                 */
                if (hPrime2 < 0)
                {
                    hPrime2 += deg360InRad;
                }
            }

            /* Equation 8 */
            double deltaLPrime = other.L - L;

            /* Equation 9 */
            double deltaCPrime = CPrime2 - CPrime1;

            /* Equation 10 */
            double deltahPrime;
            double CPrimeProduct = CPrime1 * CPrime2;
            if (Math.Abs(CPrimeProduct) < 0.000001)
            {
                deltahPrime = 0.0;
            }
            else
            {
                /* Avoid the Math.Abs() call */
                deltahPrime = hPrime2 - hPrime1;
                if (deltahPrime < -deg180InRad)
                {
                    deltahPrime += deg360InRad;
                }
                else
                {
                    if (deltahPrime > deg180InRad)
                    {
                        deltahPrime -= deg360InRad;
                    }
                }
            }

            /* Equation 11 */
            double deltaHPrime = 2.0 * Math.Sqrt(CPrimeProduct) * Math.Sin(deltahPrime / 2.0);

            /* Equation 12 */
            double barLPrime = (L + other.L) / 2.0;

            /* Equation 13 */
            double barCPrime = (CPrime1 + CPrime2) / 2.0;

            /* Equation 14 */
            double barhPrime, hPrimeSum = hPrime1 + hPrime2;
            if (Math.Abs(CPrime1 * CPrime2) < 0.000001)
            {
                barhPrime = hPrimeSum;
            }
            else
            {
                if (Math.Abs(hPrime1 - hPrime2) <= deg180InRad)
                    barhPrime = hPrimeSum / 2.0;
                else
                {
                    if (hPrimeSum < deg360InRad)
                    {
                        barhPrime = (hPrimeSum + deg360InRad) / 2.0;
                    }
                    else
                    {
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

            return (float)deltaE;
        }
    }
}