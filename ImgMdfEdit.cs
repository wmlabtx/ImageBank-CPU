﻿using System;
using System.Drawing;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Rotate(int id, RotateFlipType rft)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img))
                {
                    if (!ImageHelper.GetImageDataFromFile(
                        img.FileName,
                        out _,
                        out var bitmap,
                        out _)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {img.Folder:D2}\\{img.Id:D6}");
                        return;
                    }

                    bitmap.RotateFlip(rft);
                    if (!ImageHelper.GetImageDataFromBitmap(bitmap, out var rimagedata)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Encode error: {img.Folder:D2}\\{img.Id:D6}");
                        return;
                    }

                    var rhash = Helper.ComputeHash(rimagedata);
                    if (_hashList.ContainsKey(rhash)) {
                        ((IProgress<string>)AppVars.Progress).Report($"Dup found for {img.Folder:D2}\\{img.Id:D6}");
                        Delete(img.Id);
                    }
                    else
                    {
                        ImageHelper.ComputeKazeDescriptors(bitmap, out var rindexes, out var rmindexes);
                        var minlc = GetMinLastCheck();
                        var rid = AllocateId();
                        var rimg = new Img(
                            id: rid,
                            hash: rhash,

                            width: bitmap.Width,
                            height: bitmap.Height,
                            size: rimagedata.Length,

                            akazecentroid: rindexes,
                            akazemirrorcentroid: rmindexes,
                            akazepairs: 0,

                            lastchanged: img.LastChanged,
                            lastview: img.LastView,
                            lastcheck: minlc,

                            nexthash: rhash,
                            counter: 0);

                        Delete(img.Id);
                        Add(rimg);

                        Helper.WriteData(rimg.FileName, rimagedata);
                    }

                    bitmap.Dispose();
                }
            }
        }
    }
}
