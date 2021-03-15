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
        public void FindCandidates(Img img1, out List<Tuple<Img, int>> candidates)
        {
            candidates = new List<Tuple<Img, int>>();
            lock (_imglock)
            {
                foreach (var e in _imgList)
                {
                    if (e.Key.Equals(img1.Name))
                    {
                        continue;
                    }

                    var distance = Intrinsic.PopCnt(img1.Phash ^ e.Value.Phash);
                    candidates.Add(new Tuple<Img, int>(e.Value, distance));
                }
            }

            if (candidates.Count > 1) {
                candidates = candidates.OrderBy(e => e.Item2).ToList();
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

                if (img1.Folder.Length != 2)
                {
                    if (ImageHelper.GetImageDataFromFile(img1.FileName,
                        out byte[] imagedata,
                        out _,
                        out _)) {
                        img1.Folder = Helper.ComputeFolder(imagedata);
                    }
                }

                /*
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
                */

                /*
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
                */
            }

            var sb = new StringBuilder();
            FindCandidates(img1, out List<Tuple<Img, int>> candidates);
            if (candidates.Count > 0) {
                img2 = img1;
                var distance = img1.Distance;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 50 && candidates.Count > 0)
                {
                    var imgt = candidates[0].Item1;
                    var distancet = ImageHelper.CompareBlob(img1.GetDescriptors(), imgt.GetDescriptors());
                    if (distancet < distance)
                    {
                        img2 = imgt;
                        distance = distancet;
                        break;
                    }

                    candidates.RemoveAt(0);
                }

                if (img1.Distance < AppConsts.MaxDistance)
                {
                    sw.Restart();
                    while (sw.ElapsedMilliseconds < 1000 && candidates.Count > 0)
                    {
                        var index = _random.Next(candidates.Count);
                        var imgt = candidates[index].Item1;
                        var distancet = ImageHelper.CompareBlob(img1.GetDescriptors(), imgt.GetDescriptors());
                        if (distancet < distance)
                        {
                            img2 = imgt;
                            distance = distancet;
                        }

                        candidates.RemoveAt(index);
                    }
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

                        if (distance < 50f) {
                            img1.Counter = 0;
                        }

                        img1.Distance = distance;
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