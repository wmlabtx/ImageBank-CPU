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
        public void ComputeVectorTest()
        {
            float[] v1, v2;

            var filename = "org.jpg";
            using (var image = Image.FromFile(filename)) {
                ImageHelper.ComputeVector((Bitmap)image, out v1);
            }

            filename = "org_png.jpg";
            using (var image = Image.FromFile(filename)) {
                ImageHelper.ComputeVector((Bitmap)image, out v2);
            }
        }

        [TestMethod()]
        public void ComputeKazeDescriptorsTest()
        {
            var filename = "k1024.jpg";
            using (var image = Image.FromFile(filename)) {
                ImageHelper.ComputeFeaturePoints((Bitmap)image, out var fp);
            }
        }

        [TestMethod()]
        public void KazeBulkTest()
        {
            var img1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeVector((Bitmap)img1, out var v1);

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
                var imgdata2 = File.ReadAllBytes(filename);
                ImageHelper.GetBitmapFromImageData(imgdata2, out var bmp2);
                if (bmp2.PixelFormat != PixelFormat.Format24bppRgb) {
                    bmp2 = ImageHelper.RepixelBitmap(bmp2);
                }

                ImageHelper.ComputeVector(bmp2, out var v2, out var v2mirror);

                var sim = ImageHelper.GetCosineSimilarity(v1, v2, v2mirror);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: sim={sim:F2}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}