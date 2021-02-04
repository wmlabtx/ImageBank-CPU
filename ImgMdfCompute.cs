using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static readonly Random Random = new Random();

        public void Compute(BackgroundWorker backgroundworker)
        {
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            Img img1 = null, img2 = null;
            byte counter = 0;
            var candidates = new List<Img>();
            lock (_imglock)
            {
                if (_imgList.Count < 2)
                {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                var scopeview = _imgList.OrderBy(e => e.Value.LastView).Take(100).ToArray();
                var i1 = Random.Next(scopeview.Length);
                img1 = scopeview[i1].Value;

                /*
                var minc = scopeview.Min(e => e.Value.Counter);
                scopeview = scopeview.Where(e => e.Value.Counter == minc).ToArray();
                var minlc = scopeview.Min(e => e.Value.LastCheck);
                img1 = scopeview.FirstOrDefault(e => e.Value.LastCheck == minlc).Value;
                */

                if (!_hashList.TryGetValue(img1.NextHash, out img2))
                {
                    img1.NextHash = img1.Hash;
                    img1.Distance = AppConsts.MaxDistance;
                    img1.Counter = 0;
                }
                else
                {
                    if (!img1.Folder.StartsWith(AppConsts.FolderDefault) && !img1.Folder.Equals(img2.Folder))
                    {
                        img1.NextHash = img1.Hash;
                        img1.Distance = AppConsts.MaxDistance;
                        img1.Counter = 0;
                    }
                }

                if (img1.Folder.StartsWith(AppConsts.FolderDefault))
                {
                    foreach (var e in _imgList)
                    {
                        if (e.Key.Equals(img1.Name))
                        {
                            continue;
                        }

                        candidates.Add(e.Value);
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

                        candidates.Add(e.Value);
                    }

                    if (candidates.Count == 0)
                    {
                        foreach (var e in _imgList)
                        {
                            if (e.Key.Equals(img1.Name))
                            {
                                continue;
                            }

                            candidates.Add(e.Value);
                        }
                    }
                }
            }

            counter = (byte)Math.Min(AppConsts.MaxDescriptorsInImage, img1.Counter + 1);
            var mindistance = float.MaxValue;
            img2 = img1;
            foreach (var e in candidates)
            {
                var distance = ImageHelper.GetDistance(img1.GetDescriptors(), e.GetDescriptors(), counter);
                if (distance < mindistance)
                {
                    mindistance = distance;
                    img2 = e;
                }
            }

            var sb = new StringBuilder();
            
            if (!img1.NextHash.Equals(img2.Hash))
            {
                sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                sb.Append($" ({counter}) ");
                sb.Append($"{img1.Folder}\\{img1.Name}: ");
                sb.Append($"{img1.Distance:F2} ");
                sb.Append($"{char.ConvertFromUtf32(mindistance < img1.Distance ? 0x2192 : 0x2193)} ");
                sb.Append($"{mindistance:F2} ");
                img1.NextHash = img2.Hash;
            }

            if (mindistance != img1.Distance)
            {
                img1.Distance = mindistance;
            }

            img1.LastCheck = DateTime.Now;
            img1.Counter = counter;

            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}