using System;
using System.Drawing;
using System.IO;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Rotate(string hash, RotateFlipType rft, IProgress<string> progress)
        {
            if (!AppImgs.TryGetValue(hash, out var img)) {
                progress.Report($"Image {img.Name} not found");
                return;
            }

            var filename = FileHelper.NameToFileName(img.Name);
            var imagedata = FileHelper.ReadEncryptedFile(filename);
            if (imagedata == null) {
                progress.Report($"Cannot read {img.Name}");
                return;
            }

            using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                if (magickImage == null) {
                    progress.Report($"Corrupted image {img.Name}");
                    return;
                }

                using (var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, rft)) {
                    if (bitmap == null) {
                        progress.Report($"Corrupted image {img.Name}");
                        return;
                    }

                    img.SetOrientation(rft);
                    var rvector = VggHelper.CalculateVector(bitmap);
                    img.SetVector(rvector);
                    var lastcheck = AppImgs.GetMinLastCheck();
                    img.SetLastCheck(lastcheck);
                }
            }
        }
    }
}
