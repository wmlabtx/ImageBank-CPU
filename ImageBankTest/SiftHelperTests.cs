using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace ImageBank.Tests
{
    [TestClass()]
    public class SiftHelperTests
    {
        [TestMethod()]
        public void PopulateTest()
        {
            /*
            if (File.Exists(AppConsts.FileSiftNodes)) {
                File.Delete(AppConsts.FileSiftNodes);
            }

            for (var i = 1; i < 30; i++) {
                var name = $"train\\train{i:D2}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                var descriptors = SiftHelper.GetDescriptors(matrix);
                SiftHelper.Populate(descriptors);
            }

            SiftHelper.SaveNodes();
            for (var i = 1; i < 30; i++) {
                var name = $"train\\train{i:D2}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                var descriptors = SiftHelper.GetDescriptors(matrix);
                var vector = SiftHelper.ComputeVector(descriptors);
                Assert.IsNotNull(vector);
            }
            */
        }

        [TestMethod()]
        public void ComparisonTest()
        {
            /*
            var images = new string[] {
                "gab_org", "gab_bw", "gab_scale", "gab_flip", "gab_r90", "gab_crop", "gab_toside",
                "gab_blur", "gab_exp", "gab_logo", "gab_noice", "gab_r3", "gab_r10",
                "gab_face", "gab_sim1", "gab_sim2",
                "gab_nosim1", "gab_nosim2", "gab_nosim3", "gab_nosim4", "gab_nosim5", "gab_nosim6"
            };

            SiftHelper.LoadNodes();
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                var descriptors = SiftHelper.GetDescriptors(matrix);
                SiftHelper.Populate(descriptors);
            }

            SiftHelper.SaveNodes();

            var vectors = new int[images.Length][];
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                var descriptors = SiftHelper.GetDescriptors(matrix);
                var vector = SiftHelper.ComputeVector(descriptors);
                Assert.IsNotNull(vector);
                vectors[i] = vector;
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = SiftHelper.GetDistance(vectors[0], vectors[i]);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
            */
        }
    }
}

