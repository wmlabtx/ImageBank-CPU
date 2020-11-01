using System;
using System.Drawing;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Rotate(string name, RotateFlipType rft)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(name, out var img)) {
                    if (!ImageHelper.GetImageDataFromFile(
                        img.FileName,
                        out _,
#pragma warning disable CA2000 // Dispose objects before losing scope
                        out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                        out _,
                        out _)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {img.Folder:D2}\\{name}");
                        return;
                    }

                    bitmap.RotateFlip(rft);
                    if (!ImageHelper.GetImageDataFromBitmap(bitmap, out byte[] rimagedata)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Encode error: {img.Folder:D2}\\{name}");
                        return;
                    }

                    var rhash = Helper.ComputeHash(rimagedata);
                    if (_hashList.ContainsKey(rhash)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Rotated image found: {img.Folder:D2}\\{name}");
                        return;
                    }

                    if (!ImageHelper.ComputeDescriptors(bitmap, out var rdescriptors)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Not enough descriptors {img.Folder:D2}\\{name}");
                        return;
                    }

                    var lastcheck = DateTime.Now.AddYears(-10);
                    var rimg = new Img(
                        name: img.Name,
                        hash: rhash,
                        width: bitmap.Width,
                        heigth: bitmap.Height,
                        size: rimagedata.Length,
                        descriptors: rdescriptors,
                        folder: img.Folder,
                        lastview: img.LastView,
                        lastcheck: lastcheck,
                        lastadded: img.LastAdded,
                        nextname: "0123456789",
                        distance: 0f,
                        family: img.Family
                        );

                    bitmap.Dispose();

                    Delete(img.Name);
                    Add(rimg);

                    Helper.WriteData(rimg.FileName, rimagedata);
                }
            }
        }
    }
}
