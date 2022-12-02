using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Rotate(string hash, RotateFlipType rft, IProgress<string> progress)
        {
            if (AppImgs.TryGetValue(hash, out var img)) {
                var filename = $"{AppConsts.PathRoot}\\{img.Name}";
                var imagedata = File.ReadAllBytes(filename);
                if (imagedata != null) {
                    var bitmap = BitmapHelper.ImageDataToBitmap(imagedata);
                    if (bitmap == null) {
                        progress.Report($"Corrupted image: {filename}");
                        return;
                    }

                    bitmap.RotateFlip(rft);
                    if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                        var bitmap24BppRgb = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                        using (var g = Graphics.FromImage(bitmap24BppRgb)) {
                            g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        }

                        bitmap.Dispose();
                        bitmap = bitmap24BppRgb;
                    }

                    if (!BitmapHelper.BitmapToImageData(bitmap, out var rimagedata)) {
                        progress.Report($"Encode error: {filename}");
                        return;
                    }

                    var rhash = HashHelper.Compute(rimagedata);
                    if (AppImgs.ContainsId(rhash)) {
                        progress.Report($"Dup found for {filename}");
                        return;
                    }

                    var rvector = VggHelper.CalculateVector(bitmap);
                    var newname = Path.ChangeExtension(img.Name, AppConsts.PngExtension);
                    var rimg = new Img(
                        name: newname,
                        hash: rhash,
                        year: img.Year,
                        lastview: DateTime.Now,
                        vector: rvector);

                    Delete(hash);
                    FileHelper.WriteData(filename, rimagedata);
                    Add(rimg);

                    bitmap.Dispose();
                }
            }
        }
    }
}
