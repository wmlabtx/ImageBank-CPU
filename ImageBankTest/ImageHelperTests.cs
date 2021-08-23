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
                ImageHelper.GetVectors((Bitmap)img1, out ulong[] x, out Mat m);
                Assert.IsTrue(x != null && m != null);
                var length = x.Length * sizeof(ulong);
                Assert.IsTrue((length % 32 == 0) && (length / 32 <= AppConsts.MaxDescriptors));
                m.SaveImage("gab_org.png");
                m.Dispose();
            }
        }

        [TestMethod()]
        public void SqlPopulateNodesTest()
        {
            ImgMdf.SqlTruncateNodes();
            ImgMdf.SqlTruncateDescriptors();
            var count = ImgMdf.SqlGetNodesCount();
            Assert.IsTrue(count == 1);
            var nodeid = ImgMdf.SqlGetAvailableNodeId();
            Assert.IsTrue(nodeid == 2);
            var rootnode = ImgMdf.SqlGetNode(1);
            Assert.IsTrue(rootnode.NodeId == 1 && rootnode.ChildId == 0 && rootnode.Radius == 0 && rootnode.Core.Length == 0);
            count = ImgMdf.SqlGetDescriptorsCount();
            Assert.IsTrue(count == 0);

            foreach (var name in names) {
                using (var img = Image.FromFile(name)) {
                    ImageHelper.GetVectors((Bitmap)img, out ulong[][] vectors, out Mat[] mat);
                    Assert.IsTrue(vectors != null);
                    for (var i = 0; i < 2; i++) {
                        var length = vectors[i].Length * sizeof(ulong);
                        Assert.IsTrue((length % 32 == 0) && (length / 32 <= AppConsts.MaxDescriptors));
                        if (i == 0) {
                            var pngname = Path.ChangeExtension(name, AppConsts.PngExtension);
                            mat[i].SaveImage(pngname);
                        }

                        mat[i].Dispose();
                        ImgMdf.SqlPopulateDescriptors(vectors[i]);
                    }
                }
            }
        }

        [TestMethod()]
        public void GetSimTest()
        {
            var fimages = new List<Tuple<string, int[][]>>();
            foreach (var name in names) {
                using (var img = Image.FromFile(name)) {
                    ImageHelper.GetVectors((Bitmap)img, out ulong[][] vectors, out Mat[] mat);
                    Assert.IsTrue(vectors != null && mat != null);
                    mat[0].Dispose();
                    mat[1].Dispose();
                    ImgMdf.SqlGetFeatures(vectors, out int[][] features);
                    fimages.Add(new Tuple<string, int[][]>(name, features));
                }
            }

            var sb = new StringBuilder();
            foreach (var e in fimages) {

                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                var sim = ImageHelper.GetSim(fimages[0].Item2[0], e.Item2);
                sb.Append($"{e.Item1}: sim={sim:F1}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }

}