using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Drawing;
using System;
using System.Linq;
using System.Drawing.Imaging;

namespace ImageBank.Tests
{
    [TestClass()]
    public class HelperTests
    {
        [TestMethod()]
        public void ComputeHash3216Test()
        {
            var array = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var hash1 = Helper.ComputeHash3216(array);
            Assert.IsTrue(hash1.Equals("ygetmyp5lfmlmzlf", StringComparison.Ordinal));
            array[0] = 0x04;
            var hash2 = Helper.ComputeHash3216(array);
            Assert.IsTrue(hash2.Equals("jlnnu6bhz5zyt2jb", StringComparison.Ordinal));
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
            /*
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out _, out _)) {
                Assert.Fail();
            }

            imgdata = File.ReadAllBytes("cor.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out _, out _)) {
                Assert.Fail();
            }

            imgdata = File.ReadAllBytes("noimg.jpg");
            if (Helper.GetBitmapFromImgData(imgdata, out _, out _)) {
                Assert.Fail();
            }

            imgdata = File.ReadAllBytes("orgpng.png");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap, out _)) {
                Assert.Fail();
            }

            if (!Helper.GetImgDataFromBitmap(bitmap, out var flifdata)) {
                Assert.Fail();
            }

            File.WriteAllBytes("org.flif", flifdata);
            */
        }

        [TestMethod()]
        public void GetThumpFromBitmapTest()
        {
            /*
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap, out _)) {
                Assert.Fail();
            }

            using (var thump = Helper.GetThumpFromBitmap(bitmap)) {
                thump.Save("org_bw256.jpg");
            }
            */
        }

        [TestMethod()]
        public void FlifTest()
        {
            /*
            var imgdata = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap_org, out _)) {
                Assert.Fail();
            }

            //Helper.GetPerceptualHash(bitmap_org, out var x);

            imgdata = File.ReadAllBytes("orgpng.jpg");
            if (!Helper.GetBitmapFromImgData(imgdata, out Bitmap bitmap_png, out _)) {
                Assert.Fail();
            }

            //Helper.GetPerceptualHash(bitmap_png, out var y);
            //var ylen = y.Length;
            //var d = Helper.GetPerceptualHashDistance(x, y);


            
            if (!Helper.GetImgDataFromBitmap(bitmap_org, out byte[] webpdata)) {
                Assert.Fail();
            }

            File.WriteAllBytes("org.webp", webpdata);
            if (!Helper.GetBitmapFromImgData(webpdata, out Bitmap webpbitmap, out _)) {
                Assert.Fail();
            }
            */
        }

        [TestMethod()]
        public void WebPTest()
        {
            /*
            var data = File.ReadAllBytes("org.jpg");
            if (!Helper.GetBitmapFromImageData(data, out var bitmap)) {
                Assert.Fail();
            }

            if (bitmap.RawFormat != ImageFormat.Jpeg) {
                Assert.Fail();
            }

            if (!Helper.GetImageDataFromBitmap(bitmap, out byte[] webpdata)) {
                Assert.Fail();
            }

            if (!Helper.GetBitmapFromImageData(webpdata, out var bitmap_out)) {
                Assert.Fail();
            }

            File.WriteAllBytes("org.webp", webpdata); ;
            */
        }
    }
}