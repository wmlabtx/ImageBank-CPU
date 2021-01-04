using System;
using System.Drawing;
using System.Linq;

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
                    if (!ImageHelper.GetImageDataFromBitmap(bitmap, out byte[] rimagedata)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Encode error: {img.Folder:D2}\\{name}");
                        return;
                    }

                    if (!ImageHelper.ComputeDescriptors(bitmap, out var rdescriptors)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Not enough histogram {img.Folder:D2}\\{name}");
                        return;
                    }

                    var rhash = Helper.ComputeHash(rimagedata);
                    if (_hashList.ContainsKey(rhash)) {
                        Delete(img.Name);
                    }
                    else {
                        var minlc = _imgList.Min(e => e.Value.LastCheck).AddSeconds(-1);
                        var rimg = new Img(
                            name: img.Name,
                            hash: rhash,
                            width: bitmap.Width,
                            heigth: bitmap.Height,
                            size: rimagedata.Length,
                            descriptors: rdescriptors,
                            folder: img.Folder,
                            lastview: img.LastView,
                            lastcheck: minlc,
                            lastadded: img.LastAdded,
                            nextname: img.NextName,
                            sim: 0f,
                            family: img.Family,
                            counter: img.Counter
                        );

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
