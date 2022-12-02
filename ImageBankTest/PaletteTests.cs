using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageBank;
using System.Reflection;
using System.Drawing;

namespace ImageBankTest
{
    /// <summary>
    /// Summary description for LabHelperTests
    /// </summary>
    /// 
    [TestClass]
    public class PaletteTests
    {
        /*
        [TestMethod()]
        public void TestColors()
        {
            byte rb1 = 0;
            byte gb1 = 0;
            byte bb1 = 0;
            ColorHelper.ToLAB(rb1, gb1, bb1, out var l1, out var a1, out var b1);
            Debug.WriteLine($"{rb1} {gb1} {bb1} = {l1:F1} {a1:F1} {b1:F1}");
            ColorHelper.ToRGB(l1, a1, b1, out var rb1x, out var gb1x, out var bb1x);
            Assert.AreEqual(rb1, rb1x);
            Assert.AreEqual(gb1, gb1x);
            Assert.AreEqual(bb1, bb1x);

            byte rb2 = 255;
            byte gb2 = 255;
            byte bb2 = 255;
            ColorHelper.ToLAB(rb2, gb2, bb2, out var l2, out var a2, out var b2);
            Debug.WriteLine($"{rb2} {gb2} {bb2} = {l2:F1} {a2:F1} {b2:F1}");
            ColorHelper.ToRGB(l2, a2, b2, out var rb2x, out var gb2x, out var bb2x);
            Assert.AreEqual(rb2, rb2x);
            Assert.AreEqual(gb2, gb2x);
            Assert.AreEqual(bb2, bb2x);

            var d12_1976 = ColorHelper.Cie1976(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1976 = {d12_1976:F1}");
            var d12_1994 = ColorHelper.Cie1994(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1994 = {d12_1994:F1}");
            var d12_2000 = ColorHelper.Cie2000(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_2000 = {d12_2000:F1}");

            rb1 = 192;
            gb1 = 0;
            bb1 = 0;
            ColorHelper.ToLAB(rb1, gb1, bb1, out l1, out a1, out b1);
            Debug.WriteLine($"{rb1} {gb1} {bb1} = {l1:F1} {a1:F1} {b1:F1}");
            ColorHelper.ToRGB(l1, a1, b1, out rb1x, out gb1x, out bb1x);
            Assert.AreEqual(rb1, rb1x);
            Assert.AreEqual(gb1, gb1x);
            Assert.AreEqual(bb1, bb1x);

            rb2 = 224;
            gb2 = 0;
            bb2 = 0;
            ColorHelper.ToLAB(rb2, gb2, bb2, out l2, out a2, out b2);
            Debug.WriteLine($"{rb2} {gb2} {bb2} = {l2:F1} {a2:F1} {b2:F1}");

            d12_1976 = ColorHelper.Cie1976(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1976 = {d12_1976:F1}");
            d12_1994 = ColorHelper.Cie1994(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1994 = {d12_1994:F1}");
            d12_2000 = ColorHelper.Cie2000(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_2000 = {d12_2000:F1}");

            rb1 = 0;
            gb1 = 0;
            bb1 = 192;
            ColorHelper.ToLAB(rb1, gb1, bb1, out l1, out a1, out b1);
            Debug.WriteLine($"{rb1} {gb1} {bb1} = {l1:F1} {a1:F1} {b1:F1}");

            rb2 = 0;
            gb2 = 0;
            bb2 = 224;
            ColorHelper.ToLAB(rb2, gb2, bb2, out l2, out a2, out b2);
            Debug.WriteLine($"{rb2} {gb2} {bb2} = {l2:F1} {a2:F1} {b2:F1}");

            d12_1976 = ColorHelper.Cie1976(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1976 = {d12_1976:F1}");
            d12_1994 = ColorHelper.Cie1994(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1994 = {d12_1994:F1}");
            d12_2000 = ColorHelper.Cie2000(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_2000 = {d12_2000:F1}");

            rb1 = 0;
            gb1 = 192;
            bb1 = 0;
            ColorHelper.ToLAB(rb1, gb1, bb1, out l1, out a1, out b1);
            Debug.WriteLine($"{rb1} {gb1} {bb1} = {l1:F1} {a1:F1} {b1:F1}");

            rb2 = 0;
            gb2 = 224;
            bb2 = 0;
            ColorHelper.ToLAB(rb2, gb2, bb2, out l2, out a2, out b2);
            Debug.WriteLine($"{rb2} {gb2} {bb2} = {l2:F1} {a2:F1} {b2:F1}");

            d12_1976 = ColorHelper.Cie1976(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1976 = {d12_1976:F1}");
            d12_1994 = ColorHelper.Cie1994(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_1994 = {d12_1994:F1}");
            d12_2000 = ColorHelper.Cie2000(l1, a1, b1, l2, a2, b2);
            Debug.WriteLine($"d12_2000 = {d12_2000:F1}");

            var lmin = double.MaxValue;
            var lmax = double.MinValue;
            var amin = double.MaxValue;
            var amax = double.MinValue;
            var bmin = double.MaxValue;
            var bmax = double.MinValue;
            for (var rb = 0; rb < 256; rb++) {
                for (var gb = 0; gb < 256; gb++) {
                    for (var bb = 0; bb < 256; bb++) {
                        ColorHelper.ToLAB((byte)rb, (byte)gb, (byte)bb, out l1, out a1, out b1);
                        lmin = Math.Min(lmin, l1);
                        lmax = Math.Max(lmax, l1);
                        amin = Math.Min(amin, a1);
                        amax = Math.Max(amax, a1);
                        bmin = Math.Min(bmin, b1);
                        bmax = Math.Max(bmax, b1);
                    }
                }
            }

            Debug.WriteLine($"l = [{lmin:F2}, {lmax:F2}]");
            Debug.WriteLine($"a = [{amin:F2}, {amax:F2}]");
            Debug.WriteLine($"b = [{bmin:F2}, {bmax:F2}]");

            // l = [0.00, 100.00]
            // a = [-86.18, 98.25]
            // b = [-107.86, 94.48]
        }

        [TestMethod()]
        public void CreatePalette()
        {
            AppPalette.Create();
        }

        [TestMethod()]
        public void LearnPalette()
        {
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var counter = 0;
            var keys = AppImgs.GetKeys();
            while (counter < 10000) {
                var randomkey = keys[AppVars.IRandom(0, keys.Length - 1)];
                if (!AppImgs.TryGetValue(randomkey, out Img imgX)) {
                    continue;
                }

                var name = imgX.Name;
                var filename = FileHelper.NameToFileName(name);
                if (!File.Exists(filename)) {
                    continue;
                }

                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    continue;
                }

                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    if (bitmap == null) {
                        continue;
                    }

                    var sumerror = AppPalette.Learn(bitmap);
                    Debug.WriteLine($"{counter}: {sumerror:F1}");
                    counter++;
                }
            }
        }

        [TestMethod()]
        public void CalculateHists()
        {
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var counter = 0;
            var keys = AppImgs.GetKeys();
            foreach (var key in keys) { 
                if (!AppImgs.TryGetValue(key, out Img imgX)) {
                    continue;
                }
                var hist = imgX.GetHist();
                if (hist != null && hist.Length > 0) {
                    continue;
                }

                var name = imgX.Name;
                var filename = FileHelper.NameToFileName(name);
                if (!File.Exists(filename)) {
                    continue;
                }

                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    continue;
                }

                using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                    if (bitmap == null) {
                        continue;
                    }

                    hist = AppPalette.ComputeHist(bitmap);
                    imgX.SetHist(hist);

                    var size = Helper.SizeToString(imagedata.Length);
                    Debug.WriteLine($"{counter}: {key} - {size} ({bitmap.Width}x{bitmap.Height}) ");
                    counter++;
                }
            }
        }

        [TestMethod]
        public void GetDistance()
        {
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
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
                    var hist = AppPalette.ComputeHist(bitmap);
                    vectors[i] = new Tuple<string, float[]>(name, hist);
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = AppPalette.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance:F2}");
            }
        }

        [TestMethod()]
        public void ExportFiles()
        {
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var counter = 0;
            var keys = AppImgs.GetKeys();
            foreach (var key in keys) {
                if (!AppImgs.TryGetValue(key, out Img imgX)) {
                    continue;
                }

                var name = imgX.Name;
                var filename = FileHelper.NameToFileName(name);
                if (!File.Exists(filename)) {
                    continue;
                }

                var imagedata = FileHelper.ReadData(filename);
                if (imagedata == null) {
                    continue;
                }

                var f = name.Substring(0, 2);
                var n = name.Substring(2, 8);
                var newfilename = $"M:\\Le\\{f}\\{n}.jpg";
                var dir = Path.GetDirectoryName(newfilename);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(newfilename, imagedata);
                var size = Helper.SizeToString(imagedata.Length);
                Debug.WriteLine($"{counter}: {key} - {size}");
                counter++;
            }
        }
        */
    }
}
