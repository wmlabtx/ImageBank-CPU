using System.Diagnostics;
using System.IO;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class RootSiftHelperTests
    {
        [TestMethod()]
        public void ComputeTest()
        {
            const string name = "gab_org.jpg";
            var imagedata = File.ReadAllBytes(name);
            var matrix = BitmapHelper.GetMatrix(imagedata);
            Assert.IsNotNull(matrix);
            var descriptors = RootSiftHelper.Compute(matrix, true);
            Assert.IsNotNull(descriptors);
        }

        [TestMethod()]
        public void InitLearnTest()
        {
            NeuralGas.Clear();

            for (var i = 1; i < 30; i++) {
                var name = $"train\\train{i:D2}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                var descriptors = RootSiftHelper.Compute(matrix, true);
                NeuralGas.LearnDescriptors(descriptors);
                NeuralGas.Save();
            }

            for (var i = 1; i < 30; i++) {
                var name = $"train\\train{i:D2}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                var descriptors = RootSiftHelper.Compute(matrix, true);
                NeuralGas.Compute(descriptors, out ushort[] vector, out float minerror, out float maxerror);
                Debug.WriteLine($"minerror = {minerror:F2}, maxerror = {minerror:F2}");
            }
        }

        [TestMethod()]
        public void ComparisonTest()
        {
            var images = new string[] {
                "gab_org", "gab_bw", "gab_scale", "gab_flip", "gab_r90", "gab_crop", "gab_toside",
                "gab_blur", "gab_exp", "gab_logo", "gab_noice", "gab_r3", "gab_r10",
                "gab_face", "gab_sim1", "gab_sim2",
                "gab_nosim1", "gab_nosim2", "gab_nosim3", "gab_nosim4", "gab_nosim5", "gab_nosim6"
            };

            /*
            var images = new string[] {
                "gab_org", "gab_flip"
            };
            */

            NeuralGas.Load();

            var vectors = new ushort[images.Length][];
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                var descriptors = RootSiftHelper.Compute(matrix, true);
                NeuralGas.LearnDescriptors(descriptors);
                NeuralGas.Save();
                NeuralGas.Compute(descriptors, out var vector, out var minerror, out var maxerror);
                Debug.WriteLine($"average error = {minerror:F2} / {maxerror:F2}");
                vectors[i] = vector;
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = RootSiftHelper.GetDistance(vectors[0], vectors[i]);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
        }
    }
}

