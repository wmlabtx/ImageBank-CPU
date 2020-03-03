using ImageBank;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Drawing;
using System.Linq;

namespace ImageBank.Tests
{
    [TestClass()]
    public class HelperTest
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
        public void TestVectorBuffer()
        {
            var array1 = new float[] { 0.1f, 0f, 0f, 0f, 0.2f };
            var buffer1 = Helper.VectorToBuffer(array1);
            var array2 = Helper.BufferToVector(buffer1);
            Assert.IsTrue(array1.Length == array2.Length);
            Assert.IsTrue(Math.Abs(array1[4] - array2[4]) < 0.01);
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
    }
}