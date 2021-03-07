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
                        out var bitmap,
                        out _)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {img.Folder:D2}\\{name}");
                        return;
                    }

                    bitmap.RotateFlip(rft);
                    if (!ImageHelper.GetImageDataFromBitmap(bitmap, out var rimagedata)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Encode error: {img.Folder:D2}\\{name}");
                        return;
                    }

                    ImageHelper.ComputeBlob(bitmap, out var rphash, out var rdescriptors);
                    if (rdescriptors == null || rdescriptors.Length == 0) {
                        ((IProgress<string>)AppVars.Progress).Report($"Not enough descriptors {img.Folder:D2}\\{name}");
                        return;
                    }

                    var rblob = ImageHelper.ArrayFrom64(rdescriptors);
                    var rhash = Helper.ComputeHash(rimagedata);
                    if (_hashList.ContainsKey(rhash)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Dup found for {img.Folder:D2}\\{name}");
                        Delete(img.Name);
                    }
                    else {
                        var minlc = GetMinLastCheck();
                        var rimg = new Img(
                            name: img.Name,
                            folder: img.Folder,
                            hash: rhash,
                            blob: rblob,
                            phash: rphash,
                            lastadded: img.LastAdded,
                            lastview: img.LastView,
                            history: img.History,
                            lastcheck: minlc,
                            nexthash: rhash,
                            distance: AppConsts.MaxDistance,
                            width: bitmap.Width,
                            height: bitmap.Height,
                            size: rimagedata.Length);

                        Delete(img.Name);
                        Add(rimg);
                        Helper.WriteData(rimg.FileName, rimagedata);
                    }

                    bitmap.Dispose();
                }
            }
        }
    }
}
