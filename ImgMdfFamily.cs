using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static int GetFamilySize(int family)
        {
            lock (_imglock) {
                var familysize = _imgList.Count(e => e.Value.Family == family);
                return familysize;
            }
        }

        public static void SetFamily(Img imgX, int family)
        {
            lock (_imglock) {
                imgX.Family = family;
                var familysize = 0;
                Img imgF = null;
                foreach (var img in _imgList) {
                    if (!img.Value.Name.Equals(imgX.Name, StringComparison.OrdinalIgnoreCase) && img.Value.Family == family) {
                        if (imgF == null) {
                            imgF = img.Value;
                        }

                        familysize++;
                    }
                }

                if (familysize > 0) {
                    if (_hashList.TryGetValue(imgX.NextHash, out var imgY)) {
                        if (imgY.Family != family) {
                            imgX.NextHash = imgF.Hash;
                            imgX.Sim = ImageHelper.GetSim(imgX.Ki, imgX.Kx, imgX.Ky, imgF.Ki, imgF.Kx, imgF.Ky, imgF.KiMirror, imgF.KxMirror, imgF.KyMirror);
                            imgX.LastChanged = DateTime.Now;
                            imgX.LastCheck = GetMinLastCheck();
                            AppVars.ImgPanel[1] = GetImgPanel(imgF.Name);
                        }
                    }
                }
            }
        }
    }
}
