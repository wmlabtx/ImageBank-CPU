using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp.Dnn;
using OpenCvSharp;
using System.Diagnostics;
using ImageBank;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Web;

namespace ImageBankTest
{
    [TestClass]
    public class VggTests
    {
        [TestMethod()]
        public void GetVector()
        {
            var NetFile = "resnet152-v2-7.onnx";
            var net = CvDnn.ReadNetFromOnnx(NetFile);
            var layernames = net.GetLayerNames();
            foreach (var layername in layernames) {
                Debug.WriteLine(layername);
            }
            
            var frame = Cv2.ImRead("train\\train12.jpg");
            var inpBlob = CvDnn.BlobFromImage(
                image: frame,
                scaleFactor: 1.0 / 255,
                size: new OpenCvSharp.Size(224, 224),
                mean: new Scalar(0.485, 0.456, 0.406),
                swapRB: false,
                crop: true);

            var dims = inpBlob.Dims;
            var size0 = inpBlob.Size(0);
            var size1 = inpBlob.Size(1);
            var size2 = inpBlob.Size(2);
            var size3 = inpBlob.Size(3);
            Debug.WriteLine($"{dims}: {size0}x{size1}x{size2}x{size3}");

            net.SetInput(inpBlob);
            var output = net.Forward("onnx_node!resnetv27_flatten0_reshape0");

            // onnx_node!resnetv27_relu1_fwd - 1x2048x7x7
            // onnx_node!resnetv27_pool1_fwd - 1x2048
            // onnx_node!resnetv27_flatten0_reshape0 - 1x2048
            // onnx_node!resnetv27_dense0_fwd - 1x1000

            dims = output.Dims;
            size0 = output.Size(0);
            size1 = output.Size(1);
            size2 = output.Size(2);
            size3 = output.Size(3);
            Debug.WriteLine($"{dims}: {size0}x{size1}x{size2}x{size3}");

            //var vector = new float[2048];

            output.GetArray(out float[] data);
            VggHelper.LoadNet(null);
            var imagedata = File.ReadAllBytes("train\\train12.jpg");
            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata, RotateFlipType.RotateNoneFlipNone)) {
                var v2 = VggHelper.CalculateVector(bitmap);
            }
        }

        [TestMethod]
        public void GetDistance()
        {
            /*
            VggHelper.LoadNet(null);
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
                    var vector = VggHelper.CalculateVector(bitmap);
                    Assert.AreEqual(vector.Length, 2048);
                    vectors[i] = new Tuple<string, float[]>(name, vector);
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = VggHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
            */
        }

        [TestMethod]
        public void GetStatistics()
        {
            var hist = new SortedList<int, int>();
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var counter = 0;
            var keys = AppImgs.GetKeys();
            foreach (var key in keys) {
                if (!AppImgs.TryGetValue(key, out Img imgX)) {
                    continue;
                }

                var vector = imgX.GetVector();
                foreach (var e in vector) {
                    var q = (int)(e * 10);
                    if (hist.ContainsKey(q)) {
                        hist[q]++;
                    }
                    else {
                        hist.Add(q, 1);
                    }
                }

                Debug.WriteLine($"{counter}: {key.Substring(0, 5)}");
                counter++;
            }

            foreach (var key in hist.Keys) {
                var k = key / 10.0;
                var v = hist[key];
                Debug.WriteLine($"{k:F1} {v}");
            }
        }
    }
}
