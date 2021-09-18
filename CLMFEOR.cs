using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBank
{
    public class CLMFEOR : Cfilter
    {


        private int Power;


        // Constructor

        public CLMFEOR(int Power)
        {
            this.Power = Power;
        }


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

            int stride = srcData.Stride;
            int offset = stride - ((fmt == PixelFormat.Format8bppIndexed) ? width : width * 3);
            int i, j, t, ir, jr;

            byte[] ResultTableRed = new byte[Power * Power];
            byte[] ResultTableGreen = new byte[Power * Power];
            byte[] ResultTableBlue = new byte[Power * Power];
            byte[] ResultTableGray = new byte[Power * Power];

            int radius = Power >> 1;


            // do the job
            unsafe {
                byte* p;
                byte* src = (byte*)srcData.Scan0.ToPointer();
                byte* dst = (byte*)dstData.Scan0.ToPointer();


                if (fmt == PixelFormat.Format8bppIndexed) {
                    // Grayscale image

                    // for each line
                    for (int y = 0; y < height; y++) {
                        // for each pixel
                        for (int x = 0; x < width; x++, src++, dst++) {
                            int Current = 0;
                            // for each kernel row
                            byte LMF_E_OR_Gray = new byte();


                            for (i = 0; i < Power; i++) {
                                ir = i - radius;
                                t = y + ir;

                                // skip row
                                if (t < 0)
                                    continue;
                                // break
                                if (t >= height)
                                    break;

                                // for each kernel column


                                for (j = 0; j < Power; j++) {
                                    jr = j - radius;
                                    t = x + jr;

                                    // skip column
                                    if (t < 0)
                                        continue;

                                    if (t < width) {
                                        ResultTableGray[Current] = (byte)(src[ir * stride + jr]);
                                        Current++;
                                    }
                                }
                            }


                            LMF_E_OR_Gray = ResultTableGray[0];

                            for (int k = 1; k < (Current); k++) {


                                LMF_E_OR_Gray |= ResultTableGray[k];

                            }

                            *dst = (LMF_E_OR_Gray > 255) ? (byte)255 : ((LMF_E_OR_Gray < 0) ? (byte)0 : (byte)LMF_E_OR_Gray);
                        }
                        src += offset;
                        dst += offset;
                    }
                }
                else {
                    // RGB image

                    int Current = 0;
                    byte LMF_E_OR_Red = new byte();
                    byte LMF_E_OR_Green = new byte();
                    byte LMF_E_OR_Blue = new byte();

                    // for each line
                    for (int y = 0; y < height; y++) {
                        // for each pixel
                        for (int x = 0; x < width; x++, src += 3, dst += 3) {

                            Current = 0;
                            LMF_E_OR_Red = 0;
                            LMF_E_OR_Green = 0;
                            LMF_E_OR_Blue = 0;




                            // for each kernel row
                            for (i = 0; i < Power; i++) {
                                ir = i - radius;
                                t = y + ir;

                                // skip row
                                if (t < 0)
                                    continue;
                                // break
                                if (t >= height)
                                    break;

                                // for each kernel column
                                for (j = 0; j < Power; j++) {

                                    jr = j - radius;
                                    t = x + jr;

                                    p = &src[ir * stride + jr * 3];

                                    // skip column
                                    if (t < 0)
                                        continue;

                                    if (t < width) {

                                        ResultTableRed[Current] = p[2];
                                        ResultTableGreen[Current] = p[1];
                                        ResultTableBlue[Current] = p[0];
                                        Current++;

                                    }
                                }
                            }


                            LMF_E_OR_Red = ResultTableRed[0];
                            LMF_E_OR_Green = ResultTableGreen[0];
                            LMF_E_OR_Blue = ResultTableBlue[0];

                            for (int k = 1; k < (Power * Power); k++) {


                                LMF_E_OR_Red |= ResultTableRed[k];
                                LMF_E_OR_Green |= ResultTableGreen[k];
                                LMF_E_OR_Blue |= ResultTableBlue[k];
                            }

                            dst[2] = LMF_E_OR_Red;
                            dst[1] = LMF_E_OR_Green;
                            dst[0] = LMF_E_OR_Blue;
                        }
                        src += offset;
                        dst += offset;
                    }
                }
            }
            // unlock both images
            dstImg.UnlockBits(dstData);
            srcImg.UnlockBits(srcData);

            return dstImg;
        }
    }

}
