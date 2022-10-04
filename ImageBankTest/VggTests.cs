using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageBank;

namespace ImageBankTest
{
    /// <summary>
    /// Summary description for LabHelperTests
    /// </summary>
    [TestClass]
    public class VggTests
    {
        [TestMethod]
        public void GetVector()
        {
            VggHelper.LoadNetwork();
            var imagedata = File.ReadAllBytes("gab_org.jpg");
            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                var vector = VggHelper.CalculateVector(bitmap);
                Assert.AreEqual(vector.Length, 4096);
            }
        }

        [TestMethod]
        public void TestDistance()
        {
            var x0 = new float[0];
            var x1 = new float[] { -1.1f, -16.6f, 0.5f };
            var x2 = new float[] { -1.1f, -16.6f, 0.49f };
            var x3 = new float[] { 4.5f, -1.9f, -3.8f };

            var d0 = VggHelper.GetDistance(x0, x1);
            Assert.AreEqual(d0, 1f);
            var d1 = VggHelper.GetDistance(x1, x2);
            Assert.IsTrue(d1 < 0.1f);
            var d2 = VggHelper.GetDistance(x1, x3);
            Assert.IsTrue(d2 > 0.1f);
        }

        [TestMethod]
        public void GetDistance()
        {
            VggHelper.LoadNetwork();
            var images = new[] {
                "gab_org", "gab_bw", "gab_scale", "gab_flip", "gab_r90", "gab_crop", "gab_toside",
                "gab_blur", "gab_exp", "gab_logo", "gab_noice", "gab_r3", "gab_r10",
                "gab_face", "gab_sim1", "gab_sim2",
                "gab_nosim1", "gab_nosim2", "gab_nosim3", "gab_nosim4", "gab_nosim5", "gab_nosim6"
            };

            var vectors = new Tuple<string, float[]>[images.Length];
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    var vector = VggHelper.CalculateVector(bitmap);
                    Assert.AreEqual(vector.Length, 4096);
                    vectors[i] = new Tuple<string, float[]>(name, vector);
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = VggHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
        }
    }
}
