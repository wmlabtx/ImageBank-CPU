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
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap, out _)) {
                Assert.Fail();
            }

            using (var thump = Helper.GetThumpFromBitmap(bitmap)) {
                if (!OrbHelper.ComputeOrbs(thump, out Mat vector, out ulong[] scalar)) {
                    Assert.Fail();
                }

                Assert.IsTrue(vector.Rows == 32 && vector.Cols == 32);
                Assert.IsTrue(scalar.Length == 4);
            }
        }

        [TestMethod()]
        public void GetDistanceTest()
        {
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap, out _)) {
                Assert.Fail();
            }

            using (var thump = Helper.GetThumpFromBitmap(bitmap)) {
                if (!OrbHelper.ComputeOrbs(thump, out Mat orbs, out ulong[] scalar)) {
                    Assert.Fail();
                }

                var zero = OrbHelper.GetDistance(orbs, orbs);
                Assert.IsTrue(zero == 0f);

                var hamming = OrbHelper.GetDistance(scalar, scalar);
                Assert.IsTrue(hamming == 0);

            }
        }
    }
}