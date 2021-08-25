using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void ClearNodes()
        {
            _nodes.Clear();
        }

        public static void AddDescriptor(float[] descriptor)
        {
            var nodeid = -1;
            var mindistance = float.MaxValue;
            foreach (var e in _nodes) {
                var distance = Helper.GetDistance(descriptor, e.Value.Descriptor);
                if (distance < AppConsts.MaxDistance) {
                    return;
                }

                if (nodeid < 0 || distance < mindistance) {
                    nodeid = e.Key;
                    mindistance = distance;
                }
            }

            if (_nodes.Count >= AppConsts.MaxNodes) {
                var node = _nodes[nodeid];
                var weight = node.Weight + 1;
                for (var i = 0; i < 128; i++) {
                    node.Descriptor[i] = (node.Descriptor[i] * node.Weight + descriptor[i]) / weight;
                }

                node.Weight = weight;
                SqlUpdateNode(nodeid, node);
            }
            else {
                nodeid = _nodes.Count + 1;
                var node = new Node(descriptor, 1);
                _nodes.Add(nodeid, node);
                SqlAddNode(nodeid, node);
            }
        }

        public static void AddDescriptors(float[] descriptors)
        {            
            for (var offset = 0; offset < descriptors.Length; offset += 128) {
                var descriptor = new float[128];
                Array.Copy(descriptors, offset, descriptor, 0, 128);
                AddDescriptor(descriptor);
            }
        }

        public static short FindNodeId(float[] descriptor)
        {
            var nodeid = -1;
            var mindistance = float.MaxValue;
            foreach (var e in _nodes) {
                var distance = Helper.GetDistance(descriptor, e.Value.Descriptor);
                if (nodeid < 0 || distance < mindistance) {
                    nodeid = e.Key;
                    mindistance = distance;
                }
            }

            return (short)nodeid;
        }

        public static short[] GetKi(float[] descriptors)
        {
            var ki = new short[descriptors.Length / 128];
            var index = 0;
            for (var offset = 0; offset < descriptors.Length; offset += 128) {
                var descriptor = new float[128];
                Array.Copy(descriptors, offset, descriptor, 0, 128);
                ki[index] = FindNodeId(descriptor);
                index++;
            }

            Array.Sort(ki);
            return ki;
        }

        public static short[][] GetKi(float[][] descriptors)
        {
            var ki = new short[2][];
            for (var i = 0; i < 2; i++) {
                ki[i] = GetKi(descriptors[i]);
            }

            return ki;
        }

        public static float GetSim(short[] x, short[] y)
        {
            var sum1 = 0.0;
            var sum2 = 0.0;
            var i = 0;
            var j = 0;
            var wsum = _nodes.Sum(e => e.Value.Weight);
            while (i < x.Length && j < y.Length) {
                var w = Math.Log10(wsum / _nodes[x[i]].Weight);
                if (x[i] == y[j]) {
                    sum1 += w;
                    sum2 += w;
                    i++;
                    j++;
                }
                else {
                    if (x[i] < y[j]) {
                        sum2 += w;
                        i++;
                    }
                    else {
                        j++;
                    }
                }
            }

            var sim = sum1 * 100f / sum2;
            return (float)sim;
        }

        public static float GetSim(short[] x, short[][] y)
        {
            var sim0 = GetSim(x, y[0]);
            var sim1 = GetSim(x, y[1]);
            var sim = Math.Max(sim0, sim1);
            return sim;
        }
    }
}
