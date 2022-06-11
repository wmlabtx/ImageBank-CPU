using System;
using System.Diagnostics;
using System.IO;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class RootSiftHelperTests
    {
        /*
        [TestMethod]
        public void ComparisonTest()
        {
            var images = new[] {
                "gab_org", "gab_bw", "gab_scale", "gab_flip", "gab_r90", "gab_crop", "gab_toside",
                "gab_blur", "gab_exp", "gab_logo", "gab_noice", "gab_r3", "gab_r10",
                "gab_face", "gab_sim1", "gab_sim2",
                "gab_nosim1", "gab_nosim2", "gab_nosim3", "gab_nosim4", "gab_nosim5", "gab_nosim6"
            };

            var vectors = new Tuple<string, uint>[images.Length];
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                uint hist = BitmapHelper.GetHist(matrix);
                vectors[i] = new Tuple<string, uint>(name, hist);
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = BitmapHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance}");
            }
        }

        [TestMethod]
        public void ComparisonTest2()
        {
            var images = new[] {
                "gab_org", "gab_bw", "gab_scale", "gab_flip", "gab_r90", "gab_crop", "gab_toside",
                "gab_blur", "gab_exp", "gab_logo", "gab_noice", "gab_r3", "gab_r10",
                "gab_face", "gab_sim1", "gab_sim2",
                "gab_nosim1", "gab_nosim2", "gab_nosim3", "gab_nosim4", "gab_nosim5", "gab_nosim6"
            };

            var vectors = new Tuple<string, RootSiftDescriptor[][], ulong[][]>[images.Length];
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matrix = BitmapHelper.GetMatrix(imagedata);
                Assert.IsNotNull(matrix);
                RootSiftHelper.Compute(matrix, out var descriptors, true);
                var fingerprints = RootSiftHelper.GetFingerprints(descriptors);
                vectors[i] = new Tuple<string, RootSiftDescriptor[][], ulong[][]>(name, descriptors, fingerprints);
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = RootSiftHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                var fdistance = RootSiftHelper.GetDistance(vectors[0].Item3, vectors[i].Item3);
                Debug.WriteLine($"{images[i]} = {distance:F3} / {fdistance:F2}");
            }
        }
        */
    }
}

