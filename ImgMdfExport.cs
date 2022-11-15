using System;
using System.Drawing.Imaging;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Export(int idpanel, IProgress<string> progress)
        {
            var img = AppPanels.GetImgPanel(idpanel).Img;
            var filename = FileHelper.NameToFileName(img.Name);
            var imagedata = FileHelper.ReadData(filename);
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
