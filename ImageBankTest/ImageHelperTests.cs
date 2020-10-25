using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImageBank.Tests
{
    [TestClass()]
    public class ImageHelperTests
    {
        [TestMethod()]
        public void ComputeRGBHashTest()
        {
            var hb = ImageHelper.ComputeRGBHash(0, 0, 0);
            Assert.AreEqual(0, hb);
            var hw = ImageHelper.ComputeRGBHash(255, 0, 255);
            Assert.AreEqual(0xFF00FF, hw);
        }

        [TestMethod()]
        public void ComputeLABHashTest()
        {
            ImageHelper.ConvertToLAB(255, 0, 0, out var l, out var a, out var b);
            var k1 = ImageHelper.ComputeLABHash(l, a, b);
            ImageHelper.ConvertToLAB(k1, out var l1, out var a1, out var b1);
            var k2 = ImageHelper.ComputeLABHash(l1, a1, b1);
            Assert.AreEqual(k1, k2);
        }

        [TestMethod()]
        public void DistanceTest()
        {
            ImageHelper.ConvertToLAB(255, 0, 0, out var l1, out var a1, out var b1);
            ImageHelper.ConvertToLAB(225, 0, 33, out var l2, out var a2, out var b2);
            var d0 = ImageHelper.Distance(l1, a1, b1, l1, a1, b1);
            Assert.AreEqual(0.0, d0);
            var d1 = ImageHelper.Distance(l1, a1, b1, l2, a2, b2);
            var d2 = ImageHelper.Distance(l2, a2, b2, l1, a1, b1);
            Assert.AreEqual(d1, d2);
            var random = new Random();
            var count = 0;
            var sum = 0.0;
            for (var i = 0; i < 1000000; i++) {
                var r = random.Next(256);
                var g = random.Next(256);
                var b = random.Next(256);
                ImageHelper.ConvertToLAB(r, g, b, out l1, out a1, out b1);
                r = random.Next(256);
                g = random.Next(256);
                b = random.Next(256);
                ImageHelper.ConvertToLAB(r, g, b, out l2, out a2, out b2);
                var d = ImageHelper.Distance(l1, a1, b1, l2, a2, b2);
                count++;
                sum += d;
            }

            var avg = sum / count;
            Assert.IsTrue(avg < 100.0);
        }

        [TestMethod()]
        public void ComputeLabsTest()
        {
            var image = Image.FromFile("org.jpg");
            if (!ImageHelper.ComputeLabs((Bitmap)image, out var labs)) {
                Assert.Fail();
            }
        }

        private ColorLAB[] GetDescriptors(string filename)
        {
            var imagedata = File.ReadAllBytes(filename);
            if (!Helper.GetBitmapFromImageData(imagedata, out Bitmap bitmap)) {
                Assert.Fail();
            }

            //if (!SpectreHelper.Compute(bitmap, out var spectre)) {
            //    Assert.Fail();
            //}

            //return spectre;

            return null;
        }

        [TestMethod()]
        public void GetDistanceTest()
        {
            var baseline = GetDescriptors("org.jpg");
            var files = new[] {
                "org_png.jpg",
                "org_resized.jpg",
                "org_nologo.jpg", 
                "org_r10.jpg", 
                "org_r90.jpg", 
                "org_sim1.jpg",
                "org_sim2.jpg",
                "org_crop.jpg",
                "org_nosim1.jpg",
                "org_nosim2.jpg",
                "org_mirror.jpg"
            };

            var sb = new StringBuilder();
            foreach (var filename in files) {
                //var descriptors = GetDescriptors(filename);
                //var distance = SpectreHelper.GetDistance(baseline, descriptors);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                //sb.Append($"{filename}: {distance:F4}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}