using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageBank;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Linq;
using System.Text;
using System.Windows;

namespace ImageBankTest
{
    /// <summary>
    /// Summary description for LabHelperTests
    /// </summary>
    [TestClass]
    public class VggTests
    {
        [TestMethod]
        public void GetVector()
        {
            VggHelper.LoadNetwork();
            var imagedata = File.ReadAllBytes("gab_org.jpg");
            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                var vector = VggHelper.CalculateVector(bitmap);
                Assert.AreEqual(vector.Length, 4096);
            }
        }

        [TestMethod]
        public void TestDistance()
        {
            var x0 = new float[0];
            var x1 = new float[] { -1.1f, -16.6f, 0.5f };
            var x2 = new float[] { -1.1f, -16.6f, 0.49f };
            var x3 = new float[] { 4.5f, -1.9f, -3.8f };

            var d0 = VggHelper.GetDistance(x0, x1);
            Assert.AreEqual(d0, 1f);
            var d1 = VggHelper.GetDistance(x1, x2);
            Assert.IsTrue(d1 < 0.1f);
            var d2 = VggHelper.GetDistance(x1, x3);
            Assert.IsTrue(d2 > 0.1f);
        }

        [TestMethod]
        public void GetDistance()
        {
            VggHelper.LoadNetwork();
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
                    Assert.AreEqual(vector.Length, 4096);
                    vectors[i] = new Tuple<string, float[]>(name, vector);
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = VggHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
        }

        /*
        [TestMethod]
        public void ToQuantVector()
        {
            Debug.WriteLine("Loading network");
            VggHelper.LoadNetwork();
            AppImgs.Clear();
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var keys = AppImgs.GetKeys();
            for (var i = 0; i < keys.Length; i++) {
                var id = keys[i];
                var vector = AppDatabase.ImageGetVector(id);
                if (vector == null || vector.Length != 4096) {
                    continue;
                }

                var quant = VggHelper.QuantVector(vector);
                AppDatabase.ImageUpdateProperty(id, AppConsts.AttributeQuantVector, quant);
                Debug.WriteLine($"{i}: {id}");
            }
        }
        */

        /*
        [TestMethod]
        public void TestVectorHash()
        {
            var v1 = new float[] { -140f, -50f, -40f, 18f, 20f };
            var q1 = VggHelper.QuantVector(v1);

            AppImgs.Clear();
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var keys = AppImgs.GetKeys();
            var list = new List<Tuple<int, float[], byte[]>>();
            while (list.Count < 5) {
                var pos = AppVars.IRandom(0, keys.Length - 1);
                var id = keys[pos];
                var vector = AppDatabase.ImageGetVector(id);
                if (vector == null || vector.Length != 4096) {
                    continue;
                }

                var q = VggHelper.QuantVector(vector);
                list.Add(new Tuple<int, float[], byte[]>(id, vector, q));
            }

            for (var i = 0; i < list.Count -1; i++) {
                for (var j = i + 1; j < list.Count; j++) {
                    var distance = VggHelper.GetDistance(list[i].Item2, list[j].Item2);
                    var qdistance = VggHelper.GetDistance(list[i].Item3, list[j].Item3);
                    Debug.WriteLine($"{i}-{j}: {distance:F2} / {qdistance:F4}");
                }
            }
        }
        */

