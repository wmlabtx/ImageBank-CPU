using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ImageBankTest
{
    [TestClass()]
    public class ColorLABTest
    {
        [TestMethod()]
        public void Converter()
        {
            var black = new ColorRGB(0, 0, 0);
            var blackLAB = new ColorLAB(black);
            var white = new ColorRGB(255, 255, 255);
            var whiteLAB = new ColorLAB(white);
            var blue = new ColorRGB(0, 0, 128);
            var blueLAB = new ColorLAB(blue);
            var bluex = new ColorRGB(0, 0, 127);
            var bluexLAB = new ColorLAB(bluex);
            var bluexLABrevert = bluexLAB.ToRGB();
            var distance = blackLAB.CIEDE2000(blackLAB);
            distance = blackLAB.CIEDE2000(whiteLAB);
            distance = blackLAB.CIEDE2000(blueLAB);
            distance = bluexLAB.CIEDE2000(blueLAB);
        }

        [TestMethod()]
        public void CaclulateLABRanges()
        {
            float maxL = float.MinValue;
            float maxA = float.MinValue;
            float maxB = float.MinValue;
            float minL = float.MaxValue;
            float minA = float.MaxValue;
            float minB = float.MaxValue;

            for (int r = 0; r < 256; r += 1)
                for (int g = 0; g < 256; g += 1)
                    for (int b = 0; b < 256; b += 1) {
                        var rgb = new ColorRGB((byte)r, (byte)g, (byte)b);
                        var lab = new ColorLAB(rgb);

                        maxL = Math.Max(maxL, lab.L);
                        maxA = Math.Max(maxA, lab.A);
                        maxB = Math.Max(maxB, lab.B);
                        minL = Math.Min(minL, lab.L);
                        minA = Math.Min(minA, lab.A);
                        minB = Math.Min(minB, lab.B);
                    }

            Console.WriteLine("maxL = " + maxL + ", maxA = " + maxA + ", maxB = " + maxB);
            Console.WriteLine("minL = " + minL + ", minA = " + minA + ", minB = " + minB);

            // L 0.0 100.0 = 100.0                  * 0.15 [0 - 15]
            // A -86.18 98.25 = 184.43  186     +87        [0 - 28]
            // B -107.86 94.48 = 202.34 204 0.1569 + 108   [0 - 30]  
        }
    }
}
