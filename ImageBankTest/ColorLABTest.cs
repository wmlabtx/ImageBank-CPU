using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
