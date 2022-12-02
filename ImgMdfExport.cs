using System;
using System.Drawing.Imaging;
using System.IO;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Export(int idpanel, IProgress<string> progress)
        {
            var img = AppPanels.GetImgPanel(idpanel).Img;
            var filename = $"{AppConsts.PathRoot}\\{img.Name}";
            var imagedata = File.ReadAllBytes(filename);
            if (imagedata == null) {
                return;
            }

            using (var bitmap = BitmapHelper.ImageDataToBitmap(imagedata)) {
                if (bitmap == null) {
                    return;
                }

                bitmap.Save($"{AppConsts.PathRw}\\{img.Name}.png", ImageFormat.Png);
            }
        }
    }
}
