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
        private static readonly ImgMdf Collection = new ImgMdf();
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
                ImageHelper.GetDescriptors((Bitmap)img1, out Mat x, out KeyPoint[] keypoints, out Mat m);
                x.GetArray(out float[] fx);
                Assert.IsTrue(x != null && keypoints != null && m != null);
                Assert.IsTrue((fx.Length % 128 == 0) && (fx.Length / 128 <= AppConsts.MaxDescriptors) && keypoints.Length > 0);
                m.SaveImage("gab_org.png");
                m.Dispose();
            }
        }

        [TestMethod()]
        public void PopulateNodesTest()
        {
            ImgMdf.SqlTruncateNodes();
            ImgMdf.ClearNodes();

            for (var i = 1; i <= 30; i++) {
                var name = $"train\\train{i:D2}.jpg";
                var imagedata = File.ReadAllBytes(name);
                if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                    continue;
                }

                if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                    bitmap = ImageHelper.RepixelBitmap(bitmap);
                }

                ImageHelper.GetDescriptors(bitmap, out Mat[] descriptors, out KeyPoint[][] keypoints, out Mat mat);
                bitmap.Dispose();
                Assert.IsTrue(descriptors != null);
                var pngname = Path.ChangeExtension(name, AppConsts.PngExtension);
                mat.SaveImage(pngname);
                mat.Dispose();
                for (var j = 0; j < 2; j++) {
                    descriptors[j].GetArray(out float[] fdescriptors);
                    var num = fdescriptors.Length / 128;
                    Assert.IsTrue(num > 0 && num <= AppConsts.MaxDescriptors);
                    ImgMdf.AddDescriptors(fdescriptors);
                }
            }
        }

        [TestMethod()]
        public void GetSimTest()
        {
            ImgMdf.LoadImgs(null);

            var fimages = new List<Tuple<string, short[][], Mat[], KeyPoint[][]>>();
            foreach (var name in names) {
                using (var img = Image.FromFile(name)) {
                    ImageHelper.GetDescriptors((Bitmap)img, out Mat[] descriptors, out KeyPoint[][] keypoints, out Mat mat);
                    Assert.IsTrue(descriptors != null);
                    var pngname = Path.ChangeExtension(name, AppConsts.PngExtension);
                    mat.SaveImage(pngname);
                    mat.Dispose();
                    var fdescriptors = new float[2][];
                    descriptors[0].GetArray(out fdescriptors[0]);
                    descriptors[0].GetArray(out fdescriptors[1]);
                    var ki = ImgMdf.GetKi(fdescriptors);
                    fimages.Add(new Tuple<string, short[][], Mat[], KeyPoint[][]>(name, ki, descriptors, keypoints));
                }
            }

            var sb = new StringBuilder();
            foreach (var e in fimages) {

                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                var fsim = ImgMdf.GetSim(fimages[0].Item2[0], e.Item2);
                var sim = ImageHelper.GetSim(fimages[0].Item3[0], fimages[0].Item4[0], e.Item3, e.Item4);
                sb.Append($"{e.Item1}: fsim={fsim:F2} sim={sim:F2}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }

}