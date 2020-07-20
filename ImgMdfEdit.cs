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
                    if (!Helper.GetImageDataFromFile(
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
                    if (!Helper.GetImageDataFromBitmap(bitmap, out byte[] rimagedata)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Encode error: {img.Folder:D2}\\{name}");
                        return;
                    }

                    var rhash = Helper.ComputeHash(rimagedata);
                    if (_hashList.ContainsKey(rhash)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Rotated image found: {img.Folder:D2}\\{name}");
                        return;
                    }

                    var rscd = ScdHelper.Compute(bitmap);

                    if (!OrbHelper.Compute(bitmap, out ulong rphash, out ulong[] rdescriptors)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Not enough descriptors {img.Folder:D2}\\{name}");
                        return;
                    }

                    var rimg = new Img(
                        name: img.Name,
                        hash: rhash,
                        phash: rphash,
                        width: bitmap.Width,
                        heigth: bitmap.Height,
                        size: rimagedata.Length,
                        scd: rscd,
                        descriptors: rdescriptors,
                        folder: img.Folder,
                        path: img.Path,
                        counter: img.Counter,
                        lastadded: img.LastAdded,
                        lastview: img.LastView
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
