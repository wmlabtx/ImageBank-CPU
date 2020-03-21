using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void FindNext(int idX, out int nextid, out float sim)
        {
            Img imgX;
            ulong[] vectorX;
            var candidates = new List<Tuple<int, ulong[]>>();
            lock (_imglock) {
                sim = 0f;
                nextid = idX;
                if (!_imgList.TryGetValue(idX, out imgX)) {
                    return;
                }

                vectorX = imgX.Vector();
                sim = imgX.Sim;
                nextid = imgX.NextId;
                if (!_imgList.TryGetValue(nextid, out var imgY)) {
                    sim = 0;
                    nextid = idX;
                }
                else {
                    if (!string.IsNullOrEmpty(imgX.Person) && !imgX.Person.Equals(imgY.Person, StringComparison.OrdinalIgnoreCase)) {
                        sim = 0;
                        nextid = idX;
                    }
                }

                foreach (var e in _imgList) {
                    if (e.Value.Id != imgX.Id) {
                        if (string.IsNullOrEmpty(imgX.Person) || imgX.Person.Equals(e.Value.Person, StringComparison.OrdinalIgnoreCase)) {
                            candidates.Add(new Tuple<int, ulong[]>(e.Value.Id, e.Value.Vector()));
                        }
                    }
                }
            }

            var random = new Random();
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < AppConsts.TimeHorizon && candidates.Count > 0) {
                var index = random.Next(candidates.Count);
                var esim = OrbHelper.GetSim(vectorX, candidates[index].Item2);
                if (esim > sim) {
                    sim = esim;
                    nextid = candidates[index].Item1;
                }

                candidates.RemoveAt(index);
            }
        }
    }
}
