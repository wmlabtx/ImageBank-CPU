using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace ImageBankTest
{
    [TestClass]
    public class ColorHelperTests
    {
        [TestMethod()]
        public void CreateRandomPalette()
        {
            ColorHelper.CreateRandomPalette();
        }

        [TestMethod()]
        public void Learn()
        {
            Debug.WriteLine("Loading database");
            AppDatabase.LoadImages(null);
            ColorHelper.Learn();
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
                using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                    if (magickImage != null) {
                        using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, RotateFlipType.RotateNoneFlipNone)) {
                            var histogram = ColorHelper.CalculateHistogram(bitmap);
                            vectors[i] = new Tuple<string, float[]>(name, histogram);
                        }
                    }
                }
            }

            for (var i = 0; i < vectors.Length; i++) {
                var description = ColorHelper.GetDescription(vectors[i].Item2);
                var distance = ColorHelper.GetDistance(vectors[0].Item2, vectors[i].Item2);
                Debug.WriteLine($"{images[i]} = {distance:F3} ({description})");
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

                if (ColorHelper.IsHistogram(imgX.GetHistogram())) {
                    continue;
                }

                var filename = FileHelper.NameToFileName(hash:imgX.Hash, name:imgX.Name);
                var imagedata = File.ReadAllBytes(filename);
                using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                    if (magickImage != null) {
                        using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, RotateFlipType.RotateNoneFlipNone)) {
                            var histogram = ColorHelper.CalculateHistogram(bitmap);
                            imgX.SetHistogram(histogram);
                            var description = ColorHelper.GetDescription(histogram);
                            var size = Helper.SizeToString(imagedata.Length);
                            Debug.WriteLine($"{counter}: {key} - {size} ({bitmap.Width}x{bitmap.Height}) {description}");
                            counter++;
                        }
                    }
                }
            }
        }
    }
}
