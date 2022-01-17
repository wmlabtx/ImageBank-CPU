using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace ImageBank.Tests
{
    /*
    [TestClass()]
    public class OnnxTest
    {
        [TestMethod()]
        public void ComparisonTest()
        {
            var images = new string[] {
                "gab_org", "gab_bw", "gab_scale", "gab_flip", "gab_r90", "gab_crop", "gab_toside",
                "gab_blur", "gab_exp", "gab_logo", "gab_noice", "gab_r3", "gab_r10",
                "gab_face", "gab_sim1", "gab_sim2",
                "gab_nosim1", "gab_nosim2", "gab_nosim3", "gab_nosim4", "gab_nosim5", "gab_nosim6"
            };

            var vectors = new float[images.Length][];
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    BitmapHelper.ComputeVector(bitmap, out float[] vector);
                    Assert.IsNotNull(vector);
                    vectors[i] = vector;
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = BitmapHelper.GetCosineSimilarity(vectors[0], vectors[i]);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
        }
    }
    */
}
