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
        [TestMethod()]
        public void Populate()
        {
            ImgMdf.Populate();
        }

        [TestMethod()]
        public void DrawPalette()
        {
            ImgMdf.DrawPalette();
        }

        [TestMethod]
        public void GetColors()
        {
            using (var matcollector = new Mat()) {
                var imagedata = File.ReadAllBytes("gab_org.jpg");
                var matpixels = LabHelper.GetColors(imagedata);
                Assert.IsNotNull(matpixels);
                using (var matfloat = new Mat()) {
                    matpixels.ConvertTo(matfloat, MatType.CV_32F);
                    matcollector.PushBack(matfloat);                    
                }

                Assert.Equals(matcollector.Cols, 3);
            }
        }

        [TestMethod]
        public void GetLab()
        {
            var imagedata = File.ReadAllBytes("gab_org.jpg");
            var matcolors = LabHelper.GetColors(imagedata);
            Assert.IsNotNull(matcolors);

            ImgMdf.LoadImages(null);
            var h1 = LabHelper.GetLab(matcolors, ImgMdf.GetCenters());
            var distance = LabHelper.GetDistance(h1 , h1);
            Assert.IsTrue(distance < 0.0001f);
        }

        [TestMethod]
        public void GetDistance()
        {
            ImgMdf.LoadImages(null);
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
                var matcolors = LabHelper.GetColors(imagedata);
                var hist = LabHelper.GetLab(matcolors, ImgMdf.GetCenters());
                vectors[i] = new Tuple<string, float[]>(name, hist);
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = LabHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
        }
    }
}
