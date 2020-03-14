using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void FamilyCombine(int idx, int idy)
        {
            lock (_imglock) {
                if (!_imgList.TryGetValue(idx, out var imgX)) {
                    return;
                }

                if (!_imgList.TryGetValue(idy, out var imgY)) {
                    return;
                }

                if (imgX.Family > 0 && imgX.Family == imgY.Family) {
                    return;
                }

                if (imgX.Family <= 0 && imgY.Family <= 0) {
                    var id = AllocateFamily();
                    imgX.Family = id;
                    imgY.Family = id;
                }
                else {
                    if (imgX.Family > 0 && imgY.Family <= 0) {
                        imgY.Family = imgX.Family;
                    }
                    else {
                        if (imgX.Family <= 0 && imgY.Family > 0) {
                            imgX.Family = imgY.Family;
                        }
                        else {
                            var yfamily = _imgList
                                .Values
                                .Where(e => e.Family == imgY.Family)
                                .ToArray();

                            foreach (var img in yfamily) {
                                img.Family = imgX.Family;
                            }
                        }
                    }
                }
            }
        }

        public void FamilyBreak(int idx, int idy)
        {
            lock (_imglock) {
                if (!_imgList.TryGetValue(idx, out var imgX)) {
                    return;
                }

                if (!_imgList.TryGetValue(idy, out var imgY)) {
                    return;
                }

                if (imgX.Family <= 0 || imgX.Family != imgY.Family) {
                    return;
                }

                var yfamily = _imgList
                    .Values
                    .Where(e => e.Family == imgY.Family)
                    .ToArray();

                foreach (var img in yfamily) {
                    img.Family = 0;
                }
            }
        }
    }
}
