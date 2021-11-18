using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void FindNearestCluster(byte[] descriptors, int offset, out int nearestnode, out float mindistance)
        {
            lock (_clustersLock) {
                mindistance = float.MaxValue;
                nearestnode = 0;
                for (var i = 0; i < AppConsts.MaxClusters; i++) {
                    var distance = SiftHelper.GetDistance(descriptors, offset, _clusters[i].Descriptor, 0);
                    if (distance < mindistance) {
                        nearestnode = i;
                        mindistance = distance;
                    }
                }
            }
        }

        private static void UpdateVictimCluster()
        {
            _clustervictimid = -1;
            _clustervictimdistance = float.MaxValue;
            lock (_clustersLock) {
                for (var i = 0; i < AppConsts.MaxClusters; i++) {
                    var distance = _clusters[i].Distance;
                    if (distance < _clustervictimdistance) {
                        _clustervictimid = i;
                        _clustervictimdistance = distance;
                    }
                }
            }
        }

        private static void UpdateCluster(int id)
        {
            var mindistance = float.MaxValue;
            var minnextid = 0;
            for (var j = 0; j < AppConsts.MaxClusters; j++) {
                if (j == id) {
                    continue;
                }

                var distance = SiftHelper.GetDistance(_clusters[id].Descriptor, 0, _clusters[j].Descriptor, 0);
                if (distance < mindistance) {
                    mindistance = distance;
                    minnextid = j;
                }

                if (distance < _clusters[j].Distance) {
                    _clusters[j].NextId = id;
                    _clusters[j].Distance = distance;
                }
            }

            _clusters[id].NextId = minnextid;
            _clusters[id].Distance = mindistance;
        }

        public static int[] ComputeVector(byte[] descriptors)
        {
            var vector = new int[descriptors.Length / 128];
            var offset = 0;
            while (offset < descriptors.Length) {
                FindNearestCluster(descriptors, offset, out int nearestnode, out float mindistance);
                if (mindistance > _clustervictimdistance) {
                    lock (_clustersLock) {
                        var buffer = new byte[128];
                        Buffer.BlockCopy(descriptors, offset, buffer, 0, 128);
                        _clusters[_clustervictimid].Descriptor = buffer;
                        UpdateCluster(_clustervictimid);
                        for (var i = 0; i < AppConsts.MaxClusters; i++) {
                            if (_clusters[i].NextId == _clustervictimid) {
                                UpdateCluster(i);
                            }
                        }
                    }

                    /*
                    Img[] imgs;
                    lock (_imglock) {
                        imgs = _imgList.Select(e => e.Value).ToArray();
                    }

                    foreach (var img in imgs) {
                        if (img.Vector.Length > 0 && Array.BinarySearch(img.Vector, _clustervictimid) >= 0) {
                            img.Vector = Array.Empty<int>();
                            img.LastCheck = GetMinLastCheck();
                        }
                    }
                    */

                    vector[offset / 128] = _clustervictimid;
                    UpdateVictimCluster();
                }
                else {
                    vector[offset / 128] = nearestnode;
                }

                offset += 128;
            }

            Array.Sort(vector);
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