        /*
        [TestMethod]
        public void TestW()
        {
            AppImgs.Clear();
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var keys = AppImgs.GetKeys();
            var array = new float[4096 * 100000];
            var count = 0;
            while (count < 100000) {
                var pos = AppVars.IRandom(0, keys.Length - 1);
                var id = keys[pos];
                var vector = AppDatabase.ImageGetVector(id);
                if (vector == null || vector.Length != 4096) {
                    continue;
                }

                Array.Copy(vector, 0, array, count * 4096, 4096);
                count++;
            }

            Array.Sort(array);
            var step = array.Length / 255;
            for (var i = 0; i < 255; i++) {
                Debug.Write($"{array[i * step]:F4},");
            }

            Debug.WriteLine("");
            // -135.9650,-44.0282,-39.5161,-36.8418,-34.9228,-33.4218,-32.1846,-31.1279,-30.2048,-29.3827,-28.6422,-27.9676,-27.3475,-26.7724,-26.2370,-25.7355,-25.2633,-24.8168,-24.3937,-23.9907,-23.6054,-23.2368,-22.8828,-22.5436,-22.2174,-21.9017,-21.5972,-21.3033,-21.0178,-20.7404,-20.4718,-20.2095,-19.9549,-19.7067,-19.4648,-19.2286,-18.9981,-18.7728,-18.5520,-18.3361,-18.1246,-17.9176,-17.7142,-17.5148,-17.3191,-17.1266,-16.9374,-16.7517,-16.5689,-16.3889,-16.2117,-16.0370,-15.8651,-15.6958,-15.5290,-15.3643,-15.2019,-15.0417,-14.8834,-14.7270,-14.5728,-14.4202,-14.2692,-14.1199,-13.9724,-13.8264,-13.6821,-13.5394,-13.3982,-13.2586,-13.1203,-12.9829,-12.8472,-12.7124,-12.5791,-12.4471,-12.3160,-12.1862,-12.0575,-11.9298,-11.8031,-11.6777,-11.5527,-11.4289,-11.3062,-11.1841,-11.0627,-10.9422,-10.8224,-10.7036,-10.5856,-10.4684,-10.3519,-10.2362,-10.1211,-10.0066,-9.8927,-9.7795,-9.6671,-9.5550,-9.4436,-9.3330,-9.2226,-9.1126,-9.0031,-8.8942,-8.7859,-8.6780,-8.5707,-8.4637,-8.3572,-8.2511,-8.1453,-8.0400,-7.9351,-7.8306,-7.7261,-7.6224,-7.5190,-7.4158,-7.3128,-7.2102,-7.1079,-7.0057,-6.9039,-6.8023,-6.7010,-6.5998,-6.4991,-6.3984,-6.2979,-6.1974,-6.0972,-5.9972,-5.8974,-5.7975,-5.6979,-5.5985,-5.4991,-5.3998,-5.3004,-5.2014,-5.1023,-5.0034,-4.9043,-4.8056,-4.7068,-4.6079,-4.5090,-4.4100,-4.3112,-4.2121,-4.1131,-4.0141,-3.9150,-3.8158,-3.7165,-3.6170,-3.5173,-3.4177,-3.3177,-3.2177,-3.1174,-3.0170,-2.9166,-2.8161,-2.7153,-2.6142,-2.5129,-2.4111,-2.3091,-2.2069,-2.1042,-2.0013,-1.8981,-1.7945,-1.6904,-1.5859,-1.4814,-1.3763,-1.2705,-1.1643,-1.0574,-0.9501,-0.8423,-0.7340,-0.6249,-0.5154,-0.4053,-0.2941,-0.1827,-0.0702,0.0429,0.1568,0.2715,0.3871,0.5037,0.6213,0.7397,0.8596,0.9804,1.1021,1.2252,1.3494,1.4752,1.6022,1.7305,1.8601,1.9911,2.1243,2.2589,2.3953,2.5335,2.6737,2.8162,2.9606,3.1074,3.2568,3.4086,3.5629,3.7204,3.8810,4.0445,4.2116,4.3824,4.5572,4.7361,4.9195,5.1078,5.3016,5.5009,5.7057,5.9179,6.1364,6.3628,6.5976,6.8428,7.0976,7.3640,7.6439,7.9387,8.2507,8.5814,8.9338,9.3125,9.7215,10.1682,10.6609,11.2121,11.8381,12.5678,13.4462,14.5595,16.0949,18.6595
        }
        */
    }
}
