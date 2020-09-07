using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void FlannUpdate()
        {
            Img[] imgs;
            lock (_imglock) {
                imgs = _imgList
                    .Where(e => e.Value.GetDescriptors() != null)
                    .Select(e => e.Value)
                    .ToArray();
            }

            if (imgs.Length == 0) {
                return;
            }

            var descriptors = imgs
                .Select(e => e.GetDescriptors())
                .ToArray();

            lock (_flannlock) {
                _flannBasedMatcher.Clear();
                _flannBasedMatcher.Add(descriptors);
                _flannBasedMatcher.Train();
                _flannNames = imgs
                    .Select(e => e.Name)
                    .ToArray();

                _flannAvailable = true;
            }
        }

        private string FlannFindNextName(Img imgX)
        {
            lock (_flannlock) {
                var votes = new SortedDictionary<string, float>();
                var dmatcharray = _flannBasedMatcher.KnnMatch(imgX.GetDescriptors(), 16);
                lock (_imglock) {
                    foreach (var dmatch in dmatcharray) {
                        foreach (var d in dmatch) {
                            if (d.Distance < 64) {
                                var vote = 64 - d.Distance;
                                var name = _flannNames[d.ImgIdx];
                                if (!name.Equals(imgX.Name, StringComparison.OrdinalIgnoreCase)) {
                                    if (votes.ContainsKey(name)) {
                                        votes[name] += vote;
                                    }
                                    else {
                                        votes.Add(name, vote);
                                    }
                                }
                            }
                        }
                    }

                    if (votes.Count > 0) {
                        var candidates = votes.OrderByDescending(e => e.Value).Select(e => e.Key).ToArray();
                        foreach (var candidate in candidates) {
                            if (_imgList.ContainsKey(candidate)) {
                                if (!imgX.IsInHistory(candidate)) {
                                    return candidate;
                                }
                            }
                        }

                        _flannAvailable = false;
                        return null;
                    }
                    else {
                        return imgX.Name;
                    }
                }
            }
        }
    }
}
