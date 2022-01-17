using OpenCvSharp;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void AddCluster(Mat matdescriptor)
        {
            _clusters.PushBack(matdescriptor);
            _clusters.GetArray<float>(out var floatbuffer);
            var buffer = Helper.ArrayFromFloat(floatbuffer);
            SqlVarsUpdateProperty(AppConsts.AttrClusters, buffer);
        }

        public static ushort[] ComputeVector(Mat descriptors, BackgroundWorker backgroundworker)
        {
            var list = new List<ushort>();
            for (var i = 0; i < descriptors.Rows; i++) {
                if (_clusters.Rows == 0) {
                    AddCluster(descriptors.Row(0));
                    list.Add(0);
                }
                else {
                    var descriptor = descriptors.Row(i);
                    var d = _bfmatcher.Match(descriptor, _clusters);
                    if (d[0].Distance > AppConsts.MaxDistance) {
                        if (_clusters.Rows < AppConsts.MaxClusters) {
                            list.Add((ushort)_clusters.Rows);
                            AddCluster(descriptor);
                        }
                    }
                    else {
                        list.Add((ushort)d[0].TrainIdx);
                    }
                }
            }

            var vector = list.OrderBy(e => e).ToArray();
            return vector;
        }

        public static float GetDistance(ushort[] x, ushort[] y)
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
