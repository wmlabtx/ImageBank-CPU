using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageBank.Tests
{
    [TestClass()]
    public class BitmapHelperTests
    {
        [TestMethod()]
        public void ImageDataToBitmapSourceTest()
        {
            var imagedata = File.ReadAllBytes("testnotimage.jpg");
            var bitmap_notimage = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNull(bitmap_notimage);

            imagedata = File.ReadAllBytes("testjpg.jpg");
            var bitmap_jpg = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap_jpg);
            Assert.AreEqual(bitmap_jpg.PixelFormat, PixelFormat.Format24bppRgb);
            bitmap_jpg.Save("testjpg_.jpg");

            imagedata = File.ReadAllBytes("testjpg8.jpg");
            var bitmap8_jpg = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap8_jpg);
            Assert.AreEqual(bitmap8_jpg.PixelFormat, PixelFormat.Format24bppRgb);
            bitmap8_jpg.Save("testjpg8_.jpg");

            imagedata = File.ReadAllBytes("testpng.png");
            var bitmap_png = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap_png);
            bitmap_png.Save("testpng_.jpg");

            imagedata = File.ReadAllBytes("testwebp.webp");
            var bitmap_webp = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap_webp);
            bitmap_webp.Save("testwebp_.jpg");

            imagedata = File.ReadAllBytes("testheic.heic");
            var bitmap_heic = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap_heic);
            bitmap_webp.Save("testheic_.jpg");

            imagedata = File.ReadAllBytes("testgif.gif");
            var bitmap_gif = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap_gif);
            bitmap_gif.Save("testgif_.jpg");
        }

        [TestMethod()]
        public void BitmapToImageDataTest()
        {
            var imagedata = File.ReadAllBytes("testjpg.jpg");
            var bitmap_jpg = BitmapHelper.ImageDataToBitmap(imagedata);
            Assert.IsNotNull(bitmap_jpg);
            Assert.AreEqual(bitmap_jpg.PixelFormat, PixelFormat.Format24bppRgb);

            var jxlimagedata = BitmapHelper.BitmapToImageData(bitmap_jpg);
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

        [TestMethod()]
        public void GetMatrixTest()
        {
            using (var bitmap3x5 = new Bitmap(3, 5, PixelFormat.Format24bppRgb)) {
                bitmap3x5.SetPixel(0, 4, Color.White);
                var imagedata = BitmapHelper.BitmapToImageData(bitmap3x5);
                Assert.IsNotNull(imagedata);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                Assert.AreEqual(matrix[0][0], 0);
                Assert.AreEqual(matrix[0][2], 0);
                Assert.AreEqual(matrix[4][0], 100);
                Assert.AreEqual(matrix[4][2], 0);
            }
        }
    }
}