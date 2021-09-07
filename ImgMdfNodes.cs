using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static int GetNodeId(Mat descriptor)
        {
            if (_nodeList.Count == 0) {
                var rootnode = new Node(core: descriptor, depth: 0, childid: 0, members: string.Empty, lastadded: DateTime.Now);
                _nodeList.Add(1, rootnode);
                SqlAddNode(1, rootnode);
                return 1;
            }

            var nodeid = 1;
            do {
                var node = _nodeList[nodeid];
                if (node.Core == null) {
                    node.Core = descriptor;
                    node.LastAdded = DateTime.Now;
                    SqlUpdateNode(nodeid, node);
                    return nodeid;
                }

                var distance = Cv2.Norm(descriptor, node.Core, NormTypes.Hamming);
                if (distance < AppConsts.FeatureSim) {
                    return nodeid;
                }

                var bit = Helper.GetBit(descriptor, node.Depth);
                if (node.ChildId == 0) {
                    var nextid = _nodeList.Count + 1;
                    var depth = node.Depth + 1;
                    node.ChildId = nextid;
                    Node node0, node1;
                    if (bit == 0) {
                        node0 = new Node(core: descriptor, depth: depth, childid: 0, members: string.Empty, lastadded: DateTime.Now);
                        node1 = new Node(core: null, depth: depth, childid: 0, members: string.Empty, lastadded: DateTime.Now);
                    }
                    else {
                        node0 = new Node(core: null, depth: depth, childid: 0, members: string.Empty, lastadded: DateTime.Now);
                        node1 = new Node(core: descriptor, depth: depth, childid: 0, members: string.Empty, lastadded: DateTime.Now);
                    }

                    _nodeList.Add(nextid, node0);
                    SqlAddNode(nextid, node0);
                    _nodeList.Add(nextid + 1, node1);
                    SqlAddNode(nextid + 1, node1);
                    SqlUpdateNode(nodeid, node);
                    return node.ChildId + bit;
                }

                nodeid = node.ChildId + bit;
            }
            while (true);
        }

        private static void AddDescriptors(string name, Mat descriptors)
        {
            var i = 0;
            while (i < descriptors.Rows) {
                var nodeid = GetNodeId(descriptors.Row(i));
                AddMember(nodeid, name);
                i++;
            }
        }

        public static void AddDescriptors(string name, Mat[] descriptors, BackgroundWorker backgroundworker)
        {
            AddDescriptors(name, descriptors[0]);
            AddDescriptors(name, descriptors[1]);
            var livenodes = GetLiveNodesCount();
            if (livenodes > AppConsts.MaxNodes) {
                var nodestodelete = _nodeList
                    .Where(e => e.Value.Core != null)
                    .OrderBy(e => e.Value.Members.Count)
                    .ThenBy(e => e.Value.LastAdded)
                    .Take(livenodes - AppConsts.MaxNodes)
                    .ToArray();
                foreach (var e in nodestodelete) {
                    backgroundworker.ReportProgress(0, $"adjusing ({livenodes - AppConsts.MaxNodes})...");
                    e.Value.Kill();
                    SqlUpdateNode(e.Key, e.Value);
                    livenodes--;
                }
            }
        }

        public static void AddMember(int nodeid, string name)
        {
            var node = _nodeList[nodeid];
            if (node.AddMember(name)) {
                SqlUpdateNode(nodeid, node);
            }
        }
    }
}
