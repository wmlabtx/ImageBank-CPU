using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Rotate(int id, RotateFlipType rft, IProgress<string> progress)
        {
            if (AppImgs.TryGetValue(id, out var img)) {
                var filename = FileHelper.NameToFileName(img.Name);
                var imagedata = FileHelper.ReadData(filename);
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

                    var rhash = Md5HashHelper.Compute(rimagedata);
                    if (AppImgs.ContainsHash(rhash)) {
                        progress.Report($"Dup found for {filename}");
                        return;
                    }

                    var rhist = AppPalette.ComputeHist(bitmap);
                    var rimg = new Img(
                        id: img.Id,
                        name: img.Name,
                        hash: rhash,
                        year: img.Year,
                        lastview: DateTime.Now,
                        familyid: img.FamilyId,
                        hist: rhist);

                    Delete(id);
                    FileHelper.WriteData(filename, rimagedata);
                    Add(rimg);

                    bitmap.Dispose();
                }
            }
        }
    }
}
