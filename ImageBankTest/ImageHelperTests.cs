using System;
using System.Diagnostics;
using System.Drawing;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class ImageHelperTests
    {
        private readonly Random _random = new Random(0);

        [TestMethod()]
        public void ComputeDescriptorsTest()
        {
            var image = Image.FromFile("k1024.jpg");
            if (!ImageHelper.ComputeDescriptors((Bitmap)image, out var blob)) {
                Assert.Fail();
            }

            var descriptors = ImageHelper.ComputeDescriptors(blob);
            var distance = ImageHelper.GetDistance(descriptors, descriptors, 255);
            Assert.IsTrue(distance < 0.1f);
        }

        [TestMethod()]
        public void GetDistanceTest()
        {
            var image1 = Image.FromFile("org_png.jpg");
            if (!ImageHelper.ComputeDescriptors((Bitmap)image1, out var blob1))
            {
                Assert.Fail();
            }

            var descriptors1 = ImageHelper.ComputeDescriptors(blob1);

            var image2 = Image.FromFile("org_resized.jpg");
            if (!ImageHelper.ComputeDescriptors((Bitmap)image2, out var blob2))
            {
                Assert.Fail();
            }

            var descriptors2 = ImageHelper.ComputeDescriptors(blob2);
            var distance12 = ImageHelper.GetDistance(descriptors1, descriptors2, 64);

            var image3 = Image.FromFile("org_nosim1.jpg");
            if (!ImageHelper.ComputeDescriptors((Bitmap)image3, out var blob3))
            {
                Assert.Fail();
            }

            var descriptors3 = ImageHelper.ComputeDescriptors(blob3);
            var distance13 = ImageHelper.GetDistance(descriptors1, descriptors3, 64);


            var image4 = Image.FromFile("org_compressed.jpg");
            if (!ImageHelper.ComputeDescriptors((Bitmap)image4, out var blob4))
            {
                Assert.Fail();
            }

            var descriptors4 = ImageHelper.ComputeDescriptors(blob4);
            var distance14 = ImageHelper.GetDistance(descriptors1, descriptors4, 64);
        }

        [TestMethod()]
        public void Performance()
        {
            const int blobsize = AppConsts.MaxDescriptorsInImage * 64;
            var blob = new byte[blobsize];
            _random.NextBytes(blob);

            var x = ImageHelper.ComputeDescriptors(blob);
            var y = ImageHelper.ComputeDescriptors(blob);
            Array.Reverse(y);

            var counter = 0;
            var sum = 0L;
            var sw = Stopwatch.StartNew();
            while (counter < 100)
            {
                sw.Restart();
                var distance = ImageHelper.GetDistance(x, y, 255);
                sw.Stop();
                sum += sw.ElapsedMilliseconds;
                counter++;
            }

            var avg = sum / counter;
        }

        /*
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
        "org_bwresized.jpg",
        "org_compressed.jpg",
        "org_sim1.jpg",
        "org_sim2.jpg",
        "org_crop.jpg",
        "org_nosim1.jpg",
        "org_nosim2.jpg",
        "org_mirror.jpg",
        "k1024.jpg"
    };

    var sb = new StringBuilder();
    foreach (var filename in files) {
        var descriptors = GetDescriptors(filename);
        var sim = ImageHelper.GetSim(baseline, descriptors);
        if (sb.Length > 0) {
            sb.AppendLine();
        }

        sb.Append($"{filename}: {sim:F4}");
    }

    File.WriteAllText("report.txt", sb.ToString());
}
*/
    }
    }