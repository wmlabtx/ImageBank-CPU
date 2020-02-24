using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void FindNext(int idX, out int lastid, out DateTime lastchange, out int nextid, out float distance)
        {
            lock (_imglock) {
                var imgX = _imgList[idX];
                lastchange = imgX.LastChange;
                nextid = imgX.NextId;
                distance = imgX.Distance;
                lastid = imgX.LastId;
                if (!_imgList.ContainsKey(nextid)) {
                    distance = 1f;
                    lastid = -1;
                }

                var candidates = _imgList
                    .Values
                    .Where(e => e.Id > imgX.LastId)
                    .OrderBy(e => e.Id)
                    .ToArray();

                foreach (var imgY in candidates) {
                    lastid = imgY.Id;
                    if (imgY.Id == idX) {
                        continue;
                    }

                    var distanceXY = Helper.VectorDistance(imgX.Vector(), imgY.Vector());
                    if (distanceXY < distance) {
                        distance = distanceXY;
                        nextid = imgY.Id;
                        lastchange = DateTime.Now;
                    }
                }
            }
        }
    }
}
