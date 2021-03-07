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

        public void FindCandidates(Img img1, out List<Img> candidates)
        {
            candidates = new List<Img>();
            lock (_imglock)
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

                    candidates.Add(e.Value);
                }
            }
        }

        /*
        public static void FindCandidate(Img img1, List<Tuple<string, ulong, ulong[]>> candidates, out string name2)
        {
            var mindistance = AppConsts.MaxDistance;
            var x = img1.GetDescriptors();
            name2 = img1.Name;
            foreach (var e in candidates)
            {
                var distance = ImageHelper.CompareBlob(x, e.Item3);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    name2 = e.Item1;
                }
            }
        }
        */

        public static void FindCandidateByPhash(Img img1, List<Img> candidates, out string name2)
        {
            var mindistance = AppConsts.MaxDistance;
            name2 = img1.Name;
            foreach (var e in candidates)
            {
                var distance = Intrinsic.PopCnt(img1.Phash ^ e.Phash);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    name2 = e.Name;
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
                    //.OrderBy(e => e.Value.LastView)
                    //.Take(1000)
                    .OrderBy(e => e.Value.LastCheck)
                    .FirstOrDefault()
                    .Value;

                if (!_hashList.TryGetValue(img1.NextHash, out img2))
                {
                    img1.NextHash = img1.Hash;
                    img1.Distance = AppConsts.MaxDistance;
                }

                if (img1.Width == 0 || img1.Height == 0 || img1.Size == 0)
                {
                    if (ImageHelper.GetImageDataFromFile(img1.FileName,
                        out byte[] imagedata,
                        out var bitmap,
                        out _))
                    {
                        img1.Width = bitmap.Width;
                        img1.Height = bitmap.Height;
                        img1.Size = imagedata.Length;
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
            FindCandidates(img1, out List<Img> candidates);
            if (candidates.Count > 0)
            {
                Img img2s;
                FindCandidateByPhash(img1, candidates, out string name2s);
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
                var index = candidates.Count - 1;
                var dim1 = img1.Width * img1.Height;
                var rw1 = img1.Width * 100 / img1.Height;
                var rh1 = img1.Height * 100 / img1.Width;
                while (index >= 0)
                {
                    var img2rt = candidates[index];
                    if (img2rt.Width > 0 && img2rt.Height > 0 && img2rt.Size > 0)
                    {
                        var dim2 = img2rt.Width * img2rt.Height;
                        var r2 = img2rt.Width * 100 / img2rt.Height;
                        if ((dim1 == dim2) || (rw1 == r2) || (rh1 == r2))
                        {
                            candidates.RemoveAt(index);
                            var distancert = ImageHelper.CompareBlob(img1.GetDescriptors(), img2rt.GetDescriptors());
                            if (distancert < distancer)
                            {
                                distancer = distancert;
                                img2r = img2rt;
                            }
                        }
                    }

                    index--;
                }

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 1000 && candidates.Count > 0)
                {
                    index = _random.Next(candidates.Count);
                    var img2rt = candidates[index];
                    candidates.RemoveAt(index);
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
                            sb.Append($"{img1.Folder:D2}\\{img1.Name}: ");
                            sb.Append($"{img1.Distance:F1} ");
                            sb.Append($"{char.ConvertFromUtf32(distance < img1.Distance ? 0x2192 : 0x2193)} ");
                            sb.Append($"{distance:F1} ");
                            img1.NextHash = img2.Hash;
                        }

                        if (distance < img1.Distance)
                        {
                            img1.History = string.Empty;
                            //if (img1.Distance < AppConsts.MaxDistance)
                            //{
                                //img1.LastView = GetMinLastView();
                            //}
                        }

                        if (distance != img1.Distance)
                        {
                            img1.Distance = distance;
                            img1.LastView = GetMinLastView();
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