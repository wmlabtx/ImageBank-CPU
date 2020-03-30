using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using System.Drawing;
using System.IO;

namespace ImageBank.Tests
{
    [TestClass()]
    public class OrbHelperTests
    {
        [TestMethod()]
        public void ComputeOrbsTest()
        {
            /*
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap, out _)) {
                Assert.Fail();
            }

            using (var thump = Helper.GetThumpFromBitmap(bitmap)) {
                if (!OrbHelper.ComputeOrbs(thump, out ulong[] vector)) {
                    Assert.Fail();
                }

                Assert.IsTrue(vector.Length >= 4 && vector.Length <= 128);
            }
            */
        }

        [TestMethod()]
        public void GetDistanceTest()
        {
            var imagedata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImageData(imagedata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            if (!OrbHelper.ComputeOrbs(bitmap, out ulong[] orbs)) {
                Assert.Fail();
            }

            var imagedatapng = File.ReadAllBytes("orgpng.png");
            if (!Helper.GetBitmapFromImageData(imagedatapng, out Bitmap bitmappng)) {
                Assert.Fail();
            }

            if (!OrbHelper.ComputeOrbs(bitmappng, out ulong[] orbspng)) {
                Assert.Fail();
            }

            var zero = OrbHelper.GetSim(orbs, orbspng, 32);
            var zeroone = 64 - OrbHelper.GetDistance(orbs, 0, orbspng, 0);
        }
    }
}