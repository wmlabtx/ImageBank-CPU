﻿using OpenCvSharp;
using System;
using System.Drawing;

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
                            ImageHelper.GetVectors(bitmap, out ulong[][] rdescriptors, out Mat[] rmats);
                            for (var i = 0; i < 2; i++) {
                                rmats[i].Dispose();
                                //SqlPopulateDescriptors(rdescriptors[i]);
                            }

                            SqlGetFeatures(rdescriptors, out int[][] rfeatures);
                            MetadataHelper.GetMetadata(imagedata, out var rdatetaken, out var rmetadata);
                            var minlc = GetMinLastCheck();
                            var rimg = new Img(
                                name: name,
                                hash: rhash,
                                width: bitmap.Width,
                                height: bitmap.Height,
                                size: rimagedata.Length,
                                datetaken: rdatetaken,
                                metadata: rmetadata,
                                ki: rfeatures[0],
                                kimirror: rfeatures[1],
                                nexthash: rhash,
                                sim: 0f,
                                lastchanged: img.LastChanged,
                                lastview: img.LastView,
                                lastcheck: minlc,
                                generation: img.Generation);

                            Delete(name);
                            Add(rimg);
                            Helper.WriteData(filename, rimagedata);
                        }

                        bitmap.Dispose();
                    }
                }
            }
        }
    }
}
