using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        /*
        private static int GetAvailableNodeId()
        {
            var nodeid = 0;
            lock (_imglock) {
                nodeid = _nodeList.Max(e => e.Value.NodeId) + 1; 
            }

            return nodeid;
        }

        private static int GetNodeSize(int nodeid)
        {
            var size = 0;
            lock (_imglock) {
                size = _imgList.Count(e => e.Value.Node[0] == nodeid || e.Value.Node[1] == nodeid);
            }

            return size;
        }

        private static void SplitNode(int index, int nodeid)
        {
            lock (_imglock) {
                var scope = _imgList.Where(e => e.Value.Node[0] == nodeid || e.Value.Node[1] == nodeid).Select(e => e.Value).ToArray();
                var distances = new Tuple<Img, float>[scope.Length];
                var rindex = _random.NextShort(0, (short)(scope.Length - 1));
                var core = scope[rindex].Vector[index];
                for (var i = 0; i < scope.Length; i++) {
                    var distance = ImageHelper.GetCosineSimilarity(core, scope[i].Vector[index]);
                    distances[i] = new Tuple<Img, float>(scope[i], distance);
                }

                distances = distances.OrderByDescending(e => e.Item2).ToArray();
                var mindex = scope.Length / 2;
                var radius = distances[mindex].Item2;
                var nodeid0 = GetAvailableNodeId();
                var node0 = new Node(nodeid: nodeid0, previd: nodeid, core: Array.Empty<float>(), radius: 0f, childid0: 0, childid1: 0);
                Add(node0);
                var nodeid1 = GetAvailableNodeId();
                var node1 = new Node(nodeid: nodeid1, previd: nodeid, core: Array.Empty<float>(), radius: 0f, childid0: 0, childid1: 0);
                Add(node1);
                var node = _nodeList[nodeid];
                node.Core = core;
                node.Radius = radius;
                node.SetChildId(0, nodeid0);
                node.SetChildId(1, nodeid1);
                for (var i = 0; i < distances.Length; i++) {
                    if (distances[i].Item2 > radius) {
                        distances[i].Item1.SetNode(index, nodeid0);
                    }
                    else {
                        distances[i].Item1.SetNode(index, nodeid1);
                    }
                }
            }
        }

        private static void RemoveNode(int nodeid)
        {
            lock (_imglock) {
                var currentnode = _nodeList[nodeid];
                var fatherid = currentnode.PrevId;
                var fathernode = _nodeList[fatherid];
                if (fathernode.NodeId > 2) {
                    var grandfatherid = fathernode.PrevId;
                    var grandfathernode = _nodeList[grandfatherid];
                    var broid = fathernode.ChildId[0] == nodeid ? fathernode.ChildId[1] : fathernode.ChildId[0];
                    if (grandfathernode.ChildId[0] == fatherid) {
                        grandfathernode.SetChildId(0, broid);
                    }
                    else {
                        grandfathernode.SetChildId(1, broid);
                    }

                    _nodeList.Remove(nodeid);
                    SqlDelete(nodeid);
                    _nodeList.Remove(fatherid);
                    SqlDelete(fatherid);
                }
            }
        }

        private static void AssignNode(int index, Img img1)
        {
            lock (_imglock) {
                var nodeid = index + 1;
                while (!_nodeList[nodeid].IsLeaf()) {
                    var distance = ImageHelper.GetCosineSimilarity(img1.Vector[index], _nodeList[nodeid].Core);
                    if (distance > _nodeList[nodeid].Radius) {
                        nodeid = _nodeList[nodeid].ChildId[0];
                    }
                    else {
                        nodeid = _nodeList[nodeid].ChildId[1];
                    }
                }

                img1.SetNode(index, nodeid);
                var size = GetNodeSize(nodeid);
                if (size > AppConsts.MaxImagesInNode) {
                    SplitNode(index, nodeid);
                }
            }
        }

        private static Img[] GetCandidates(int index, Img img1)
        {
            Img[] candidates = null;
            lock (_imglock) {
                if (img1.Node[index] == 0) {
                    AssignNode(index, img1);
                }

                var nodeid = img1.Node[index];
                candidates = _imgList.Where(e => e.Value.Node[index] == nodeid).Select(e => e.Value).ToArray();
                if (candidates.Length <= 1) {
                    candidates = _imgList.Select(e => e.Value).ToArray();
                    var shuffle = candidates.OrderBy(x => _random.GetRandom64()).Take(10).ToArray();
                    candidates = shuffle;
                }
            }

            return candidates;
        }

        private static void CheckNode(int index, int nodeid)
        {
            lock (_imglock) {
                var candidates = _imgList.Where(e => e.Value.Node[index] == nodeid).Select(e => e.Value).ToArray();
                if (candidates.Length <= 1) {
                    if (candidates.Length == 1) {
                        candidates[0].Node[index] = 0;
                    }

                    RemoveNode(nodeid);
                }
            }
        }
        */
    }
}
