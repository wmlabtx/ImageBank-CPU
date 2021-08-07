using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class ImageHelperTests
    {
        /*
        [TestMethod()]
        public void GetAverageAngleTest()
        {
            var ka1 = new short[] { 10, 10 };
            var avg1 = ImageHelper.GetAverageAngle(ka1);
            Assert.AreEqual(avg1, 10);

            var ka2 = new short[] { 0, 10, 350 };
            var avg2 = ImageHelper.GetAverageAngle(ka2);
            Assert.AreEqual(avg2, 0);

            var ka3 = new short[] { 90, 180, 270, 360 };
            var avg3 = ImageHelper.GetAverageAngle(ka3);
            Assert.AreEqual(avg3, 270);
        }
        */

        [TestMethod()]
        public void ComputeKazeDescriptorsTest()
        {
            var filename = "k1024.jpg";
            using (var image = Image.FromFile(filename)) {
                ImageHelper.ComputeKazeDescriptors((Bitmap)image, out var ki, out var kx, out var ky);
            }
        }

        [TestMethod()]
        public void KazeBulkTest()
        {
            var img1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeKazeDescriptors((Bitmap)img1, out var ki1, out var kx1, out var ky1);

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
            foreach (var filename in files)
            {
                var img2 = Image.FromFile(filename);
                ImageHelper.ComputeKazeDescriptors((Bitmap)img2, out var ki2, out var kx2, out var ky2, out var ki2mirror, out var kx2mirror, out var ky2mirror);

                var sim = ImageHelper.GetSimRandom(ki1, kx1, ky1, ki2, kx2, ky2, ki2mirror, kx2mirror, ky2mirror);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: sim={sim:F2}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}