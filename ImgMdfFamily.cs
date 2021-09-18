using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void AddToFamily(Img imgX, Img imgY)
        {
            if (imgX.Family == 0 && imgY.Family != 0) {
                imgX.Family = imgY.Family;
                imgX.History.Clear();
                imgX.History.Add(imgY.Id, imgY.Id);
                imgX.SaveHistory();
            }
        }

        public static void RemoveFromFamily(Img imgX)
        {
            if (imgX.Family > 0) {
                imgX.Family = 0;
                ClearHistory(imgX);
            }
        }

        public static void ClearHistory(Img imgX)
        {
            if (imgX.History.Count > 0) {
                imgX.History.Clear();
                imgX.SaveHistory();
            }
        }

        public static void CreateFamily(Img imgX)
        {
            if (imgX.Family == 0) {
                imgX.LastView = DateTime.Now;
                imgX.Family = AllocateFamily();
                imgX.BestId = imgX.Id;
                imgX.BestDistance = 1000f;
                ClearHistory(imgX);
            }
        }

        public static void KillFamily(Img imgX)
        {
            if (imgX.Family > 0) {
                var family = imgX.Family;
                lock (_sqllock) {
                    foreach (var img in _imgList) {
                        if (img.Value.Family == family) {
                            RemoveFromFamily(img.Value);
                        }
                    }
                }
            }
        }

        public static int GetFamilySize(int family)
        {
            int familysize;
            lock (_sqllock) {
                familysize = _imgList.Count(e => e.Value.Family == family);
            }

            return familysize;
        }
    }
}