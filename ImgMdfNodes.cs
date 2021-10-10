using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static SiftNode GetNewSiftNode(int id)
        {
            var siftnode = new SiftNode(
                id: id,
                core: new byte[128],
                sumdst: 0f,
                maxdst: 0f,
                cnt: 0,
                avgdst: 0f,
                childid: 0);

            return siftnode;
        }

        public static SiftNode FindNode(byte[] array, int offset)
        {
            lock (_nodesLock) {
                if (_nodesList.Count == 0) {
                    var node = GetNewSiftNode(1);
                    Add(node);
                    return node;
                }

                var id = 1;
                while (true) {
                    var node = _nodesList[id];
                    if (node.Cnt == 0 || node.ChildId == 0) {
                        return node;
                    }

                    var distance = SiftHelper.GetDistance(array, offset, node.Core, 0);
                    id = distance < node.AvgDst ? node.ChildId : node.ChildId + 1;
                }
            }
        }

        private static int Compute(byte[] array, int offset)
        {
            lock (_nodesLock) {
                var node = FindNode(array, offset);
                if (node.Cnt == 0) {
                    var core = new byte[128];
                    Buffer.BlockCopy(array, offset, core, 0, 128);
                    node.Core = core;
                    node.Cnt = 1;
                }
                else {
                    var distance = SiftHelper.GetDistance(array, offset, node.Core, 0);
                    node.SumDst += distance;
                    node.Cnt++;
                    node.AvgDst = node.SumDst / node.Cnt;
                    node.MaxDst = Math.Max(node.MaxDst, distance);
                }

                return node.Id;
            }
        }

        public static float GetNodeCount()
        {
            lock (_nodesLock) {
                var count = _nodesList.Count(e => e.Value.ChildId == 0);
                return count;
            }
        }

        public static float GetNodeMaxDistance()
        {
            lock (_nodesLock) {
                var maxdst = _nodesList.Where(e => e.Value.ChildId == 0).Max(e => e.Value.MaxDst);
                return maxdst;
            }
        }

        public static int[] ComputeVector(byte[] descriptors)
        {
            var vector = new int[descriptors.Length / 128];
            for (var offset = 0; offset < descriptors.Length; offset += 128) {
                vector[offset / 128] = Compute(descriptors, offset);
            }

            Array.Sort(vector);
            lock (_nodesLock) {
                SiftNode pnode = null;
                foreach (var node in _nodesList.Where(e => e.Value.ChildId == 0 && e.Value.Cnt > 1).Select(e => e.Value)) {
                    if (pnode == null || node.MaxDst > pnode.MaxDst) {
                        pnode = node;
                    }
                }

                if (pnode != null) {
                    var nextid = _nodesList.Max(e => e.Key) + 1;
                    var siftnode0 = GetNewSiftNode(nextid);
                    Add(siftnode0);
                    var siftnode1 = GetNewSiftNode(nextid + 1);
                    Add(siftnode1);
                    pnode.ChildId = nextid;
                }
            }

            return vector;
        }

        public static bool CheckVector(int[] vector)
        {
            lock (_nodesLock) {
                foreach (int v in vector) {
                    if (!_nodesList.ContainsKey(v) || _nodesList[v].ChildId != 0) {
                        return false;
                    }
                }
            }

            return true;
        }

        public static float GetDistance(int[] x, int[] y)
        {
            if (x == null || x.Length == 0 || y == null || y.Length == 0 || x.Length != y.Length) {
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
