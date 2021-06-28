using System;
using System.Drawing;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Rotate(string filename, RotateFlipType rft)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(filename, out var img)) {
                    if (!ImageHelper.GetImageDataFromFile(img.FileName, out _, out _, out var bitmap, out _)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {filename}");
                        return;
                    }

                    bitmap.RotateFlip(rft);
                    if (!ImageHelper.GetImageDataFromBitmap(bitmap, out var rimagedata)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Encode error: {filename}");
                        return;
                    }

                    var rhash = Helper.ComputeHash(rimagedata);
                    if (_hashList.ContainsKey(rhash)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Dup found for {filename}");
                        return;
                    }
                    else {
                        ImageHelper.ComputeKazeDescriptors(bitmap, out var rindexes, out var rmindexes);
                        ImageHelper.GetExif(img.FileName, out var rdatetaken, out var rmetadata);

                        var minlc = GetMinLastCheck();
                        var rimg = new Img(
                            filename: filename,
                            hash: rhash,
                            width: bitmap.Width,
                            height: bitmap.Height,
                            size: rimagedata.Length,
                            datetaken: rdatetaken,
                            metadata: rmetadata,
                            kazeone: rindexes,
                            kazetwo: rmindexes,
                            nexthash: rhash,
                            kazematch: 0,
                            lastchanged: img.LastChanged,
                            lastview: img.LastView,
                            lastcheck: minlc,
                            counter: 0);

                        Delete(img.FileName);
                        Add(rimg);
                        Helper.WriteData(rimg.FileName, rimagedata);
                    }

                    bitmap.Dispose();
                }
            }
        }
    }
}
