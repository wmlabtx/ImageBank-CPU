using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace ImageBank
{
    public partial class ImgMdf
    {
        /*
        private bool FindNext(string idX, out string nextid, out float distance)
        {            
            var candidates = new List<Tuple<string, Mat>>();
            Mat descriptorsX;
            nextid = idX;
            distance = 256f;
            lock (_imglock) {
                if (!_imgList.TryGetValue(idX, out Img imgX)) {
                    return false; 
                }

                descriptorsX = imgX.Descriptors;

                if (imgX.Folder.StartsWith(AppConsts.FolderLegacy, StringComparison.OrdinalIgnoreCase)) {
                    foreach (var e in _imgList) {
                        if (!e.Value.Id.Equals(imgX.Id, StringComparison.OrdinalIgnoreCase)) {
                            candidates.Add(new Tuple<string, Mat>(e.Value.Id, e.Value.Descriptors));
                        }
                    }
                }
                else {
                   foreach (var e in _imgList) {
                        if (!e.Value.Id.Equals(imgX.Id, StringComparison.OrdinalIgnoreCase)) {
                            if ((e.Value.Folder + "\\").StartsWith(imgX.Folder + "\\", StringComparison.OrdinalIgnoreCase)) {
                                candidates.Add(new Tuple<string, Mat>(e.Value.Id, e.Value.Descriptors));
                            }
                        }
                    }
                }
            }

            var index = 0;
            while (index < candidates.Count) {
                var edistance = OrbHelper.GetDistance(descriptorsX, candidates[index].Item2);
                if (string.IsNullOrEmpty(nextid) || edistance < distance) {
                    distance = edistance;
                    nextid = candidates[index].Item1;
                }

                index++;
            }
            
            return true;
        }
        */
    }
}
