using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class ImageHelperTests
    {
        [TestMethod()]
        public void ComputeFeaturePointsTest()
        {
            FeaturePoint2[] x, y;

            var filename = "org.jpg";
            using (var img1 = Image.FromFile(filename)) {
                ImageHelper.ComputeFeaturePoints2((Bitmap)img1, out x);
            }

            filename = "org_png.jpg";
            using (var img2 = Image.FromFile(filename)) {
                ImageHelper.ComputeFeaturePoints2((Bitmap)img2, out y);
            }

            var sim = ImageHelper.GetSim2(x, y);
        }

        [TestMethod()]
        public void KazeBulkTest()
        {
            var img1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeFeaturePoints2((Bitmap)img1, out var fp1);
            //var rv1 = ImageHelper.GetRandomVector(fp1);

            var files = new[] {
                "org.jpg",
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
                ImageHelper.ComputeFeaturePoints2((Bitmap)img2, out var fp2, out var fp2mirror);

                var sim = ImageHelper.GetSim2(fp1, fp2, fp2mirror);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: sim={sim:F2}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }

        [TestMethod()]
        public void KazeBulkTest2()
        {
            var img1 = Image.FromFile("arianna-048-001.jpg");
            ImageHelper.ComputeFeaturePoints2((Bitmap)img1, out var fp1);
            //var rv1 = ImageHelper.GetRandomVector(fp1);

            var files = new[] {
                "arianna-048-009.jpg",
                "arianna-048-063.jpg",
                "arianna-048-093.jpg",
                "arianna-048-113.jpg",
                "org_nosim1.jpg"
            };

            var sb = new StringBuilder();
            foreach (var filename in files) {
                var img2 = Image.FromFile(filename);
                ImageHelper.ComputeFeaturePoints2((Bitmap)img2, out var fp2, out var fp2mirror);

                var sim = ImageHelper.GetSim2(fp1, fp2, fp2mirror);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: sim={sim:F4}");
            }

            File.WriteAllText("report2.txt", sb.ToString());
        }
    }
}