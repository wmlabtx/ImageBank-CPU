using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public sealed class CStreach : Cfilter
    {
        // Apply filter
        public Bitmap Apply(Bitmap srcImg)
        {
            // get source image size
            int width = srcImg.Width;
            int height = srcImg.Height;

            PixelFormat fmt = (srcImg.PixelFormat == PixelFormat.Format8bppIndexed) ?
                        PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;

            // lock source bitmap data
            BitmapData srcData = srcImg.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, fmt);

            // create new image
            Bitmap dstImg = new Bitmap(width, height);

            // lock destination bitmap data
            BitmapData dstData = dstImg.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, fmt);

            int offset = srcData.Stride - ((fmt == PixelFormat.Format8bppIndexed) ? width : width * 3);


            double[] GrayLuminance = new double[256];
            double[] RedLuminance = new double[256];
            double[] GreenLuminance = new double[256];
            double[] BlueLuminance = new double[256];


            for (int i = 0; i < 256; i++) {
                GrayLuminance[i] = 0;
                RedLuminance[i] = 0;
                GreenLuminance[i] = 0;
                BlueLuminance[i] = 0;
            }

            // Getting the histogram
            // do the job
            unsafe {
                byte* src = (byte*)srcData.Scan0.ToPointer();

                if (fmt == PixelFormat.Format8bppIndexed) {

                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++, src++) {

                            GrayLuminance[(int)*src] += 1;

                        }
                        src += offset;

                    }
                }
                else {

                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++, src += 3) {
                            int red = src[2];
                            int green = src[1];
                            int blue = src[0];

                            RedLuminance[red] += 1;
                            GreenLuminance[green] += 1;
                            BlueLuminance[blue] += 1;


                        }
                        src += offset;

                    }
                }
            }

            // end of histogram

            double MinimumRed = FindMin(RedLuminance);
            double MinimumGreen = FindMin(GreenLuminance);
            double MinimumBlue = FindMin(BlueLuminance);
            double MinimumGray = FindMin(GrayLuminance);

            double MaximumRed = FindMax(RedLuminance, width * height);
            double MaximumGreen = FindMax(GreenLuminance, width * height);
            double MaximumBlue = FindMax(BlueLuminance, width * height);
            double MaximumGray = FindMax(GrayLuminance, width * height);


            // do the job
            unsafe {
                byte* src = (byte*)srcData.Scan0.ToPointer();
                byte* dst = (byte*)dstData.Scan0.ToPointer();

                if (fmt == PixelFormat.Format8bppIndexed) {
                    // grayscale invert
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++, src++, dst++) {
                            // convert each pixel

                            *dst = (byte)(255 * (((double)*src - MinimumGray) / (MaximumGray - MinimumGray)));


                        }
                        src += offset;
                        dst += offset;
                    }
                }
                else {
                    // RGB invert
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++, src += 3, dst += 3) {
                            // ivert each pixel

                            dst[2] = (byte)(255 * (((double)src[2] - MinimumRed) / (MaximumRed - MinimumRed)));
                            dst[1] = (byte)(255 * (((double)src[1] - MinimumGreen) / (MaximumGreen - MinimumGreen)));
                            dst[0] = (byte)(255 * (((double)src[0] - MinimumBlue) / (MaximumBlue - MinimumBlue)));


                        }
                        src += offset;
                        dst += offset;
                    }
                }
            }
            // unlock both images
            dstImg.UnlockBits(dstData);
            srcImg.UnlockBits(srcData);
            // end of filtering

            return dstImg;
        }

        private double FindMin(double[] Input)
        {
            double Result = 0;
            int Temp1 = 0;

            for (int i = 0; i < Input.Length; i++) {
                Temp1 += (int)Input[i];
                if (Temp1 > 0) {
                    Result = (double)i;
                    break;
                }
            }
            return (Result);

        }

        private double FindMax(double[] Input, int Dimensions)
        {
            double Result = 0;
            int Temp1 = 0;

            for (int i = 0; i < Input.Length; i++) {
                Temp1 += (int)Input[i];
                if (Temp1 == Dimensions) {
                    Result = (double)i;
                    break;
                }
            }
            return (Result);

        }


    }



}
