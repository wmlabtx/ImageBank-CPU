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
        private Random _random = new Random();

        public void Compute(BackgroundWorker backgroundworker)
        {
            AppVars.SuspendEvent.WaitOne(Timeout.Infinite);

            Img img1 = null, img2 = null;
            var candidates = new List<Tuple<string, ulong[], byte[], int>>();
            lock (_imglock)
            {
                if (_imgList.Count < 2)
                {
                    backgroundworker.ReportProgress(0, "no images");
                    return;
                }

                var array = _imgList.ToArray();
                var index = _random.Next(array.Length);
                img1 = array[index].Value;
                if (img1.GetDescriptors().Length == 0)
                {
                    return;
                }

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

                if (img1.Folder.StartsWith(AppConsts.FolderDefault))
                {
                    foreach (var e in _imgList)
                    {
                        if (e.Key.Equals(img1.Name))
                        {
                            continue;
                        }

                        var hamming = Intrinsic.PopCnt(img1.Phash ^ e.Value.Phash);
                        candidates.Add(new Tuple<string, ulong[], byte[], int>(
                            e.Value.Name, 
                            e.Value.GetDescriptors(), 
                            e.Value.GetMapDescriptors(),
                            hamming));
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

                        var hamming = Intrinsic.PopCnt(img1.Phash ^ e.Value.Phash);
                        candidates.Add(new Tuple<string, ulong[], byte[], int>(
                            e.Value.Name,
                            e.Value.GetDescriptors(),
                            e.Value.GetMapDescriptors(),
                            hamming));
                    }

                    if (candidates.Count == 0)
                    {
                        foreach (var e in _imgList)
                        {
                            if (e.Key.Equals(img1.Name))
                            {
                                continue;
                            }

                            var hamming = Intrinsic.PopCnt(img1.Phash ^ e.Value.Phash);
                            candidates.Add(new Tuple<string, ulong[], byte[], int>(
                                e.Value.Name,
                                e.Value.GetDescriptors(),
                                e.Value.GetMapDescriptors(),
                                hamming));
                        }
                    }
                }
            }

            var sb = new StringBuilder();
            if (candidates.Count > 0)
            {
                candidates = candidates.OrderBy(e => e.Item4).Take(10000).ToList();
                var name2 = img1.Name;
                var mindistance = AppConsts.MaxDistance;
                foreach (var e in candidates)
                {
                    var distance = ImageHelper.CompareBlob(img1.GetMapDescriptors(), img1.GetDescriptors(), e.Item3, e.Item2);
                    if (distance < mindistance){
                        name2 = e.Item1;
                        mindistance = distance;
                    }
                }

                lock (_imglock)
                {
                    if (_imgList.TryGetValue(name2, out img2))
                    {
                        if (!img1.NextHash.Equals(img2.Hash))
                        {
                            sb.Append($"[{Helper.TimeIntervalToString(DateTime.Now.Subtract(img1.LastCheck))} ago] ");
                            sb.Append($"{img1.Folder}\\{img1.Name}: ");
                            sb.Append($"{img1.Distance:F1} ");
                            sb.Append($"{char.ConvertFromUtf32(mindistance < img1.Distance ? 0x2192 : 0x2193)} ");
                            sb.Append($"{mindistance:F1} ");
                            img1.NextHash = img2.Hash;
                        }

                        if (mindistance < img1.Distance && img1.Distance != AppConsts.MaxDistance)
                        {
                            img1.LastView = GetMinLastView();
                        }

                        if (mindistance != img1.Distance)
                        {
                            img1.Distance = mindistance;
                        }

                        img1.LastCheck = DateTime.Now;
                    }
                }
            }
            
            if (sb.Length > 0) {
                var message = sb.ToString();
                backgroundworker.ReportProgress(0, message);
            }
        }
    }
}