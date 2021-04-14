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
                        ImageHelper.ComputeColorDescriptors(bitmap, out var rcolordescriptors);
                        ImageHelper.ComputePerceptiveDescriptors(bitmap, out var rperceptivedescriptors);
                        ImageHelper.ComputeOrbDescriptors_v2(bitmap, out var rorbdescriptors, out var rorbkeypoints);
                        var minlc = GetMinLastCheck();
                        var id = AllocateId();
                        var rimg = new Img(
                            id: id,
                            name: img.Name,
                            folder: img.Folder,
                            hash: rhash,

                            width: bitmap.Width,
                            height: bitmap.Height,
                            size: rimagedata.Length,

                            colordescriptors: rcolordescriptors,
                            colordistance: 100f,
                            perceptivedescriptors: rperceptivedescriptors,
                            perceptivedistance: AppConsts.MaxPerceptiveDistance,
                            orbdescriptors: rorbdescriptors,
                            orbkeypoints: rorbkeypoints,
                            orbdistance: AppConsts.MaxOrbDistance,

                            lastchanged: img.LastChanged,
                            lastview: img.LastView,
                            lastcheck: minlc,

                            nexthash: rhash,
                            lastid: 0,
                            counter: img.Counter);

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
