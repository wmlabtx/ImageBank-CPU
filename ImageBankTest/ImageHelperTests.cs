using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
                if (!ImageHelper.GetDescriptors((Bitmap)img1, out Mat d, out Mat m)) {
                    Assert.Fail();
                }

                m.SaveImage("gab_org.png");
                m.Dispose();
            }
        }

        /*
        [TestMethod()]
        public void PopulateNodesTest()
        {
            ImgMdf.SqlTruncateAll();
            ImgMdf.LoadImgs(null);

            for (var i = 1; i <= 30; i++) {
                var name = $"train\\train{i:D2}.jpg";
                var imagedata = File.ReadAllBytes(name);
                if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                    continue;
                }

                if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                    bitmap = ImageHelper.RepixelBitmap(bitmap);
                }

                if (!ImageHelper.GetKazeDescriptors(bitmap, out KazeDescriptor[][] kazedescriptors, out Mat[] mat)) {
                    Assert.Fail();
                }

                bitmap.Dispose();
                var pngname = Path.ChangeExtension(name, AppConsts.PngExtension);
                mat[0].SaveImage(pngname);
                mat[1].SaveImage($"{pngname}.mirror.png");
                var descriptors = ImgMdf.GetDescriptors(kazedescriptors);
                Assert.IsTrue(descriptors != null);
            }
        }
        */

        [TestMethod()]
        public void GetSimTest()
        {
            var sb = new StringBuilder();
            var idx = File.ReadAllBytes(names[0]);
            foreach (var name in names) {
                var idy = File.ReadAllBytes(name);
                var distance = ImageHelper.GetDistance(idx, idy);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{name}: distance={distance:F2}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}
 