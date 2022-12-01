using System;

namespace ImageBankTest
{
    public static class ColorHelper
    {
        private static double CubicRoot(double n)
        {
            return Math.Pow(n, 1.0 / 3.0);
        }

        private static double PivotRgb(double n)
        {
            return (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
        }

        private static double PivotXyz(double n)
        {
            return n > 0.008856 ? CubicRoot(n) : (903.3 * n + 16) / 116;
        }

        public static void ToLAB(byte rb, byte gb, byte bb, out double l, out double a, out double b)
        {
            var rf = PivotRgb(rb / 255.0);
            var gf = PivotRgb(gb / 255.0);
            var bf = PivotRgb(bb / 255.0);

            var x = rf * 0.4124 + gf * 0.3576 + bf * 0.1805;
            var y = rf * 0.2126 + gf * 0.7152 + bf * 0.0722;
            var z = rf * 0.0193 + gf * 0.1192 + bf * 0.9505;

            x = PivotXyz(x / 95.047);
            y = PivotXyz(y / 100.0);
            z = PivotXyz(z / 108.883);

            l = Math.Max(0, 116 * y - 16);
            a = 500 * (x - y);
            b = 200 * (y - z);
        }

        private static double Distance(double a, double b)
        {
            return (a - b) * (a - b);
        }

        public static double Cie1976(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            var differences = Distance(l1, l2) + Distance(a1, a2) + Distance(b1, b2);
            return Math.Sqrt(differences);
        }

        public static double Cie1994(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            var deltaL = l1 - l2;
            var deltaA = a1 - a2;
            var deltaB = b1 - b2;

            var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
            var c2 = Math.Sqrt(a2 * a2 + b2 * b2);
            var deltaC = c1 - c2;

            var deltaH = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
            deltaH = deltaH < 0 ? 0 : Math.Sqrt(deltaH);

            const double sl = 1.0;
            const double kc = 1.0;
            const double kh = 1.0;

            var sc = 1.0 + 0.045 * c1;
            var sh = 1.0 + 0.015 * c1;

            var deltaLKlsl = deltaL / (1.0 * sl);
            var deltaCkcsc = deltaC / (kc * sc);
            var deltaHkhsh = deltaH / (kh * sh);
            var i = deltaLKlsl * deltaLKlsl + deltaCkcsc * deltaCkcsc + deltaHkhsh * deltaHkhsh;
            return i < 0 ? 0 : Math.Sqrt(i);
        }

        public static double Cie2000(double l1, double a1, double b1, double l2, double a2, double b2)
        {
            const double kl = 1.0;
            const double kc = 1.0;
            const double kh = 1.0;

            var lBar = (l1 + l2) / 2.0;

            var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
            var c2 = Math.Sqrt(a2 * a2 + b2 * b2);
            var cBar = (c1 + c2) / 2.0;

            var cBarInPower7 = cBar * cBar * cBar;
            cBarInPower7 *= cBarInPower7 * cBar;
            var g = (1 - Math.Sqrt(cBarInPower7 / (cBarInPower7 + 6103515625))); // 25 ^ 7
            var aPrime1 = a1 * (a1 / 2.0) * g;
            var aPrime2 = a2 * (a2 / 2.0) * g;

            var cPrime1 = Math.Sqrt(aPrime1 * aPrime1 + b1 * b1);
            var cPrime2 = Math.Sqrt(aPrime2 * aPrime2 + b2 * b2);
            var cBarPrime = (cPrime1 + cPrime2) / 2.0;

            var hPrime1 = Math.Atan2(b1, aPrime1) % 360;
            var hPrime2 = Math.Atan2(b2, aPrime2) % 360;

            var hBar = Math.Abs(hPrime1 - hPrime2);

            double deltaHPrime;
            if (hBar <= 180) {
                deltaHPrime = hPrime2 - hPrime1;
            }
            else if (hBar > 180 && hPrime2 <= hPrime1) {
                deltaHPrime = hPrime2 - hPrime1 + 360.0;
            }
            else {
                deltaHPrime = hPrime2 - hPrime1 - 360.0;
            }

            var deltaLPrime = l2 - l1;
            var deltaCPrime = cPrime2 - cPrime1;
            deltaHPrime = 2 * Math.Sqrt(cPrime1 * cPrime2) * Math.Sin(deltaHPrime / 2.0);

            var hBarPrime = hBar > 180
                                     ? (hPrime1 + hPrime2 + 360) / 2.0
                                     : (hPrime1 + hPrime2) / 2.0;

            var t = 1
                    - .17 * Math.Cos(hBarPrime - 30)
                    + .24 * Math.Cos(2 * hBarPrime)
                    + .32 * Math.Cos(3 * hBarPrime + 6)
                    - .2 * Math.Cos(4 * hBarPrime - 63);

            double lBarMinus50Sqr = (lBar - 50) * (lBar - 50);
            var sl = 1 + (.015 * lBarMinus50Sqr) / Math.Sqrt(20 + lBarMinus50Sqr);
            var sc = 1 + .045 * cBarPrime;
            var sh = 1 + .015 * cBarPrime * t;

            double cBarPrimeInPower7 = cBarPrime * cBarPrime * cBarPrime;
            cBarPrimeInPower7 *= cBarPrimeInPower7 * cBarPrime;
            var rt = -2
                     * Math.Sqrt(cBarPrimeInPower7 / (cBarPrimeInPower7 + 6103515625)) // 25 ^ 7
                     * Math.Sin(60.0 * Math.Exp(-((hBarPrime - 275.0) / 25.0)));

            double deltaLPrimeDivklsl = deltaLPrime / (kl * sl);
            double deltaCPrimeDivkcsc = deltaCPrime / (kc * sc);
            double deltaHPrimeDivkhsh = deltaHPrime / (kh * sh);
            var deltaE = Math.Sqrt(
                deltaLPrimeDivklsl * deltaLPrimeDivklsl +
                deltaCPrimeDivkcsc * deltaCPrimeDivkcsc +
                deltaHPrimeDivkhsh * deltaHPrimeDivkhsh +
                rt * (deltaCPrime / (kc * kh)) * (deltaHPrime / (kh * sh)));

            return deltaE;
        }
    }
}
