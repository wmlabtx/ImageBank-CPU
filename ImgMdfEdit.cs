using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Rotate(int id, RotateFlipType rft)
        {
            /*
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    var filename = FileHelper.NameToFileName(img.Name);
                    var imagedata = FileHelper.ReadData(filename);
                    if (imagedata != null) {
                        var bitmap = BitmapHelper.ImageDataToBitmap(imagedata);
                        if (bitmap == null) {
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

                        var rhash = MD5HashHelper.Compute(rimagedata);
                        if (_hashList.ContainsKey(rhash)) {
                            ((IProgress<string>)AppVars.Progress).Report($"Dup found for {filename}");
                            return;
                        }

                        var rdescriptors = ImageHelper.GetAkaze2Descriptors(bitmap);
                        if (bitmap != null) {
                            bitmap.Dispose();
                        }

                        MetadataHelper.GetMetadata(imagedata, out var rdatetaken);
                        var rid = AllocateId();
                        var rimg = new Img(
                            id: rid,
                            name: img.Name,
                            hash: rhash,
                            datetaken: rdatetaken,
                            descriptors: rdescriptors,
                            family: img.Family,
                            history: img.History,
                            bestid: img.BestId,
                            bestdistance: img.BestDistance,
                            lastview: img.LastView,
                            lastcheck: img.LastCheck);

                        Delete(id);
                        Add(rimg);
                        FileHelper.WriteData(filename, rimagedata);

                        bitmap.Dispose();
                    }
                }
            }
            */
        }
    }
}
