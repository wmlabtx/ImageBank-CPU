using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Rotate(int id, RotateFlipType rft)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    var filename = Helper.GetFileName(img.Name);
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
                        var rid = AllocateId();
                        var rimg = new Img(
                            id: rid,
                            name: img.Name,
                            hash: rhash,
                            datetaken: rdatetaken,
                            lastid: 0,
                            bestid: 0,
                            bestdistance: 0f,
                            lastview: img.LastView);

                        Delete(id);
                        Add(rimg);
                        Helper.WriteData(filename, rimagedata);

                        bitmap.Dispose();
                    }
                }
            }
        }
    }
}
