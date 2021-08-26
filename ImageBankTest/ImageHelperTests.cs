using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;

namespace ImageBankTest
{
    [TestClass()]
    public class ImageHelperTests
    {
        private readonly string[] names = new string[] {
            "gab_org.jpg", "gab_scale.jpg", "gab_crop.jpg", "gab_blur.jpg", "gab_exp.jpg", "gab_face.jpg", "gab_flip.jpg",
            "gab_logo.jpg", "gab_noice.jpg", "gab_r3.jpg", "gab_r10.jpg", "gab_r90.jpg", "gab_sim1.jpg", "gab_sim2.jpg",
            "gab_nosim1.jpg", "gab_nosim2.jpg", "gab_nosim3.jpg", "gab_nosim4.jpg", "gab_nosim5.jpg"
        };

        [TestMethod()]
        public void GetDescriptorsTest()
        {
            var filename = "gab_org.jpg";
            using (var img1 = Image.FromFile(filename)) {
                var x = ImageHelper.GetBothDescriptors((Bitmap)img1, out Mat[] m);
                Assert.IsTrue(m[0] != null);
                Assert.IsTrue(m[1] != null);
                Assert.IsTrue((x[0].Width == 128) && (x[0].Height > 0) && (x[0].Height <= AppConsts.MaxDescriptors));
                Assert.IsTrue((x[1].Width == 128) && (x[1].Height > 0) && (x[1].Height <= AppConsts.MaxDescriptors));
                m[0].SaveImage("gab_org.png");
                m[1].SaveImage("gab_org_mirror.png");
            }
        }

        [TestMethod()]
        public void GetSimTest()
        {
            var fimages = new List<Tuple<string, Mat[], Mat>>();
            foreach (var name in names) {
                using (var img = Image.FromFile(name)) {
                    var descriptors = ImageHelper.GetBothDescriptors((Bitmap)img, out Mat[] mat);
                    var moments = ImageHelper.GetColorMoments((Bitmap)img);
                    Assert.IsTrue(descriptors != null);
                    var pngname = Path.ChangeExtension(name, AppConsts.PngExtension);
                    mat[0].SaveImage(pngname);
                    fimages.Add(new Tuple<string, Mat[], Mat>(name, descriptors, moments));
                }
            }

            var sb = new StringBuilder();
            foreach (var e in fimages) {

                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                var sim = ImageHelper.GetSim(fimages[0].Item2[0], e.Item2);
                var distance = ImageHelper.GetLogDistance(fimages[0].Item3, e.Item3);
                sb.Append($"{e.Item1}: sim={sim:F2} distance={distance:F2}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }

}