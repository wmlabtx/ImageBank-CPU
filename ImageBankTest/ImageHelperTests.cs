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
        public void ComputeDescriptorsTest()
        {
            var image = Image.FromFile("k1024.jpg");
            if (!ImageHelper.ComputeDescriptors((Bitmap)image, out var descriptors)) {
                Assert.Fail();
            }

            var array = ImageHelper.DescriptorsToArray(descriptors);
            var d2 = ImageHelper.ArrayToDescriptors(array);
            Assert.AreEqual(descriptors[0].Vector[3], d2[0].Vector[3]);
        }
        */

        /*
        private OrbDescriptorX[] GetDescriptors(string filename)
        {
            var image = Image.FromFile(filename);
            if (!ImageHelper.ComputeDescriptors((Bitmap)image, out var descriptors)) {
                Assert.Fail();
            }

            return descriptors;
        }
        */

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