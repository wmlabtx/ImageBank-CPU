using OpenCvSharp;
using OpenCvSharp.Features2D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static class SiftHelper
    {
        private static readonly SIFT _sift = SIFT.Create(10000);
        private static readonly SortedList<int, SiftNode> _nodes = new SortedList<int, SiftNode>();
        private static object _nodesLock = new object();

        public static int Count => _nodes.Count(e => e.Value.ChildId == 0);

        public static void LoadNodes()
        {
            if (!File.Exists(AppConsts.FileSiftNodes)) {
                return;
            }

            lock (_nodesLock) {
                _nodes.Clear();
                using (var fs = new FileStream(AppConsts.FileSiftNodes, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs)) {
                    while (br.BaseStream.Position != br.BaseStream.Length) {
                        int id = br.ReadInt32();
                        byte[] core = br.ReadBytes(128);
                        float sum = br.ReadSingle();
                        float max = br.ReadSingle();
                        int cnt = br.ReadInt32();
                        float avg = br.ReadSingle();
                        int childid = br.ReadInt32();
                        var siftnode = new SiftNode() {
                            Id = id,
                            Core = core,
                            Sum = sum,
                            Max = max,
                            Cnt = cnt,
                            Avg = avg,
                            ChildId = childid
                        };

                        _nodes.Add(siftnode.Id, siftnode);
                    }
                }
            }
        }

        public static void SaveNodes()
        {
            lock (_nodesLock) {
                var temp = Path.ChangeExtension(AppConsts.FileSiftNodes, ".temp");
                using (var fs = new FileStream(temp, FileMode.Create, FileAccess.Write))
                using (var bw = new BinaryWriter(fs)) {
                    foreach (var siftnode in _nodes.Values) {
                        bw.Write(siftnode.Id);
                        bw.Write(siftnode.Core);
                        bw.Write(siftnode.Sum);
                        bw.Write(siftnode.Max);
                        bw.Write(siftnode.Cnt);
                        bw.Write(siftnode.Avg);
                        bw.Write(siftnode.ChildId);
                    }
                }

                var bak = Path.ChangeExtension(AppConsts.FileSiftNodes, ".bak");
                if (File.Exists(bak)) {
                    File.Delete(bak);
                }

                if (File.Exists(AppConsts.FileSiftNodes)) {
                    File.Move(AppConsts.FileSiftNodes, bak);
                }

                File.Move(temp, AppConsts.FileSiftNodes);
            }
        }

        public static byte[] GetDescriptors(float[][] matrix)
        {
            byte[] descriptors;

            int numRows = matrix.Length;
            int numCols = matrix[0].Length;

            float fmin = float.MaxValue;
            float fmax = float.MinValue;
            for (int i = 0; i < numRows; i++) {
                for (int j = 0; j < numCols; j++) {
                    if (matrix[i][j] < fmin) {
                        fmin = matrix[i][j];
                    }

                    if (matrix[i][j] > fmax) {
                        fmax = matrix[i][j];
                    }
                }
            }

            Mat mat = new Mat();
            using (var matraw = new Mat(numRows, numCols, MatType.CV_8U)) {
                for (int i = 0; i < numRows; i++) {
                    for (int j = 0; j < numCols; j++) {
                        byte val = (byte)(255f * (matrix[i][j] - fmin) / (fmax - fmin));
                        matraw.At<byte>(i, j) = val;
                    }
                }
                
                var f = 512f / Math.Min(matraw.Width, matraw.Height);
                if (f < 1f) {
                    Cv2.Resize(matraw, mat, new Size(0, 0), f, f, InterpolationFlags.Cubic);
                }
                else {
                    mat = matraw.Clone();
                }
            }

            var keypoints = _sift.Detect(mat);
            keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(1000).ToArray();
            using (var matdescriptors = new Mat()) {
                _sift.Compute(mat, ref keypoints, matdescriptors);
                /*
                using (var matkeypoints = new Mat()) {
                    Cv2.DrawKeypoints(mat, keypoints, matkeypoints, null, DrawMatchesFlags.DrawRichKeypoints);
                    matkeypoints.SaveImage("matkeypoints.png");
                }
                */

                using (var matflip = new Mat()) {
                    Cv2.Flip(mat, matflip, FlipMode.Y);
                    keypoints = _sift.Detect(matflip);
                    keypoints = keypoints.OrderByDescending(e => e.Size).ThenBy(e => e.Response).Take(1000).ToArray();
                    using (var matdescriptorsflip = new Mat()) {
                        _sift.Compute(matflip, ref keypoints, matdescriptorsflip);
                        matdescriptors.PushBack(matdescriptorsflip);
                    }
                }

                matdescriptors.GetArray(out float[] fdata);
                descriptors = new byte[fdata.Length];
                for (var i = 0; i < fdata.Length; i++) {
                    descriptors[i] = (byte)Math.Floor(fdata[i]);
                }
            }

            mat.Dispose();
            return descriptors;
        }

        private static float GetDistance(byte[] x, int xo, byte[] y, int yo)
        {
            float distance = 0f;
            for (var i = 0; i < 128; i++) {
                float dx = (float)x[xo + i] - y[yo + i];
                distance += dx * dx;
            }

            distance = (float)Math.Sqrt(distance);
            return distance;
        }

        private static SiftNode GetNewNode(int id)
        {
            var siftnode = new SiftNode() {
                Id = id,
                Core = new byte[128],
                Sum = 0f,
                Max = 0f,
                Cnt = 0,
                Avg = 0f,
                ChildId = 0
            };

            return siftnode;
        }

        public static void Populate(byte[] array, int offset)
        {
            lock (_nodesLock) {
                if(_nodes.Count == 0) {
                    var siftnode = GetNewNode(1);
                    _nodes.Add(siftnode.Id, siftnode);
                }

                var id = 1;
                while (true) {
                    var node = _nodes[id];
                    if (node.Cnt == 0) {
                        Buffer.BlockCopy(array, offset, node.Core, 0, 128);
                        node.Cnt = 1;
                        return;
                    }

                    var distance = GetDistance(array, offset, node.Core, 0);
                    if (node.ChildId == 0) {
                        node.Sum += distance;
                        node.Cnt++;
                        node.Avg = node.Sum / node.Cnt;
                        node.Max = Math.Max(node.Max, distance);
                        if (node.Cnt <= 1000 || node.Max < 10f) {
                            return;
                        }

                        var nextid = _nodes.Max(e => e.Key) + 1;
                        var siftnode0 = GetNewNode(nextid);
                        _nodes.Add(siftnode0.Id, siftnode0);
                        var siftnode1 = GetNewNode(nextid + 1);
                        _nodes.Add(siftnode1.Id, siftnode1);
                        node.ChildId = nextid;
                        return;
                    }

                    id = distance < node.Avg ? node.ChildId : node.ChildId + 1;
                }
            }
        }

        private static int Compute(byte[] array, int offset)
        {
            lock (_nodesLock) {
                var id = 1;
                while (true) {
                    var node = _nodes[id];
                    if (node.Cnt == 0) {
                        Buffer.BlockCopy(array, offset, node.Core, 0, 128);
                        node.Cnt = 1;
                        return id;
                    }

                    if (node.ChildId == 0) {
                        return id;
                    }
                    
                    var distance = GetDistance(array, offset, node.Core, 0);
                    id = distance < node.Avg ? node.ChildId : node.ChildId + 1;
                }
            }
        }

        public static void Populate(byte[] descriptors)
        {
            for (var offset = 0; offset < descriptors.Length; offset += 128) {
                Populate(descriptors, offset);
            }
        }

        public static int[] ComputeVector(byte[] descriptors)
        {
            var vector = new int[descriptors.Length / 128];
            for (var offset = 0; offset < descriptors.Length; offset += 128) {
                vector[offset / 128] = Compute(descriptors, offset);
            }

            Array.Sort(vector);
            return vector;
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
