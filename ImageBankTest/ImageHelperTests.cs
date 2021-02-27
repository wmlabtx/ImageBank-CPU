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
        public void ComputeBlobTest()
        {
            var image = Image.FromFile("org.jpg");
            ImageHelper.ComputeBlob((Bitmap)image, out var h, out var d);
        }

        [TestMethod()]
        public void CompareBlobTest()
        {
            var image1 = Image.FromFile("arianna-048-001.jpg");
            ImageHelper.ComputeBlob((Bitmap)image1, out var h1, out var d1);

            var image2 = Image.FromFile("arianna-048-009.jpg");
            ImageHelper.ComputeBlob((Bitmap)image2, out var h2, out var d2);

            var result = ImageHelper.CompareBlob(d1, d2);
        }

        [TestMethod()]
        public void GetDistanceBulkTest()
        {
            var image1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeBlob((Bitmap)image1, out var h1, out var d1);
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
                ImageHelper.ComputeBlob((Bitmap)image2, out var h2, out var d2);
                var result = ImageHelper.CompareBlob(d1, d2);
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: {result:F4}");
            }

            sb.AppendLine();

            image1 = Image.FromFile("arianna-048-001.jpg");
            ImageHelper.ComputeBlob((Bitmap)image1, out h1, out d1);

            files = new[] {
                "arianna-048-009.jpg",
                "arianna-048-063.jpg",
                "arianna-048-093.jpg",
                "arianna-048-113.jpg",
                "k1024.jpg"
            };

            foreach (var filename in files)
            {
                var image2 = Image.FromFile(filename);
                ImageHelper.ComputeBlob((Bitmap)image2, out var h2, out var d2);
                var result = ImageHelper.CompareBlob(d1, d2);
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: {result}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}