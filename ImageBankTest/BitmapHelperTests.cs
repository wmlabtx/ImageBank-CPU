using ImageBank;
using ImageMagick;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace ImageBankTest
{
    [TestClass()]
    public class BitmapHelperTests
    {
        [TestMethod()]
        public void Color1Test() {
            BitmapHelper.RGB2ITP(0, 0, 0, out double id1, out double td1, out double pd1);
            BitmapHelper.RGB2ITP(255, 255, 255, out double id2, out double td2, out double pd2);
            BitmapHelper.RGB2ITP(255, 0, 0, out double id3, out double td3, out double pd3);

            var imin = double.MaxValue;
            var imax = double.MinValue;
            var tmin = double.MaxValue;
            var tmax = double.MinValue;
            var pmin = double.MaxValue;
            var pmax = double.MinValue;
            for (var r = 0; r < 256; r++) {
                for (var g = 0; g < 256; g++) {
                    for (var b = 0; b < 256; b++) {
                        BitmapHelper.RGB2ITP(r, g, b, out double id, out double td, out double pd);
                        imin = Math.Min(imin, id);
                        imax = Math.Max(imax, id);
                        tmin = Math.Min(tmin, td);
                        tmax = Math.Max(tmax, td);
                        pmin = Math.Min(pmin, pd);
                        pmax = Math.Max(pmax, pd);
                    }
                }
            }

             Debug.WriteLine($"{imin:F2} {imax:F2} {tmin:F2} {tmax:F2} {pmin:F2} {pmax:F2}");
        }

        private static void TestMagickImageParameters(string filename, MagickFormat format, int channelcount, int depth)
        {
            /*
            var imagedata = File.ReadAllBytes(filename);
            using (var mi = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                Assert.AreEqual(mi.Format, format);
                Assert.AreEqual(mi.ChannelCount, channelcount);
                Assert.AreEqual(mi.Depth, depth);
            }
            */
        }

        [TestMethod()]
        public void ImageDataToBitmapSourceTest()
        {
            /*
            var imagedata = File.ReadAllBytes("testnotimage.jpg");
            var miNotimage = BitmapHelper.ImageDataToMagickImage(imagedata);
            Assert.IsNull(miNotimage);

            TestMagickImageParameters("magick\\testjpg.jpg", MagickFormat.Jpeg, 3, 8);
            TestMagickImageParameters("magick\\testjpg8.jpg", MagickFormat.Jpeg, 2, 8);
            TestMagickImageParameters("magick\\testpng.png", MagickFormat.Png, 3, 8);
            TestMagickImageParameters("magick\\testwebp.webp", MagickFormat.WebP, 3, 8);
            TestMagickImageParameters("magick\\testheic.heic", MagickFormat.Heic, 3, 8);
            TestMagickImageParameters("magick\\testgif.gif", MagickFormat.Gif, 5, 8);
            */
        }

        /*
        private static bool VerifyMagickImage(string filename)
        {
            
            bool result;
            var imagedata = File.ReadAllBytes(filename);
            using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                if (magickImage == null) {
                    result = false;
                }
                else {
                    if (magickImage.Format == MagickFormat.Jpeg) {
                        if (
                            imagedata.Length > 16 && 
                            imagedata[0] == 0xFF && imagedata[1] == 0xD8 &&
                            imagedata[imagedata.Length - 2] == 0xFF && imagedata[imagedata.Length - 1] == 0xD9) {
                            result = true;
                        }
                        else {
                            result = false;
                        }
                    }
                    else {
                        result = true;
                    }
                }
            }

            return result;

        }

        [TestMethod()]
        public void VerificationTest()
        {
            Assert.IsTrue(VerifyMagickImage("magick\\testjpg.jpg"));
            Assert.IsFalse(VerifyMagickImage("magick\\corrupted1.jpg"));
            Assert.IsFalse(VerifyMagickImage("magick\\corrupted2.jpg"));
            Assert.IsFalse(VerifyMagickImage("magick\\corrupted3.jpg"));
        }
        */

        /*
        [TestMethod]
        public void BitmapToImageDataTest()
        {
            var imagedata = File.ReadAllBytes("magick\\testheic.heic");
            var output = "magick\\testoutput$.jxl";
            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata, System.Drawing.RotateFlipType.RotateNoneFlipNone)) {
                Assert.IsNotNull(bitmap);
                Assert.AreEqual(bitmap.PixelFormat, PixelFormat.Format24bppRgb);

                if (!BitmapHelper.BitmapToImageData(bitmap, MagickFormat.Jxl, out byte[] newimagedata)) {
                    Assert.Fail();
                }

                Assert.IsNotNull(newimagedata);
                File.WriteAllBytes(output, newimagedata);
            }
        }
        */

        /*
        private static DateTime GetDateTaken(string filename)
        {
            var imagedata = File.ReadAllBytes(filename);
            var defaultValue = File.GetLastWriteTime(filename);
            using (var mi = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                var dt = BitmapHelper.GetDateTaken(mi, defaultValue);
                return dt;
            }
        }

        [TestMethod]
        public void GetDateTakenTest()
        {
            var dt = GetDateTaken("magick\\testjpg.jpg");
            Assert.AreEqual(dt.Year, 2013);
            dt = GetDateTaken("magick\\exif2002.jpg");
            Assert.AreEqual(dt.Year, 2002);
            dt = GetDateTaken("magick\\exif2010.jpg");
            Assert.AreEqual(dt.Year, 2010);
            dt = GetDateTaken("magick\\exifnone.jpg");
            Assert.AreEqual(dt.Year, 2021);
        }
        */
    }
}