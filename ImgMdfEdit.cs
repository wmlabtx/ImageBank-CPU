using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Rotate(string name, RotateFlipType rft)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(name, out var img)) {
                    var filename = Helper.GetFileName(name);
                    var imagedata = Helper.ReadData(filename);
                    if (imagedata != null) {
                        if (!ImageHelper.GetBitmapFromImageData(imagedata, out var bitmap)) {
                            ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {filename}");
                            return;
                        }

                        bitmap.RotateFlip(rft);
                        if (bitmap.PixelFormat != PixelFormat.Format24bppRgb) {
                            bitmap = ImageHelper.RepixelBitmap(bitmap);
                        }

                        if (!ImageHelper.GetImageDataFromBitmap(bitmap, out var rimagedata)) {
                            ((IProgress<string>)AppVars.Progress).Report($"Encode error: {filename}");
                            return;
                        }

                        var rhash = Helper.ComputeHash(rimagedata);
                        if (_hashList.ContainsKey(rhash)) {
                            ((IProgress<string>)AppVars.Progress).Report($"Dup found for {filename}");
                            return;
                        }

                        MetadataHelper.GetMetadata(imagedata, out var rdatetaken);

                        var rimg = new Img(
                            name: name,
                            hash: rhash,
                            datetaken: rdatetaken,
                            bestnames: img.BestNames,
                            family: img.Family,
                            lastchanged: img.LastChanged,
                            lastview: img.LastView,
                            lastcheck: img.LastCheck,
                            generation: img.Generation);

                        Delete(name);
                        Add(rimg);
                        Helper.WriteData(filename, rimagedata);

                        bitmap.Dispose();
                    }
                }
            }
        }
    }
}
