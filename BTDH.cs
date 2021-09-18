using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public class BTDH
    {
        public Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight) { 
            Bitmap result = new Bitmap(nWidth, nHeight); 
            using (Graphics g = Graphics.FromImage((Image)result)) 
                g.DrawImage(b, 0, 0, nWidth, nHeight); return result; 
        }

        public BTDH(int TextureAreas, int BrightnessAreas, bool SpatialInformation)
        {
            this.TextureAreas = TextureAreas;
            this.BrightnessAreas = BrightnessAreas;
            this.SpatialInformation = SpatialInformation;

            if (TextureAreas == 16) {

                TextureMembershipValues = new double[64] {
                                                    0,0,0,1,
                                                    0,1,1,2,
                                                    1,2,2,3,
                                                    2,3,3,4,
                                                    3,4,4,5,
                                                    4,5,5,6,
                                                    5,6,6,7,
                                                    6,7,7,8,
                                                    7,8,8,9,
                                                    8,9,9,10,
                                                    9,10,10,11,
                                                    10,11,11,12,
                                                    11,12,12,13,
                                                    12,13,13,14,
                                                    13,14,14,15,
                                                    14,15,15,15 };


            }
            else {
                TextureMembershipValues = new double[32] {
                                                    0,0,0,1,
                                                    0,1,1,2,
                                                    1,2,2,3,
                                                    2,3,3,4,
                                                    3,4,4,5,
                                                    4,5,5,6,
                                                    5,6,6,7,
                                                    6,7,7,8 };

            }

            if (BrightnessAreas == 16) {
                LumMembershipValues = new double[64] { 0,0,1.71, 12.25,
                                                       1.71,12.25,12.25,22.53,
                                                       12.25,22.53,22.53, 35.38,
                                                       22.53, 35.38,35.38, 50.38,
                                                       35.38, 50.38,50.38, 65.60,
                                                       50.38, 65.60,65.60, 82.41,
                                                       65.60, 82.41,82.41, 99.99,
                                                       82.41, 99.99,99.99,116.92,
                                                       99.99,116.92,116.92,134.31,
                                                       116.92 ,134.31 ,134.31 , 153.65,
                                                       134.31,153.65 ,153.65 , 173.36,
                                                       153.65,173.36 ,173.36 ,193.70 ,
                                                       173.36, 193.70,193.70 ,214.88 ,
                                                       193.70, 214.88,214.88 , 234.91,
                                                       214.88,234.91 ,234.91 , 251.23,
                                                       234.91, 251.23, 255 ,255  };

            }
            else {

                LumMembershipValues = new double[32] { 0,0,3.18, 22.68,
                                                       3.18,22.68,22.68,54.00,
                                                       22.68,54.00,54.00, 90.13,
                                                       54.00, 90.13,90.13, 125.80,
                                                       90.13, 125.80,125.80, 162.57,
                                                       125.80, 162.57,162.57, 202.25,
                                                       162.57, 202.25,202.25, 243.64,
                                                       202.25, 243.64,255,255 };
            }

        }


        double[] QuantizationTable = new double[8]
                {

                   0.0625,0.1875,0.3125,0.4375,0.5625,0.6875,0.8125,0.9375


                };

        int TextureAreas;
        int BrightnessAreas;
        bool SpatialInformation;

        double[] LumMembershipValues;
        double[] TextureMembershipValues;


        int[] SpatialHilbertPossition = new int[16] { 1, 2, 5, 6, 3, 4, 7, 8, 9, 10, 13, 14, 11, 12, 15, 16 };


        private double[] FindMembershipValueForTriangles(double Input, double[] Triangles, int NumberOfTriangles)
        {
            int Temp = 0;

            double[] MembershipFunctionToSave = new double[NumberOfTriangles];

            for (int i = 0; i <= Triangles.Length - 1; i += 4) {

                MembershipFunctionToSave[Temp] = 0;

                //Бн еЯнбй бксйвьт уфз кпсхцЮ
                if (Input >= Triangles[i + 1] && Input <= +Triangles[i + 2]) {
                    MembershipFunctionToSave[Temp] = 1;
                    break;
                }

                //Бн еЯнбй деойЬ фпх фсйгюнпх    
                if (Input >= Triangles[i] && Input < Triangles[i + 1]) {
                    MembershipFunctionToSave[Temp] = (Input - Triangles[i]) / (Triangles[i + 1] - Triangles[i]);
                }

                //Бн еЯнбй бсйуфесб фпх фсйгюнпх    

                if (Input > Triangles[i + 2] && Input <= Triangles[i + 3]) {
                    MembershipFunctionToSave[Temp] = (Input - Triangles[i + 2]) / (Triangles[i + 2] - Triangles[i + 3]) + 1;
                }

                Temp += 1;
            }

            return (MembershipFunctionToSave);

        }

        private int[,] grayScales;
        private int imgWidth, imgHeight;
        public double[] histogram; // stores all three tamura features in one histogram.
        public double[] BTDH_Descriptor; // The descriptor
        private static double[,] filterH = { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
        private static double[,] filterV = { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };


        public double[] directionality()
        {
            double[] histogram = new double[TextureAreas * BrightnessAreas];
            double maxResult = 3;
            double binWindow = maxResult / (double)(16 - 1);
            double DeltaV = 0;
            double DeltaH = 0;
            double TextureForm = 0;
            double[] TextureParticipation = new double[TextureAreas];
            double[] LumParticipation = new double[BrightnessAreas];
            int tempcount = 0;

            for (int x = 1; x < this.imgWidth - 1; x++) {
                for (int y = 1; y < this.imgHeight - 1; y++) {
                    DeltaV = this.calculateDeltaV(x, y);
                    DeltaH = this.calculateDeltaH(x, y);

                    if (DeltaV == 0 && DeltaH == 0) {
                        TextureForm = 0;
                    }
                    else TextureForm = (Math.PI / 2 + Math.Atan(DeltaV / DeltaH)) / binWindow;



                    int DivFactor = 1;
                    if (TextureAreas == 8) DivFactor = 2;

                    TextureParticipation = FindMembershipValueForTriangles(TextureForm / DivFactor, TextureMembershipValues, TextureAreas);
                    LumParticipation = FindMembershipValueForTriangles(grayScales[x, y], LumMembershipValues, BrightnessAreas);

                    tempcount = 0;

                    for (int T = 0; T < TextureAreas; T++) {
                        for (int Q = 0; Q < BrightnessAreas; Q++) {
                            histogram[tempcount] += TextureParticipation[T] * LumParticipation[Q];
                            tempcount++;

                        }

                    }


                }
            }

            return histogram;
        }


        public double calculateDeltaH(int x, int y)
        {
            double result = 0;

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    result = result + this.grayScales[x - 1 + i, y - 1 + j] * filterH[i, j];
                }
            }
            return result;
        }


        public double calculateDeltaV(int x, int y)
        {
            double result = 0;

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    result = result + this.grayScales[x - 1 + i, y - 1 + j] * filterV[i, j];
                }
            }
            return result;
        }


        public double[] extract(Bitmap image)
        {
            // image prefiltering
            int L_Blocks = 1;

            CStreach PreFiltering = new CStreach();
            image = PreFiltering.Apply(image);


            CLMFEOR Filtering = new CLMFEOR(3);
            image = Filtering.Apply(image);


            if (SpatialInformation == false) {
                //image resize --> if no spatial information image is resized to 64X64
                image = ResizeBitmap(image, 240, 240);

            }
            else {
                //image resize --> if spatial information image is resized to 240*240
                image = ResizeBitmap(image, 240, 240);
                L_Blocks = 16;  //Define the number of blocks

            }


            BTDH_Descriptor = new double[L_Blocks * TextureAreas * BrightnessAreas];

            histogram = new double[TextureAreas * BrightnessAreas];
            double[] directionality;

            imgWidth = image.Width;
            imgHeight = image.Height;

            int[] tmp = new int[3];

            int[,] GrayImage = new int[imgWidth, imgHeight];

            PixelFormat fmt = (image.PixelFormat == PixelFormat.Format8bppIndexed) ?
                               PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;

            BitmapData srcData = image.LockBits(
               new Rectangle(0, 0, imgWidth, imgHeight),
               ImageLockMode.ReadOnly, fmt);

            int offset = srcData.Stride - srcData.Width * 3;


            unsafe {
                byte* src = (byte*)srcData.Scan0.ToPointer();

                for (int yi = 0; yi < imgHeight; yi++) {
                    for (int xi = 0; xi < imgWidth; xi++, src += 3) {


                        int mean = (int)(0.114 * src[0] + 0.587 * src[1] + 0.299 * src[2]);

                        GrayImage[xi, yi] = mean;


                    }

                    src += offset;


                }

            }

            image.UnlockBits(srcData);





            // Get the No Spatial Descriptor


            if (SpatialInformation == false) {
                this.grayScales = new int[imgWidth, imgHeight];
                grayScales = GrayImage;

                directionality = this.directionality(); // Get the descriptor


                // Normalize the descriptor

                double TempSum = 0;

                for (int i = 0; i < TextureAreas * BrightnessAreas; i++) {
                    histogram[i] = directionality[i];
                    TempSum += histogram[i];
                }

                for (int i = 0; i < TextureAreas * BrightnessAreas; i++) {
                    histogram[i] = histogram[i] / TempSum;
                    BTDH_Descriptor[i] = histogram[i];
                }

            }

            else   // Get the Spatial Descriptor
            {
                // if each image is 240 * 240 the each image block is 240/8 =30. 

                imgWidth = imgWidth / 4;
                imgHeight = imgHeight / 4;

                this.grayScales = new int[imgWidth, imgHeight];

                // X==Y == L{Block}. The implementation is for L{Block}=8
                int CurrentBlock = 0;


                for (int x = 0; x < 4; x++) {

                    for (int y = 0; y < 4; y++) {

                        // get the block information
                        for (int gx = 0; gx < imgWidth; gx++) {

                            for (int gy = 0; gy < imgHeight; gy++) {

                                grayScales[gx, gy] = GrayImage[x * imgWidth + gx, y * imgHeight + gy];
                            }
                        }


                        directionality = this.directionality(); // Get the descriptor for eack block


                        for (int i = 0; i < TextureAreas * BrightnessAreas; i++) {
                            BTDH_Descriptor[(SpatialHilbertPossition[CurrentBlock] - 1) * TextureAreas * BrightnessAreas + i] = directionality[i];

                        }

                        CurrentBlock++;

                    }
                }


                //Normalize Histogram

                double[] TempSum = new double[16];

                for (int j = 0; j < 16; j++) {
                    for (int i = 0; i < TextureAreas * BrightnessAreas; i++) {
                        TempSum[j] += BTDH_Descriptor[j * TextureAreas * BrightnessAreas + i];
                    }
                }

                for (int j = 0; j < 16; j++) {
                    for (int i = 0; i < TextureAreas * BrightnessAreas; i++) {
                        BTDH_Descriptor[j * TextureAreas * BrightnessAreas + i] = BTDH_Descriptor[j * TextureAreas * BrightnessAreas + i] / TempSum[j];
                    }
                }




            }


            Quant Quantize = new Quant(TextureAreas, BrightnessAreas, SpatialInformation);
            BTDH_Descriptor = Quantize.Apply(BTDH_Descriptor);

            return (BTDH_Descriptor);


        }



        public double GetDistanceSpatial(double[] Descriptor1, double[] Descriptor2, int Blocks, int Texture_Times_Brightness)
        {
            double[] TempDescriptro1 = new double[3 * Texture_Times_Brightness];
            double[] TempDescriptro2 = new double[3 * Texture_Times_Brightness];

            double[] TempDescriptro1Bountaries = new double[2 * Texture_Times_Brightness];
            double[] TempDescriptro2Bountaries = new double[2 * Texture_Times_Brightness];

            double Distance = 0;


            for (int i = 1; i < Blocks - 1; i++) {
                for (int j = 0; j < (Texture_Times_Brightness); j++) {
                    TempDescriptro1[j] = Descriptor1[(i - 1) * Texture_Times_Brightness + j];
                    TempDescriptro1[Texture_Times_Brightness + j] = Descriptor1[i * Texture_Times_Brightness + j];
                    TempDescriptro1[(2 * Texture_Times_Brightness) + j] = Descriptor1[(i + 1) * Texture_Times_Brightness + j];

                    TempDescriptro2[j] = Descriptor2[(i - 1) * Texture_Times_Brightness + j];
                    TempDescriptro2[Texture_Times_Brightness + j] = Descriptor2[i * Texture_Times_Brightness + j];
                    TempDescriptro2[(2 * Texture_Times_Brightness) + j] = Descriptor2[(i + 1) * Texture_Times_Brightness + j];

                }

                Distance += GetDistanceNOSpatial(TempDescriptro1, TempDescriptro2);

            }

            for (int j = 0; j < (Texture_Times_Brightness); j++) {
                TempDescriptro1Bountaries[j] = Descriptor1[j];
                TempDescriptro1Bountaries[Texture_Times_Brightness + j] = Descriptor1[Texture_Times_Brightness + j];

                TempDescriptro2Bountaries[j] = Descriptor2[j];
                TempDescriptro2Bountaries[Texture_Times_Brightness + j] = Descriptor2[Texture_Times_Brightness + j];
            }

            Distance += GetDistanceNOSpatial(TempDescriptro1Bountaries, TempDescriptro2Bountaries);

            for (int j = 0; j < (Texture_Times_Brightness); j++) {
                TempDescriptro1Bountaries[j] = Descriptor1[(Blocks - 2) * Texture_Times_Brightness + j];
                TempDescriptro1Bountaries[Texture_Times_Brightness + j] = Descriptor1[(Blocks - 1) * Texture_Times_Brightness + j];

                TempDescriptro2Bountaries[j] = Descriptor1[(Blocks - 2) * Texture_Times_Brightness + j];
                TempDescriptro2Bountaries[Texture_Times_Brightness + j] = Descriptor2[(Blocks - 1) * Texture_Times_Brightness + j];

            }


            Distance += GetDistanceNOSpatial(TempDescriptro1Bountaries, TempDescriptro2Bountaries);


            return (Distance);

        }

        public double GetDistanceNOSpatial(double[] Table1, double[] Table2)
        {
            double Result = 0;
            double Temp1 = 0;
            double Temp2 = 0;

            double TempCount1 = 0, TempCount2 = 0, TempCount3 = 0;

            for (int i = 0; i < Table1.Length; i++) {
                Temp1 += Table1[i];
                Temp2 += Table2[i];
            }

            if (Temp1 == 0 || Temp2 == 0) Result = 100;
            if (Temp1 == 0 && Temp2 == 0) Result = 0;

            if (Temp1 > 0 && Temp2 > 0) {
                for (int i = 0; i < Table1.Length; i++) {
                    TempCount1 += (Table1[i] / Temp1) * (Table2[i] / Temp2);
                    TempCount2 += (Table2[i] / Temp2) * (Table2[i] / Temp2);
                    TempCount3 += (Table1[i] / Temp1) * (Table1[i] / Temp1);

                }

                Result = (100 - 100 * (TempCount1 / (TempCount2 + TempCount3 - TempCount1))); //Tanimoto
            }

            return (Result);

        } //
    }
}
