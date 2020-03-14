using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Drawing;
using System;
using System.Linq;

namespace ImageBank.Tests
{
    [TestClass()]
    public class HelperTests
    {
        [TestMethod()]
        public void ComputeHash3250Test()
        {
            var array = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var hash1 = Helper.ComputeHash3250(array);
            Assert.IsTrue(hash1.Equals("ygetmyp5lfmlmzlfu3qyrezblaqspkhv4fd26hjm7iia5a33ok", StringComparison.Ordinal));
            array[0] = 0x04;
            var hash2 = Helper.ComputeHash3250(array);
            Assert.IsTrue(hash2.Equals("jlnnu6bhz5zyt2jb5e5ht462o2nrwq7odjnpwyr23tkf2s2plg", StringComparison.Ordinal));
        }

        [TestMethod()]
        public void TestEncrypt()
        {
            var array = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var enc = Helper.Encrypt(array, "P@ssw0rd1");
             var array0 = Helper.Decrypt(enc, "p@ssw0rd1");
            Assert.IsTrue(array0 == null);
            var array1 = Helper.Decrypt(enc, "P@ssw0rd1");
            Assert.IsTrue(array.SequenceEqual(array1));
        }

        [TestMethod()]
        public void GetBitmapFromImgDataTest()
        {
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out _)) {
                Assert.Fail();
            }

            imgdata = File.ReadAllBytes("cor.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out _)) {
                Assert.Fail();
            }

            imgdata = File.ReadAllBytes("noimg.jpg");
            if (Helper.GetBitmapFromImgData(imgdata, out _)) {
                Assert.Fail();
            }

            imgdata = File.ReadAllBytes("orgpng.png");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            if (!Helper.GetImgDataFromBitmap(bitmap, out var jpgdata)) {
                Assert.Fail();
            }

            File.WriteAllBytes("orgpng.jpg", jpgdata);
        }

        [TestMethod()]
        public void GetThumpFromBitmapTest()
        {
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            using (var thump = Helper.GetThumpFromBitmap(bitmap)) {
                thump.Save("org_bw256.jpg");
            }
        }

        [TestMethod()]
        public void FlifTest()
        {
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap_org)) {
                Assert.Fail();
            }

            var h1 = Helper.GetHashFromBitmap(bitmap_org);

            //Helper.GetPerceptualHash(bitmap_org, out var x);

            imgdata = File.ReadAllBytes("orgpng.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap_png)) {
                Assert.Fail();
            }

            var h2 = Helper.GetHashFromBitmap(bitmap_png);

            //Helper.GetPerceptualHash(bitmap_png, out var y);
            //var d = Helper.GetPerceptualHashDistance(x, y);


            /*
            if (!Helper.GetFlifFromBitmap(bitmap, out byte[] flifdata)) {
                Assert.Fail();
            }

            File.WriteAllBytes("org.flif", flifdata);
            if (!Helper.GetBitmapFromImgData(flifdata, out Bitmap flifbitmap)) {
                Assert.Fail();
            }
            */
        }
    }
}