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
        public static readonly ImgMdf Collection = new ImgMdf();
        private readonly string[] names = new string[] {
            "gab_org.jpg", "gab_scale.jpg", "gab_crop.jpg", "gab_blur.jpg", "gab_exp.jpg", "gab_face.jpg", "gab_flip.jpg",
            "gab_logo.jpg", "gab_noice.jpg", "gab_r3.jpg", "gab_r10.jpg", "gab_r90.jpg", "gab_sim1.jpg", "gab_sim2.jpg",
            "gab_nosim1.jpg", "gab_nosim2.jpg", "gab_nosim3.jpg", "gab_nosim4.jpg", "gab_nosim5.jpg"
        };

        [TestMethod()]
        public void GetDescriptorsTest()
        {
            var filename = "gab_org.jpg";
            var imgdata = File.ReadAllBytes(filename);
            if (!ImageHelper.GetBitmapFromImageData(imgdata, out var bitmap)) {
                Assert.Fail();
            }

            try {
                var descriptors = ImageHelper.GetSiftDescriptors(bitmap);
                if (descriptors == null) {
                    Assert.Fail();
                }
            }
            finally {
                if (bitmap != null) {
                    bitmap.Dispose();
                }
            }
        }

        /*
        [TestMethod()]
        public void PopulateNodesTest()
        {
            ImgMdf.LoadNodes(null);
            for (var i = 1; i <= 30; i++) {
                var filename = $"train\\train{i:D2}.jpg";
                var imgdata = File.ReadAllBytes(filename);
                if (!ImageHelper.GetBitmapFromImageData(imgdata, out var bitmap)) {
                    Assert.Fail();
                }

                try {
                    var descriptors = ImageHelper.GetDescriptors(bitmap);
                    if (descriptors == null) {
                        Assert.Fail();
                    }

                    //var ki = ImgMdf.GetKi(descriptors);
                }
                finally {
                    if (bitmap != null) {
                        bitmap.Dispose();
                    }
                }
            }
        }
        */

        [TestMethod()]
        public void GetDistanceTest()
        {
            var array = new Tuple<string, Mat[]>[names.Length];
            for (var i = 0; i < names.Length; i++) {
                var filename = names[i];
                var imgdata = File.ReadAllBytes(filename);
                if (!ImageHelper.GetBitmapFromImageData(imgdata, out var bitmap)) {
                    Assert.Fail();
                }

                try {
                    var descriptors = ImageHelper.GetSift2Descriptors(bitmap);
                    if (descriptors == null) {
                        Assert.Fail();
                    }

                    array[i] = new Tuple<string, Mat[]>(filename, descriptors);
                }
                finally {
                    if (bitmap != null) {
                        bitmap.Dispose();
                    }
                }
            }

            var sb = new StringBuilder();
            for (var i = 0; i < names.Length; i++) {
                var distance = ImageHelper.GetDistance(array[0].Item2[0], array[i].Item2);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{array[i].Item1}: distance={distance}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }
    }
}
 