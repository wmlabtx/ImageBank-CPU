using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public int FamilySize(string family)
        {
            if (family == null || family.Length == 0) {
                return 0;
            }

            lock (_imglock) {
                var familysize = _imgList.Count(e => string.CompareOrdinal(e.Value.Family, family) == 0);
                return familysize;
            }
        }

        public void MoveFamily(string oldfamily, string newfamily)
        {
            if (oldfamily == null || oldfamily.Length == 0) {
                return;
            }

            if (newfamily == null || newfamily.Length == 0) {
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
            Contract.Requires(imgX != null);
            Contract.Requires(imgY != null);

            if (!string.IsNullOrEmpty(imgX.Family) && imgX.Family.Equals(imgY.Family, StringComparison.OrdinalIgnoreCase)) {
                imgY.Family = string.Empty;
                return;
            }

            if (string.IsNullOrEmpty(imgX.Family) && string.IsNullOrEmpty(imgY.Family)) {
                string family;
                do {
                    family = Helper.RandomFamily();
                }
                while (FamilySize(family) > 0);

                imgX.Family = family;
                imgY.Family = family;
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
            Contract.Requires(imgX != null);
            imgX.Family = family;
            FastFindNext(imgX);
            AppVars.ImgPanel[1] = GetImgPanel(imgX.NextName);
        }

        public void CopyLeft(Img imgX, Img imgY)
        {
            Contract.Requires(imgX != null);
            Contract.Requires(imgY != null);

            var oldfile = imgX.FileName;
            imgX.Folder = imgY.Folder;
            File.Move(oldfile, imgX.FileName);
            Delete(imgY.Name);
        }
    }
}
