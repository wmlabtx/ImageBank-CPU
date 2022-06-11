using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class BitmapHelperTests
    {
        /*
        [TestMethod()]
        public void ImageDataToBitmapSourceTest()
        {
            var imagedata = File.ReadAllBytes("testnotimage.jpg");
            var bitmapNotimage = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNull(bitmapNotimage);

            imagedata = File.ReadAllBytes("testjpg.jpg");
            var bitmapJpg = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmapJpg);
            Assert.AreEqual(bitmapJpg.PixelFormat, PixelFormat.Format24bppRgb);
            bitmapJpg.Save("testjpg_.jpg");

            imagedata = File.ReadAllBytes("testjpg8.jpg");
            var bitmap8Jpg = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap8Jpg);
            Assert.AreEqual(bitmap8Jpg.PixelFormat, PixelFormat.Format24bppRgb);
            bitmap8Jpg.Save("testjpg8_.jpg");

            imagedata = File.ReadAllBytes("testpng.png");
            var bitmapPng = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmapPng);
            bitmapPng.Save("testpng_.jpg");

            imagedata = File.ReadAllBytes("testwebp.webp");
            var bitmapWebp = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmapWebp);
            bitmapWebp.Save("testwebp_.jpg");

            imagedata = File.ReadAllBytes("testheic.heic");
            var bitmapHeic = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmapHeic);
            bitmapWebp.Save("testheic_.jpg");

            imagedata = File.ReadAllBytes("testgif.gif");
            var bitmapGif = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmapGif);
            bitmapGif.Save("testgif_.jpg");
        }

        [TestMethod]
        public void BitmapToImageDataTest()
        {
            var imagedata = File.ReadAllBytes("testjpg.jpg");
            var bitmapJpg = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmapJpg);
            Assert.AreEqual(bitmapJpg.PixelFormat, PixelFormat.Format24bppRgb);

            var jxlimagedata = BitmapHelper.BitmapToImageData(bitmapJpg);
            Assert.IsNotNull(jxlimagedata);
            File.WriteAllBytes("testjxl.jxl", jxlimagedata);
        }

        [TestMethod()]
        public void GetBrightnessTest()
        {
            var lblack = BitmapHelper.GetBrightness(0, 0, 0);
            Assert.IsTrue(Math.Abs(lblack - 0.00) < 0.01);
            var lred = BitmapHelper.GetBrightness(255, 0, 0);
            Assert.IsTrue(Math.Abs(lred - 53.23) < 0.01);
            var lgreen = BitmapHelper.GetBrightness(0, 255, 0);
            Assert.IsTrue(Math.Abs(lgreen - 87.74) < 0.01);
            var lblue = BitmapHelper.GetBrightness(0, 0, 255);
            Assert.IsTrue(Math.Abs(lblue - 32.30) < 0.01);
            var lwhite = BitmapHelper.GetBrightness(255, 255, 255);
            Assert.IsTrue(Math.Abs(lwhite - 100.00) < 0.01);
        }

        [TestMethod]
        public void GetMatrixTest()
        {
            using (var bitmap3X5 = new Bitmap(3, 5, PixelFormat.Format24bppRgb)) {
                bitmap3X5.SetPixel(0, 4, Color.White);
                var imagedata = BitmapHelper.BitmapToImageData(bitmap3X5);
                Assert.IsNotNull(imagedata);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                Assert.AreEqual(matrix[0][0], 0);
                Assert.AreEqual(matrix[0][2], 0);
                Assert.AreEqual(matrix[4][0], 100);
                Assert.AreEqual(matrix[4][2], 0);
            }
        }
        */
    }
}