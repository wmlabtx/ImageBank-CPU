using OpenCvSharp;
using System.Collections.Generic;
using System.Diagnostics;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void FindNext(int idX, out int lastid, out int nextid, out float distance)
        {
            Mat vectorX;
            SortedDictionary<int, Img> clone = new SortedDictionary<int, Img>();
            lock (_imglock) {
                var imgX = _imgList[idX];
                vectorX = imgX.Vector();
                lastid = imgX.LastId;
                distance = imgX.Distance;
                nextid = imgX.NextId;
                if (!_imgList.ContainsKey(nextid)) {
                    lastid = 0;
                    distance = 256f;
                    nextid = idX;
                }

                foreach (var e in _imgList) {
                    if (e.Key > lastid) {
                        clone.Add(e.Key, e.Value);
                    }
                }

                clone.Remove(idX);
                var history = imgX.GetHistoryIds();
                foreach (var other in history) {
                    if (!clone.Remove(other)) {
                        imgX.RemoveFromHistory(other);
                    }
                }
            }

            var sw = Stopwatch.StartNew();
            foreach (var other in clone) {
                lastid = other.Value.Id;
                var otherdistance = OrbHelper.GetDistance(vectorX, other.Value.Vector());
                if (otherdistance < distance) {
                    distance = otherdistance;
                    nextid = other.Value.Id;
                }

                if (sw.ElapsedMilliseconds > 800) {
                    sw.Stop();
                    return;
                }
            }

            sw.Stop();
            lastid = _id;
        }
    }
}
