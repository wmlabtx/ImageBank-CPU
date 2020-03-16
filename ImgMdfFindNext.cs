using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void FindNext(int idX, out int lastid, out int nextid, out float distance)
        {
            Img imgX;
            ulong[] scalarX;
            var scalars = new List<Tuple<int, ulong[]>>();
            var distances = new List<Tuple<int, int>>();
            lock (_imglock) {
                imgX = _imgList[idX];
                scalarX = imgX.Scalar();
                lastid = imgX.LastId;
                distance = imgX.Distance;
                nextid = imgX.NextId;
                if (!_imgList.ContainsKey(nextid)) {
                    lastid = 0;
                    distance = 256f;
                    nextid = idX;
                }

                foreach (var e in _imgList) {
                    if (e.Value.Id > lastid && e.Value.Id != imgX.Id) {
                        if (imgX.Person == 0 || imgX.Person == e.Value.Person) {
                            scalars.Add(new Tuple<int, ulong[]>(e.Value.Id, e.Value.Scalar()));
                        }
                    }
                }
            }

            foreach (var e in scalars) {
                var hamming = OrbHelper.GetDistance(scalarX, e.Item2);
                if (distances.Count == 0) {
                    distances.Add(new Tuple<int, int>(e.Item1, hamming));
                }
                else {
                    if (hamming < distances[distances.Count - 1].Item2) {
                        for (var i = 0; i < distances.Count; i++) {
                            if (hamming < distances[i].Item2) {
                                distances.Insert(i, new Tuple<int, int>(e.Item1, hamming));
                                break;
                            }
                        }

                        if (distances.Count > AppConsts.FindHorizon) {
                            distances.RemoveRange(AppConsts.FindHorizon, distances.Count - AppConsts.FindHorizon);
                        }
                    }
                }
            }

            var vectors = new List<Tuple<int, Mat>>();
            Mat vectorX;
            lock (_imglock) {
                vectorX = imgX.Vector();
                foreach (var e in distances) {
                    if (_imgList.TryGetValue(e.Item1, out Img imgY)) {
                        vectors.Add(new Tuple<int, Mat>(e.Item1, imgY.Vector()));
                    }
                }
            }

            foreach (var e in vectors) {
                var edistance = OrbHelper.GetDistance(vectorX, e.Item2);
                if (edistance < distance) {
                    distance = edistance;
                    nextid = e.Item1;
                }
            }

            lastid = _id;
        }
    }
}
