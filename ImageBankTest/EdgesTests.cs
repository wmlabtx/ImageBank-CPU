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
    public class EdgesTests
    {

        [TestMethod]
        public void GetSim()
        {
            /*
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
                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    var palette = ImgMdf.ComputePalette(bitmap);
                    vectors[i] = new Tuple<string, float[]>(name, palette);
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var palettedistance = ImgMdf.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {palettedistance:F2}");
            }
            */
        }
    }
}
