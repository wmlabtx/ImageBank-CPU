using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private bool FindNext(int idX, out int nextid, out float sim, out int lastid)
        {            
            var candidates = new List<Tuple<int, ulong[]>>();
            ulong[] vectorX;
            lock (_imglock) {
                if (!_imgList.TryGetValue(idX, out Img imgX)) {
                    nextid = 0;
                    sim = 0f;
                    lastid = 0;
                    return false;
                }

                vectorX = imgX.Vector();
                sim = imgX.Sim;
                nextid = imgX.NextId;
                lastid = imgX.LastId;
                if (lastid == 0 || nextid <= 0 || !_imgList.TryGetValue(nextid, out var imgY)) {
                    sim = 0f;
                    nextid = idX;
                    lastid = 0;
                }

                foreach (var e in _imgList) {
                    if (e.Value.Id != imgX.Id && e.Value.Id > lastid) {
                        candidates.Add(new Tuple<int, ulong[]>(e.Value.Id, e.Value.Vector()));
                    }
                }

                //candidates.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            }

            var sw = Stopwatch.StartNew();
            var index = 0;
            while (sw.ElapsedMilliseconds < AppConsts.TimeHorizon && index < candidates.Count) {
                var esim = OrbHelper.GetSim(vectorX, candidates[index].Item2);
                if (esim > sim) {
                    sim = esim;
                    nextid = candidates[index].Item1;
                }

                index++;
            }

            lastid = index < candidates.Count ? candidates[index].Item1 : _id;
            return true;
        }
    }
}
