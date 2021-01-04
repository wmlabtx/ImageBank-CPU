using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public int FamilySize(string family)
        {
            if (string.IsNullOrEmpty(family)) {
                return 0;
            }

            lock (_imglock) {
                var familysize = _imgList.Count(e => string.CompareOrdinal(e.Value.Family, family) == 0);
                return familysize;
            }
        }

        private void MoveFamily(string oldfamily, string newfamily)
        {
            if (string.IsNullOrEmpty(oldfamily)) {
                return;
            }

            if (string.IsNullOrEmpty(newfamily)) {
                return;
            }

            lock (_imglock) {
                _imgList
                .Where(e => e.Value.Family.Equals(oldfamily, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Value)
                .ToList()
                .ForEach(e => e.Family = newfamily);
            }
        }

        public void CombineFamilies(Img imgX, Img imgY)
        {
            if (!string.IsNullOrEmpty(imgX.Family) && imgX.Family.Equals(imgY.Family, StringComparison.OrdinalIgnoreCase)) {
                imgY.Family = string.Empty;
                return;
            }

            if (string.IsNullOrEmpty(imgX.Family) && string.IsNullOrEmpty(imgY.Family)) {
                imgX.Family = "";
                imgY.Family = "";
                return;
            }

            if (!string.IsNullOrEmpty(imgX.Family) && string.IsNullOrEmpty(imgY.Family)) {
                imgY.Family = imgX.Family;
                return;
            }

            if (string.IsNullOrEmpty(imgX.Family) && !string.IsNullOrEmpty(imgY.Family)) {
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

        public void AssignFamily(Img imgX, string family)
        {
            imgX.Family = family;
            AppVars.ImgPanel[1] = GetImgPanel(imgX.NextName);
        }

        public void CopyLeft(Img imgX, Img imgY)
        {
            var oldfile = imgX.FileName;
            imgX.Folder = imgY.Folder;
            File.Move(oldfile, imgX.FileName);
            Delete(imgY.Name);
        }
    }
}
