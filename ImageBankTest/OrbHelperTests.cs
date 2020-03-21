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
                if (!OrbHelper.ComputeOrbs(thump, out ulong[] vector)) {
                    Assert.Fail();
                }

                Assert.IsTrue(vector.Length >= 4 && vector.Length <= 128);
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
                if (!OrbHelper.ComputeOrbs(thump, out ulong[] orbs)) {
                    Assert.Fail();
                }

                var zero = OrbHelper.GetSim(orbs, orbs);
                Assert.IsTrue(zero == 64f);

                var imgdata2 = File.ReadAllBytes("org.webp");
                if (!Helper.GetBitmapFromImgData(imgdata2, out Bitmap bitmap2, out _)) {
                    Assert.Fail();
                }

                using (var thump2 = Helper.GetThumpFromBitmap(bitmap2)) {
                    if (!OrbHelper.ComputeOrbs(thump2, out ulong[] orbs2)) {
                        Assert.Fail();
                    }

                    var sim = OrbHelper.GetSim(orbs, orbs2);
                }
            }


        }
    }
}