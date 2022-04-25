using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageBank;
using OpenCvSharp;

namespace ImageBankTest
{
    /// <summary>
    /// Summary description for LabHelperTests
    /// </summary>
    [TestClass]
    public class LabHelperTests
    {
        [TestMethod]
        public void GetLab()
        {
            var imagedata = File.ReadAllBytes("gab_org.jpg");
            var lab = LabHelper.GetLab(imagedata, "gab_org");
            Assert.IsNotNull(lab);
        }

        [TestMethod]
        public void GetDistance()
        {
            var images = new[] {
                "gab_org", "gab_bw", "gab_scale", "gab_flip", "gab_r90", "gab_crop", "gab_toside",
                "gab_blur", "gab_exp", "gab_logo", "gab_noice", "gab_r3", "gab_r10",
                "gab_face", "gab_sim1", "gab_sim2",
                "gab_nosim1", "gab_nosim2", "gab_nosim3", "gab_nosim4", "gab_nosim5", "gab_nosim6"
            };

            var vectors = new Tuple<string, Mat>[images.Length];
            for (var i = 0; i < images.Length; i++) {
                var name = $"{images[i]}.jpg";
                var imagedata = File.ReadAllBytes(name);
                var matlab = LabHelper.GetLab(imagedata, name);
                Assert.IsNotNull(matlab);
                vectors[i] = new Tuple<string, Mat>(name, matlab);
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = LabHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
        }
    }
}
