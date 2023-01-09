using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageBank;
using System.Drawing;

namespace ImageBankTest
{
    [TestClass]
    public class PaletteTests
    {
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
        public void AverageDistance()
        {
            var counter = 0.0;
            var dmax = 0.0;
            var sum = 0.0;
            byte rb, gb, bb; 
            for (var i = 0; i < 256 * 256 * 256;  i++) {
                rb = (byte)AppVars.IRandom(0, 255);
                gb = (byte)AppVars.IRandom(0, 255);
                bb = (byte)AppVars.IRandom(0, 255);
                ColorHelper.ToLAB(rb, gb, bb, out var l1, out var a1, out var b1);
                rb = (byte)AppVars.IRandom(0, 255);
                gb = (byte)AppVars.IRandom(0, 255);
                bb = (byte)AppVars.IRandom(0, 255);
                ColorHelper.ToLAB(rb, gb, bb, out var l2, out var a2, out var b2);
                var d = ColorHelper.Cie1994(l1, a1, b1, l2, a2, b2);
                sum += d;
                counter += 1.0;
                if (d > dmax) {
                    var avg = sum / counter;
                    Debug.WriteLine($"{counter}: dmax = {d:F2} avg = {avg:F2}");
                    dmax = d;
                }
            }
        }

        [TestMethod()]
        public void CalculateHistograms()
        {
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            var counter = 0;
            var keys = AppImgs.GetKeys();
            foreach (var key in keys) {
                if (!AppImgs.TryGetValue(key, out Img imgX)) {
                    continue;
                }

                if (imgX.IsHistogram()) {
                    continue;
                }

                var name = imgX.Name;
                var filename = FileHelper.NameToFileName(name);
                if (!File.Exists(filename)) {
                    continue;
                }

                var imagedata = File.ReadAllBytes(filename);
                if (imagedata == null) {
                    continue;
                }

                using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                    if (magickImage != null) {
                        using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, RotateFlipType.RotateNoneFlipNone)) {
                            var histogram = ColorHelper.CalculateHistogram(bitmap);
                            imgX.SetHistogram(histogram);

                            var size = Helper.SizeToString(imagedata.Length);
                            counter++;
                            Debug.WriteLine($"{counter}: {key} - {size} ({bitmap.Width}x{bitmap.Height}) L={histogram[0]:F0} A={histogram[1]:F0} B={histogram[2]:F0} ");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void GetDistance()
        {
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
                using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                    if (magickImage != null) {
                        using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, RotateFlipType.RotateNoneFlipNone)) {
                            var avg = ColorHelper.CalculateHistogram(bitmap);
                            vectors[i] = new Tuple<string, float[]>(name, avg);
                        }
                    }
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var distance = ColorHelper.Cie1976(vectors[0].Item2[0], vectors[0].Item2[1], vectors[0].Item2[2], vectors[i].Item2[0], vectors[i].Item2[1], vectors[i].Item2[2]) / 100.0;
                Debug.WriteLine($"{images[i]} = {distance:F3}");
            }
        }
    }
}
