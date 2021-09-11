using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void AddToFamily(Img imgX, Img imgY)
        {
            if (imgX.Family == 0 || imgX.Family > imgY.Family) {
                imgX.Family = imgY.Family;
                imgX.History.Clear();
                imgX.History.Add(imgY.Id, imgY.Id);
                imgX.SaveHistory();
            }
            else {
                imgY.Family = imgX.Family;
                imgY.History.Clear();
                imgY.History.Add(imgX.Id, imgX.Id);
                imgY.SaveHistory();
            }

            imgX.LastView = DateTime.Now;
        }

        public static void RemoveFromFamily(Img imgX)
        {
            imgX.Family = 0;
            imgX.History.Clear();
            imgX.SaveHistory();
        }

        public static void ClearHistory(Img imgX)
        {
            imgX.History.Clear();
            imgX.SaveHistory();
        }

        public static void CreateFamily(Img imgX)
        {
            if (imgX.Family > 0) {
                return;
            }

            imgX.LastView = DateTime.Now;
            imgX.Family = AllocateFamily();
            imgX.History.Clear();
            imgX.SaveHistory();
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