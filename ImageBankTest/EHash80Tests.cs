using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ImageBank.Tests
{
    [TestClass()]
    public class EHash80Tests
    {
        [TestMethod()]
        public void HammingDistanceTest()
        {
            var imagedata = File.ReadAllBytes("gab_org.jpg");
            var matrix_org = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_org);
            var e_org = new EHash80(matrix_org);
            var d_org = e_org.ManhattanDistance(e_org);
            Assert.AreEqual(d_org, 0);

            imagedata = File.ReadAllBytes("gab_scale.jpg");
            var matrix_scale = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_scale);
            var e_scale = new EHash80(matrix_scale);
            var d_scale = e_scale.ManhattanDistance(e_org);
            Assert.AreEqual(d_scale, 193);

            imagedata = File.ReadAllBytes("gab_nosim1.jpg");
            var matrix_nosim = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix_nosim);
            var e_nosim = new EHash80(matrix_nosim);
            var d_nosim = e_nosim.ManhattanDistance(e_org);
            Assert.AreEqual(d_nosim, 110);
        }
    }
}
