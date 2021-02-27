using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private readonly Random _random = new Random();

        public void FindCandidates(Img img1, out List<Tuple<string, ulong, ulong[]>> candidates)
        {
            candidates = new List<Tuple<string, ulong, ulong[]>>();
            lock (_imglock)
            {
                if (img1.Folder.StartsWith(AppConsts.FolderDefault))
                {
                    foreach (var e in _imgList)
                    {
                        if (e.Key.Equals(img1.Name))
                        {
                            continue;
                        }

                        if (img1.IsInHistory(e.Value.Hash))
                        {
                            continue;
                        }

                        candidates.Add(new Tuple<string, ulong, ulong[]>(
                            e.Value.Name,
                            e.Value.Phash,
                            e.Value.GetDescriptors()));
                    }
                }
                else
                {
                    foreach (var e in _imgList)
                    {
                        if (e.Key.Equals(img1.Name))
                        {
                            continue;
                        }

                        if (!e.Value.Folder.Equals(img1.Folder))
                        {
                            continue;
                        }

                        if (img1.IsInHistory(e.Value.Hash))
                        {
                            continue;
                        }

                        candidates.Add(new Tuple<string, ulong, ulong[]>(
                            e.Value.Name,
                            e.Value.Phash,
                            e.Value.GetDescriptors()));
                    }

                    if (candidates.Count < 2)
                    {
                        foreach (var e in _imgList)
                        {
                            if (e.Key.Equals(img1.Name))
                            {
                                continue;
                            }

                            if (img1.IsInHistory(e.Value.Hash))
                            {
                                continue;
                            }

                            candidates.Add(new Tuple<string, ulong, ulong[]>(
                                e.Value.Name,
                                e.Value.Phash,
                                e.Value.GetDescriptors()));
                        }
                    }
                }
            }
        }

        public static void FindCandidate(Img img1, List<Tuple<string, ulong, ulong[]>> candidates, bool fast, out string name2)
        {
            var mindistance = AppConsts.MaxDistance;
            var x = img1.GetDescriptors();
            name2 = img1.Name;
            foreach (var e in candidates)
            {
                var distance = fast ? Intrinsic.PopCnt(img1.Phash ^ e.Item2) : ImageHelper.CompareBlob(x, e.Item3);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    name2 = e.Item1;
                }
            }
        }

        public void Compute(BackgroundWorker backgroundworker)
        {
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);
            Img img1 = null;
            Img img2;
            lock (_imglock)
            {
                if (_imgList.Count < 2)
                {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                img1 = _imgList
                    .OrderBy(e => e.Value.LastView)
                    .Take(1000)
                    .OrderBy(e => e.Value.LastCheck)
                    .FirstOrDefault()
                    .Value;

                /*
                var array = _imgList.ToArray();
                var index = _random.Next(array.Length);
                img1 = array[index].Value;
                */

                /*
                foreach (var e in _imgList)
                {
                    var eX = e.Value;
                    if (img1 != null &&
                        img1.LastCheck <= eX.LastCheck)
                    {
                        continue;
                    }

                    img1 = eX;
                }
                */

                if (!_hashList.TryGetValue(img1.NextHash, out img2))
                {
                    img1.NextHash = img1.Hash;
                    img1.Distance = AppConsts.MaxDistance;
                }
                else
                {
                    if (!img1.Folder.StartsWith(AppConsts.FolderDefault) && !img1.Folder.Equals(img2.Folder))
                    {
                        img1.NextHash = img1.Hash;
                        img1.Distance = AppConsts.MaxDistance;
                    }
                }

                var history = img1.History;
                var offset = history.Length - AppConsts.HashLength;
                while (offset >= 0)
                {
                    var prevhash = history.Substring(offset, AppConsts.HashLength);
                    if (!_hashList.ContainsKey(prevhash))
                    {
                        img1.RemoveFromHistory(prevhash);
                    }

                    offset -= AppConsts.HashLength;
                }
            }

            var sb = new StringBuilder();
            FindCandidates(img1, out List<Tuple<string, ulong, ulong[]>> candidates);
            if (candidates.Count > 0)
            {
                Img img2s;
                var fast = img1.Folder.StartsWith(AppConsts.FolderDefault);
                FindCandidate(img1, candidates, fast, out string name2s);
                lock (_imglock)
                {
                    if (!_imgList.TryGetValue(name2s, out img2s))
                    {
                        return;
                    }
                }

                var distances = ImageHelper.CompareBlob(img1.GetDescriptors(), img2s.GetDescriptors());
                Img img2r = null;
                var distancer = float.MaxValue;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 1000 && candidates.Count > 0)
                {
                    var index = _random.Next(candidates.Count);
                    var name2rt = candidates[index].Item1;
                    candidates.RemoveAt(index);
                    if (!_imgList.TryGetValue(name2rt, out var img2rt))
                    {
                        continue;
                    }

                    var distancert = ImageHelper.CompareBlob(img1.GetDescriptors(), img2rt.GetDescriptors());
                    if (distancert < distancer)
                    {
                        distancer = distancert;
                        img2r = img2rt;
                    }
                }

                float distance;
                if (distancer < distances)
                {
                    distance = distancer;
                    img2 = img2r;
                }
                else
                {
                    distance = distances;
                    img2 = img2s;

                }

                lock (_imglock)
                {
                    if (distance < img1.Distance)
                    {
                        if (!img1.NextHash.Equals(img2.Hash))
                        {
                            sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                            sb.Append($"{img1.Folder}\\{img1.Name}: ");
                            sb.Append($"{img1.Distance:F1} ");
                            sb.Append($"{char.ConvertFromUtf32(distance < img1.Distance ? 0x2192 : 0x2193)} ");
                            sb.Append($"{distance:F1} ");
                            img1.NextHash = img2.Hash;
                        }

                        if (distance < img1.Distance)
                        {
                            img1.History = string.Empty;
                        }

                        if (distance != img1.Distance)
                        {
                            img1.Distance = distance;
                        }
                    }

                    img1.LastCheck = DateTime.Now;
                }
            }
            
            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}