using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void FindNearestCluster(byte[] descriptors, int offset, out int nearestnode, out float mindistance)
        {
            mindistance = float.MaxValue;
            nearestnode = 0;
            for (var i = 0; i < _clusters.Count; i++) {
                var distance = SiftHelper.GetDistance(descriptors, offset, _clusters[i].Descriptor, 0);
                if (distance < mindistance) {
                    nearestnode = i;
                    mindistance = distance;
                }
            }
        }

        private static void AddCluster(byte[] descriptors, int offset, int id)
        {
            var buffer = new byte[128];
            Buffer.BlockCopy(descriptors, offset, buffer, 0, 128);
            var cluster = new Cluster(id: id, descriptor: buffer);
            _clusters.Add(cluster);
            SqlAddCluster(cluster);
        }

        public static int[] ComputeVector(byte[] descriptors, BackgroundWorker backgroundworker)
        {
            var list = new List<int>();
            var offset = 0;
            while (offset < descriptors.Length) {
                int nearestnode = 0;
                float mindistance;
                if (_clusters.Count == 0) {
                    AddCluster(descriptors, offset, 0);
                    list.Add(nearestnode);
                }
                else {
                    FindNearestCluster(descriptors, offset, out nearestnode, out mindistance);
                    if (mindistance > AppConsts.MaxDistance) {
                        var id = _clusters.Max(e => e.Id) + 1;
                        AddCluster(descriptors, offset, id);
                        nearestnode = id;
                    }

                    list.Add(nearestnode);
                }
               
                offset += 128;
            }

            var vector = list.OrderBy(e => e).ToArray();
            return vector;
        }

        public static float GetDistance(int[] x, int[] y)
        {
            if (x == null || x.Length == 0 || y == null || y.Length == 0) {
                return 100f;
            }

            var m = 0;
            var i = 0;
            var j = 0;
            while (i < x.Length && j < y.Length) {
                if (x[i] == y[j]) {
                    m++;
                    i++;
                    j++;
                }
                else {
                    if (x[i] < y[j]) {
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            var distance = 100f * (x.Length - m) / x.Length;
            return distance;
        }
    }
}
