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
        [TestMethod()]
        public void ComputeKpDescriptors()
        {
            var filename = "k1024.jpg";
            using (var image = Image.FromFile(filename)) {
                ImageHelper.ComputeKpDescriptors((Bitmap)image, out var b1, out var bm1);
            }
        }

        [TestMethod()]
        public void KpBulkTest()
        {
            var image1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeKpDescriptors((Bitmap)image1, out var b1, out var _);

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
                var image2 = Image.FromFile(filename);
                ImageHelper.ComputeKpDescriptors((Bitmap)image2, out var b2, out var bm2);

                var m = ImageHelper.ComputeKpMatch(b1, b2, bm2);

                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: m={m}");
            }

            sb.AppendLine();

            image1 = Image.FromFile("000817-01-27.jpg");
            ImageHelper.ComputeKpDescriptors((Bitmap)image1, out b1, out var _);
            files = new[] {
                "000817-01-29.jpg",
                "000817-01-31.jpg",
                "000817-01-32.jpg",
                "000817-01-33.jpg",
                "000817-01-34.jpg",
                "000809-01-32.jpg",
                "020808-036.jpg",
                "020808-106.jpg",
                "020808-107.jpg",
                "020808-108.jpg",
                "2816-2893.jpg",
                "2816-2938.jpg",
                "7e8f4c20.jpg",
                "k1024.jpg"
            };

            foreach (var filename in files) {
                var image2 = Image.FromFile(filename);
                ImageHelper.ComputeKpDescriptors((Bitmap)image2, out var b2, out var bm2);

                var m = ImageHelper.ComputeKpMatch(b1, b2, bm2);

                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: m={m}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}