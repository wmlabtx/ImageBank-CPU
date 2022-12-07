using System;
using System.IO;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Export(int idpanel, IProgress<string> progress)
        {
            var img = AppPanels.GetImgPanel(idpanel).Img;
            var filename = FileHelper.NameToFileName(img.Name);
            var imagedata = FileHelper.ReadEncryptedFile(filename);
            if (imagedata == null) {
                return;
            }

            using (var mi = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                var ext = mi.Format.ToString().ToLower();
                var exportfilename = $"{AppConsts.PathRw}\\{img.Name}{ext}";
                File.WriteAllBytes(exportfilename, imagedata);
                progress?.Report($"Exported {exportfilename}");
            }
        }
    }
}
