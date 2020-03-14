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
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            using (var thump = Helper.GetThumpFromBitmap(bitmap)) {
                if (!OrbHelper.ComputeOrbs(thump, out Mat orbs)) {
                    Assert.Fail();
                }

                Assert.IsTrue(orbs.Rows == 32 && orbs.Cols == 32);
            }
        }

        [TestMethod()]
        public void GetDistanceTest()
        {
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            using (var thump = Helper.GetThumpFromBitmap(bitmap)) {
                if (!OrbHelper.ComputeOrbs(thump, out Mat orbs)) {
                    Assert.Fail();
                }

                Assert.IsTrue(orbs.Rows == 32 && orbs.Cols == 32);
                var zero = OrbHelper.GetDistance(orbs, orbs);
                Assert.IsTrue(zero == 0f);
            }
        }
    }
}