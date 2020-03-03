using System;
using System.Linq;
using System.Threading.Tasks;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void FindNext(int idX, out int lastid, out int nextid, out float distance)
        {
            float[] vectorX;
            int[] ids;
            float[][] vectors;
            float[] distances;
            lock (_imglock) {
                var imgX = _imgList[idX];
                vectorX = imgX.Vector();
                if (imgX.Name.StartsWith(AppConsts.PrefixLegacy, StringComparison.OrdinalIgnoreCase)) {
                    ids = _imgList
                            .Values
                            .Select(e => e.Id)
                            .ToArray();

                    vectors = _imgList
                            .Values
                            .Select(e => e.Vector())
                            .ToArray();
                }
                else {
                    var nodeX = Helper.GetNode(imgX.Name);
                    ids = _imgList
                            .Values
                            .Where(e => Helper.GetNode(e.Name).Equals(nodeX, StringComparison.OrdinalIgnoreCase))
                            .Select(e => e.Id)
                            .ToArray();

                    vectors = _imgList
                            .Values
                            .Where(e => Helper.GetNode(e.Name).Equals(nodeX, StringComparison.OrdinalIgnoreCase))
                            .Select(e => e.Vector())
                            .ToArray();
                }
            }

            distances = new float[ids.Length];
            nextid = idX;
            distance = 1f;
            lastid = _id;
            Parallel.For(0, ids.Length, i =>
            {
                distances[i] = Helper.VectorDistance(vectorX, vectors[i]);
            });

            for (var i = 0; i < ids.Length; i++) {
                if (ids[i] == idX) {
                    continue;
                }

                if (distances[i] < distance) {
                    distance = distances[i];
                    nextid = ids[i];
                }
            }
        }
    }
}
