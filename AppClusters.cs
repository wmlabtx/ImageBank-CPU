using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public static class AppClusters
    {
        const int MAXCLUSTERS = 1024;
        const float SIMD = 0.1f;
        const int MAXAGE = 25000;

        private static readonly SortedList<int, Cluster> _clusters = new SortedList<int, Cluster>();

        public static void Clear()
        {
            _clusters.Clear();
        }

        public static void Add(Cluster cluster)
        {
            _clusters.Add(cluster.Id, cluster);
        }

        public static void Compute(Img img, float[] imgvector, out int clusterid, out int bestid, out float distance)
        {
            clusterid = 0;
            bestid = img.BestId;
            distance = img.Distance;

            if (_clusters.Count == 0) {
                var cluster = new Cluster(id:1, counter:1, age:0, vector:imgvector);
                _clusters.Add(1, cluster);
                AppDatabase.AddCluster(cluster);
                clusterid = 1;
                bestid = img.Id;
                distance = 2f;
                return;
            }

            var clusterida = 0;
            var minda = 2f;
            var clusteridb = 0;
            var mindb = 2f;
            foreach (var e in _clusters.Keys) {
                var d = VggHelper.GetDistance(imgvector, _clusters[e].GetVector());
                if (d < minda) {
                    mindb = minda;
                    clusteridb = clusterida;
                    minda = d;
                    clusterida = e;
                }
                else {
                    if (d < mindb) {
                        mindb = d;
                        clusteridb = e;
                    }
                }
            }

            if (minda > SIMD && _clusters.Count < MAXCLUSTERS) {
                var newid = _clusters.Values.Max(e => e.Id) + 1;
                var cluster = new Cluster(id:newid, counter:1, age:0, vector:imgvector);
                _clusters.Add(cluster.Id, cluster);
                AppDatabase.AddCluster(cluster);
                clusterid = newid;

                clusterida = 0;
                minda = 2f;
                clusteridb = 0;
                mindb = 2f;
                foreach (var e in _clusters.Keys) {
                    var d = VggHelper.GetDistance(imgvector, _clusters[e].GetVector());
                    if (d < minda) {
                        mindb = minda;
                        clusteridb = clusterida;
                        minda = d;
                        clusterida = e;
                    }
                    else {
                        if (d < mindb) {
                            mindb = d;
                            clusteridb = e;
                        }
                    }
                }

                List<Tuple<int, float[]>> minscope = new List<Tuple<int, float[]>>();
                minscope.AddRange(AppImgs.GetVectors(clusteridb));
                bestid = img.Id;
                distance = 2f;
                foreach (var e in minscope) {
                    if (img.Id == e.Item1) {
                        continue;
                    }

                    var d = VggHelper.GetDistance(imgvector, e.Item2);
                    if (d < distance) {
                        distance = d;
                        bestid = e.Item1;
                    }
                }

                return;
            }

            clusterid = clusterida;
            List<Tuple<int, float[]>> scope = new List<Tuple<int, float[]>>();
            scope.AddRange(AppImgs.GetVectors(clusterida));
            if (clusteridb != 0) {
                scope.AddRange(AppImgs.GetVectors(clusteridb));
            }

            bestid = img.Id;
            distance = 2f;
            foreach (var e in scope) {
                if (img.Id == e.Item1) {
                    continue;
                }

                var d = VggHelper.GetDistance(imgvector, e.Item2);
                if (d < distance) {
                    distance = d;
                    bestid = e.Item1;
                }
            }
        }

        public static void Update(int clusterid)
        {
            if (!_clusters.TryGetValue(clusterid, out Cluster cluster)) {
                return;
            }

            var scope = AppImgs.GetVectors(clusterid);
            cluster.SetCounter(scope.Count);
            if (scope.Count > 0) {
                var vector = new float[4096];
                foreach (var e in scope) {
                    for (var i = 0; i < 4096; i++) {
                        vector[i] += e.Item2[i];
                    }
                }

                for (var i = 0; i < 4096; i++) {
                    vector[i] /= scope.Count;
                }

                cluster.SetVector(vector);
            }
            else {
                if (_clusters.ContainsKey(clusterid)) {
                    _clusters.Remove(clusterid);
                }

                AppDatabase.DeleteCluster(clusterid);
            }

            cluster.SetAge(0);
        }

        public static void DeleteAged()
        {
            foreach (var e in _clusters) {
                e.Value.SetAge(e.Value.Age + 1);
            }

            var found = _clusters.Where(e => e.Value.Age >= MAXAGE).ToArray();
            foreach (var e in found) {
                var id = e.Key;
                if (_clusters.ContainsKey(id)) {
                    _clusters.Remove(id);
                }

                AppDatabase.DeleteCluster(id);
            }
        }

        public static int GetSize(int clusterid)
        {
            int result;
            if (!_clusters.TryGetValue(clusterid, out Cluster cluster)) {
                result = 0;
            }
            else {
                result = cluster.Counter;
            }

            return result;
        }
    }
}
