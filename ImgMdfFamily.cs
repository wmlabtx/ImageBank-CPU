using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void CombineFamily()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;

            if (imgX.FamilyId == 0 && imgY.FamilyId == 0) {
                var familyid = AppImgs.AllocateFamilyId();
                imgX.SetFamilyId(familyid);
                imgY.SetFamilyId(familyid);
            }
            else {
                if (imgX.FamilyId != 0 && imgY.FamilyId == 0) {
                    imgY.SetFamilyId(imgX.FamilyId);
                }
                else {
                    if (imgX.FamilyId == 0 && imgY.FamilyId != 0) {
                        imgX.SetFamilyId(imgY.FamilyId);
                    }
                    else {
                        var familyid = Math.Min(imgX.FamilyId, imgY.FamilyId);
                        imgX.SetFamilyId(familyid);
                        imgY.SetFamilyId(familyid);
                    }
                }
            }
        }

        public static void SplitFamily()
        {
            AppVars.ImgPanel[0].Img.SetFamilyId(0);
            AppVars.ImgPanel[1].Img.SetFamilyId(0);
            AppImgs.SetFirst(AppVars.ImgPanel[1].Img.Id);
        }
    }
}
