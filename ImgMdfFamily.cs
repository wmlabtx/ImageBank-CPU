using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static int FamilySize(int family)
        {
            if (family == 0) {
                return 0;
            }

            lock (_imglock) {
                var familysize = _imgList.Count(e => e.Value.Family == family);
                return familysize;
            }
        }

        public static void MoveFamily(int oldfamily, int newfamily)
        {
            if (oldfamily == 0) {
                return;
            }

            _imgList
                .Where(e => e.Value.Family == oldfamily)
                .Select(e => e.Value)
                .ToList()
                .ForEach(e => e.Family = newfamily);
        }

        public static void CombineFamilies(Img imgX, Img imgY)
        {
            if (imgX.Family != 0 && imgX.Family == imgY.Family) {
                imgY.Family = 0;
                return;
            }

            if (imgX.Family == 0 && imgY.Family == 0) {
                int family;
                lock (_imglock) {
                    family = _imgList.Max(e => e.Value.Family) + 1;
                }

                imgX.Family = family;
                imgY.Family = family;
                return;
            }

            if (imgX.Family != 0 && imgY.Family == 0) {
                imgY.Family = imgX.Family;
                return;
            }

            if (imgX.Family == 0 && imgY.Family != 0) {
                imgX.Family = imgY.Family;
                return;
            }

            var sizeX = FamilySize(imgX.Family);
            var sizeY = FamilySize(imgY.Family);

            if (sizeX < sizeY) {
                MoveFamily(imgX.Family, imgY.Family);
            }
            else {
                MoveFamily(imgY.Family, imgX.Family);
            }
        }
    }
}