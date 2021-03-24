using System;
using System.Drawing;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Rotate(string name, RotateFlipType rft)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(name, out var img))
                {
                    if (!ImageHelper.GetImageDataFromFile(
                        img.FileName,
                        out _,
                        out var bitmap,
                        out _)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {img.Folder}\\{name}");
                        return;
                    }

                    bitmap.RotateFlip(rft);
                    if (!ImageHelper.GetImageDataFromBitmap(bitmap, out var rimagedata)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Encode error: {img.Folder}\\{name}");
                        return;
                    }

                    var rhash = Helper.ComputeHash(rimagedata);
                    if (_hashList.ContainsKey(rhash)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Dup found for {img.Folder}\\{name}");
                        Delete(img.Name);
                    }
                    else
                    {
                        ImageHelper.ComputeBlob(bitmap, out var rdescriptors);
                        if (rdescriptors == null || rdescriptors.Length == 0)
                        {
                            ((IProgress<string>)AppVars.Progress).Report($"Not enough descriptors {img.Folder}\\{name}");
                            return;
                        }

                        var rblob = ImageHelper.ArrayFrom64(rdescriptors);
                        ImageHelper.ComputePBlob(bitmap, out var rhashes);
                        var rpblob = ImageHelper.ArrayFrom64(rhashes);

                        var minlc = GetMinLastCheck();
                        var id = AllocateId();
                        var rimg = new Img(
                            name: img.Name,
                            folder: img.Folder,
                            hash: rhash,
                            blob: rblob,
                            pblob: rpblob,
                            lastchanged: img.LastChanged,
                            lastview: img.LastView,
                            counter: img.Counter,
                            lastcheck: minlc,
                            nexthash: rhash,
                            diff: new byte[1] { 0xFF },
                            width: bitmap.Width,
                            height: bitmap.Height,
                            size: rimagedata.Length,
                            id: id,
                            lastid: 0,
                            distance: 256);

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
