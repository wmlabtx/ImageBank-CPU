using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private bool FindNext(int idX, out int nextid, out float distance)
        {            
            var candidates = new List<Tuple<int, Scd>>();
            Scd vectorX;
            lock (_imglock) {
                if (!_imgList.TryGetValue(idX, out Img imgX)) {
                    nextid = 0;
                    distance = 0f;
                    nextid = 0;
                    return false;
                }

                distance = imgX.Distance;
                nextid = imgX.NextId;
                vectorX = imgX.Vector;
                if (vectorX.IsEmpty()) {
                    if (!Helper.GetImageDataFromFile(
                        imgX.FileName,
                        out var imgdata,
                        out var magicformat,
#pragma warning disable CA2000 // Dispose objects before losing scope
                        out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                        out var checksum,
                        out var message,
                        out var bitmapchanged)) {
                        return false;
                    }

                    if (bitmapchanged) {
                        Helper.WriteData(imgX.FileName, imgdata);
                        imgX.Format = magicformat;
                        imgX.Checksum = checksum;
                    }

                    imgX.Vector = ScdHelper.Compute(bitmap);
                    vectorX = imgX.Vector;
                    nextid = 0;
                }

                if (nextid <= 0 || !_imgList.TryGetValue(nextid, out var imgY)) {
                    distance = 0f;
                    nextid = 0;
                }

                foreach (var e in _imgList) {
                    if (e.Value.Id != imgX.Id) {
                        if (!e.Value.Vector.IsEmpty()) {
                            candidates.Add(new Tuple<int, Scd>(e.Value.Id, e.Value.Vector));
                        }
                    }
                }
            }

            var index = 0;
            while (index < candidates.Count) {
                var edistance = vectorX.GetDistance(candidates[index].Item2);
                if (nextid == 0 || edistance < distance) {
                    distance = edistance;
                    nextid = candidates[index].Item1;
                }

                index++;
            }
            
            return true;
        }
    }
}
