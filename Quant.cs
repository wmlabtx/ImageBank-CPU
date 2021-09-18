using System;

namespace ImageBank
{
    public class Quant
    {

        int TextureAreas;
        int BrightnessAreas;
        bool Spatial;

        public Quant(int TextureAreas, int BrightnessAreas, bool Spatial)
        {
            this.TextureAreas = TextureAreas;
            this.BrightnessAreas = BrightnessAreas;
            this.Spatial = Spatial;
        }

        private double[,] SpatialOLDTable = new double[16, 8] {
    {4.50E-06,  0.00124246350675031,    0.00456973152853036,    0.00978171300991577,    0.0173796482009469, 0.0287739408026226, 0.0509860910289666, 0.13054252852},
    {1.46974023546088E-06,0.00082162471497779,0.00351865274608443,0.00742758523426112,0.0129176218164252,0.0208053977858975,0.0333525471139184,0.0745309461365227},
    {4.65941674785077E-06,0.00149616740469469,0.00473775137061115,0.00980576338135794,0.017005548650521,0.0278969242028933,0.0501364985877576,0.125952225159413},
    {5.36634177038635E-06,0.0012300443738355,0.00446172062548841,0.00938841498090264,0.016610466476276,0.026857521070813,0.0518438787164721,0.414310053141076},
    {4.6279780182511E-06,0.00120270445055479,0.00450658150506715,0.00972243713810707,0.0172968164459785,0.0224556620933577,0.0524723732972723,0.441612087145171},
    {5.7292741615542E-06,0.00145570616752219,0.00424125612611452,0.00903020717871653,0.0162628627210892,0.0275518369787849,0.0516643204123384,0.492703745973503},
    {3.83623272932965E-06,0.000858012039029967,0.00323792802566004,0.00679316707681369,0.0117302355455334,0.018502731199495,0.0290225323530541,0.0595726709743482},
    {3.44627704327834E-06,0.000910924176463719,0.00344400238930029,0.00723415033540935,0.0124970891336459,0.0216871245811705,0.0306672918746653,0.067851565396865},
    {5.25275093964139E-06,0.00119938232153515,0.00426737297522299,0.00963562171793311,0.0191262325691893,0.0325876057589331,0.0530403774567776,0.185332795737594},
    {4.75889162316316E-06,0.00118653976083603,0.00449602258541521,0.00960133096416046,0.0219734170985487,0.0284089402818994,0.0526403019320785,0.319644230791},
    {2.94864334612111E-06,0.000826517264475425,0.00352303923116031,0.00746401548885986,0.0129480418232672,0.0206142910346901,0.032859126308479,0.0736894468391272},
    {1.84958299860586E-06,0.000956038672412262,0.00378830343086018,0.00607855618631713,0.0138524758413968,0.0220729572288803,0.0353701572472348,0.0855835511779838},
    {2.99904790731771E-06,0.00107551255079509,0.00426224726135623,0.00909600688133965,0.0156918143777354,0.0251008626036777,0.0405834535056431,0.114699485454238},
    {2.678116922376935E-06,0.00120164374829441,0.00471447566134928,0.00800231185972554,0.0177588411941401,0.0297279420586928,0.0542803491030661,0.330371085188034},
    {3.43891636436031E-06,0.00172503503198207,0.00422059654141053,0.00904065517778227,0.016301473144174,0.0281829013994913,0.0547893228366567,0.182821931659683},
    {2.75E-06,  0.001134712,    0.004375753,    0.009482266,    0.01730313, 0.02998095, 0.057176423,    0.410466067}


        };





        private static double[] QuantTable3 = { 0.00035861762528768723, 0.0022793044360157073, 0.0045336266247823125, 0.0071998477438984755, 0.010858943799110959, 0.016146473626141064, 0.031466608147416614, 0.56719876196068508 };

        private static double[] QuantTable4 = { 0.00031822637141413562, 0.001936792542869522, 0.0036844475457703117, 0.0057242270081077611, 0.0083900166150163245, 0.012529918117551194, 0.023997249021176122, 0.53158634841563446 };

        private static double[] QuantTable5 = { 0.00032971937214338394, 0.0018777464074620942, 0.0035149474642130005, 0.0053337455459429349, 0.0075520769660119436, 0.010624219361124067, 0.016466885109217851, 0.040288866228097096 };

        private static double[] QuantTable6 = { 0.00036745455559561777, 0.00212069559104055, 0.00405009269277973, 0.0062613225600007313, 0.0088308849056867363, 0.012375724598824311, 0.020220426222299713, 0.0686184583927647 };

        private static double[] QuantTable7 = { 0.00032326049785118906, 0.0017879111620344621, 0.0034277853103210115, 0.0053535231679812867, 0.0078736045499481037, 0.011152485271941858, 0.017153171845605775, 0.038584452261010753 };

        private static double[] QuantTable8 = { 0.00030814967593383025, 0.0017469320958295375, 0.003312937685796311, 0.0052875072865408304, 0.0078483137085537009, 0.011171171539780561, 0.017802525919799663, 0.055665250740670844 };

        private static double[] QuantTable9 = { 0.00036229034408965356, 0.0020112064340277406, 0.0038891802080074241, 0.0060790293418711162, 0.0085537812714586714, 0.012544536779362373, 0.02112478092472549, 0.077678179913336462 };

        private static double[] QuantTable10 = { 0.00035823934115519979, 0.0021351601323515734, 0.0039690913083005527, 0.00620911552004438, 0.00902512341162859, 0.013190469131101026, 0.02422448460443807, 0.18668069703210538 };

        private static double[] QuantTable11 = { 0.00037270038592385522, 0.0023132958487435385, 0.0043681047182135774, 0.0069901649580760049, 0.010462542979675733, 0.01571136416117111, 0.028929481960353314, 0.24177084901124096 };

        private static double[] QuantTable12 = { 0.00041605262839343433, 0.0024783292992877908, 0.00485347775891699, 0.0076087641472461559, 0.011319386594497036, 0.018519876861957753, 0.053243808356033276, 0.68047990796556956 };

        private static double[] QuantTable13 = { 0.00036190987206291829, 0.0020784607015249994, 0.0039327771101254358, 0.006259840012128432, 0.00926892471332372, 0.01389992511868526, 0.027103743433719091, 0.4364488263571118 };

        private static double[] QuantTable14 = { 0.000369777312280419, 0.0022342679152454531, 0.0042899500866882866, 0.006938602069535337, 0.010552830448349563, 0.0163137463020355, 0.034752029851805366, 0.38142181096897676 };

        private static double[] QuantTable15 = { 0.00033223210375315512, 0.0020187704656425731, 0.0038477644187828773, 0.0061881584160416768, 0.0093970682154760812, 0.013972880393185548, 0.028170346167945889, 0.37382710971303124 };

        private static double[] QuantTable16 = { 0.00027359516774617303, 0.0018040956955412968, 0.003611034780779046, 0.0057309892351163072, 0.0089064810085068367, 0.013421159963829182, 0.026100620692603251, 0.36367980249357834 };

        private static double[] QuantTable17 = { 0.00029970584173539888, 0.0019570093458816094, 0.0039207189083075829, 0.0060629000866591065, 0.0088445498702165344, 0.013088158823369824, 0.025540790535958181, 0.36692900847370186 };

        private static double[] QuantTable18 = { 0.00031835786741941008, 0.0020813509480875321, 0.0042289226822749678, 0.0067762865754409157, 0.0101802111044336, 0.015167293978227536, 0.030627998537876663, 0.43333440529819872 };




        private static double[] QuantTable19 = { 0.0007956603846864292, 0.0045844891600577663, 0.0087627215538677358, 0.013926931927122552, 0.019802336727250774, 0.02975105285966206, 0.055308055071450146, 0.40428900544099272 };

        private static double[] QuantTable20 = { 0.00084298362804544633, 0.0051047161838910261, 0.0099127221934486066, 0.015235733324905937, 0.022021345215648181, 0.033426388808185756, 0.065487576943133535, 0.590259468067587 };

        private static double[] QuantTable21 = { 0.00078266956120968067, 0.0044881149288320743, 0.008502501193293684, 0.013652386266283758, 0.019753953436237114, 0.028355405853564941, 0.055333860990877982, 0.555714695772752 };

        private static double[] QuantTable22 = { 0.0008273093000202939, 0.0050499679941942181, 0.0094950523076609891, 0.014358645075507116, 0.020408831818531144, 0.029759251896460878, 0.053683419627095622, 0.29171504426068529 };

        private static double[] QuantTable23 = { 0.00071872422890074147, 0.0041465277683807355, 0.0077945226215580714, 0.012152528854468074, 0.018160930397856861, 0.027155916270613732, 0.05071851851640128, 0.3568346069085036 };

        private static double[] QuantTable24 = { 0.00067541516216399551, 0.0039220184735201981, 0.0077127245781583951, 0.012035478857483272, 0.017535343928709413, 0.025680533674346164, 0.047647089309922175, 0.3062898875776271 };

        private static double[] QuantTable25 = { 0.00069089177999538264, 0.0041568571742699777, 0.0078454697471378367, 0.013133489470506226, 0.019243914571783149, 0.027582829421791931, 0.049760331425850714, 0.31075321690513513 };

        private static double[] QuantTable26 = { 0.00089190547337314571, 0.0052366236933889523, 0.010181963890735257, 0.015825944255342592, 0.02256402970410876, 0.032951107573878841, 0.062195352160545343, 0.34383664490507504 };



        private static double[] QuantTable27 = { 0.00023159381515987256, 0.0017539031130623505, 0.0039893835318425, 0.0068026405374449518, 0.010708118051526798, 0.017460020015034675, 0.039593371837015404, 0.56565570021286893 };

        private static double[] QuantTable28 = { 0.00026596877884360723, 0.0018632179457198084, 0.004028339175942381, 0.00649319409081693, 0.0098004494917205282, 0.015175271369089157, 0.027205815016005823, 0.082733034780121825 };

        private static double[] QuantTable29 = { 0.00020662128248271169, 0.0013777429785044131, 0.0031423106609729727, 0.0051298130177201886, 0.0076225315553733091, 0.011174051766427766, 0.017960434191324613, 0.044636325288693095 };

        private static double[] QuantTable30 = { 0.00024328096433519453, 0.0016068108782400722, 0.0034791984506726049, 0.0056945019014632012, 0.0086088843061209955, 0.012838922319144424, 0.021328918968089448, 0.0622787642296141 };

        private static double[] QuantTable31 = { 0.00024030514008461707, 0.0017221425662821475, 0.0038886035689272751, 0.0066586070023841922, 0.010468450911215078, 0.017370082256927967, 0.038558553889901911, 0.36020119648283211 };

        private static double[] QuantTable32 = { 0.00023525371625589793, 0.0017052025161047126, 0.0039468584007968741, 0.0067320817657782707, 0.010318407018027543, 0.017039695389447773, 0.038713122473173253, 0.46005603777828813 };

        private static double[] QuantTable33 = { 0.00023827610121592664, 0.0017618954217592845, 0.0040069616201534493, 0.0068640276176982684, 0.010633452980076159, 0.017982448315797924, 0.040422105128201734, 0.39307817465610673 };

        private static double[] QuantTable34 = { 0.00021926447537790657, 0.0017171160166628157, 0.0038138346905925219, 0.0065200177078928731, 0.010132589546509651, 0.016775901661125279, 0.035939975537944951, 0.3952432884686779 };




        public double[] Apply(double[] Local_Edge_Histogram)
        {
            if (Spatial) {
                return (QuantizationSpatial(Local_Edge_Histogram));
            }
            else {
                if (BrightnessAreas == 8 && TextureAreas == 16) return (QuantizationT16B8(Local_Edge_Histogram));
                else if (BrightnessAreas == 8 && TextureAreas == 8) return (QuantizationT8B8(Local_Edge_Histogram));
                else if (BrightnessAreas == 16 && TextureAreas == 8) return (QuantizationT8B16(Local_Edge_Histogram));
                else return (null);
            }
        }


        private double[] QuantizationSpatial(double[] Local_Edge_Histogram)
        {

            double[] Edge_HistogramElement = new double[2048];
            double[] ElementsDistance = new double[8];
            double Max = 1;


            double[,] SpatialQuantTable = new double[16, 8];
            /*
                       int[] NEW = new int[64] { 1, 2, 5, 6, 9, 10, 13, 14, 3, 4, 7, 8, 11, 12, 15, 16, 17, 18, 21, 22, 25, 26, 29, 30, 19, 20, 23, 24, 27, 28, 31, 32, 33, 34, 37, 38, 41, 42, 45, 46, 35, 36, 39, 40, 43, 44, 47, 48, 49, 50, 53, 54, 57, 58, 61, 62, 51, 52, 55, 56, 59, 60, 63, 64 };


                       int[] OLD = new int[64] { 22, 21, 20, 17, 16, 15, 2, 1, 23, 24, 19, 18, 13, 14, 3, 4, 26, 25, 30, 31, 12, 9, 8, 5, 27, 28, 29, 32, 11, 10, 7, 6, 38, 37, 36, 33, 54, 55, 58, 59, 39, 40, 35, 34, 53, 56, 57, 60, 42, 41, 46, 47, 52, 51, 62, 61, 43, 44, 45, 48, 49, 50, 63, 64 };
                   */

            int[] OLD = new int[16] { 6, 5, 4, 1, 7, 8, 3, 2, 10, 9, 14, 15, 11, 12, 13, 16 };

            int[] NEW = new int[16] { 1, 2, 5, 6, 3, 4, 7, 8, 9, 10, 13, 14, 11, 12, 15, 16 };



            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 8; j++) {

                    SpatialQuantTable[NEW[i] - 1, j] = SpatialOLDTable[OLD[i] - 1, j];
                }
            }

            int Current = 0;

            for (int T = 0; T < 16; T++) {
                Current = 0;

                for (int i = T * 128 + Current; i < T * 128 + Current + 128; i++) {
                    Edge_HistogramElement[i] = 0;
                    for (int j = 0; j < 8; j++) {
                        ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - SpatialQuantTable[T, j]);
                    }
                    Max = 1;
                    for (int j = 0; j < 8; j++) {
                        if (ElementsDistance[j] < Max) {
                            Max = ElementsDistance[j];
                            Edge_HistogramElement[i] = j;
                        }
                    }
                }
                Current = Current + 128;
            }


            return Edge_HistogramElement;

        }

        private double[] QuantizationT8B16(double[] Local_Edge_Histogram)
        {

            double[] Edge_HistogramElement = new double[128];
            double[] ElementsDistance = new double[8];
            double Max = 100000;

            for (int i = 0; i < 16; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable27[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }
            }

            for (int i = 16; i < 32; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable28[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 32; i < 48; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable29[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 48; i < 64; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable30[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 64; i < 80; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable31[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 80; i < 96; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable32[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 96; i < 112; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable33[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 112; i < 128; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable34[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            return Edge_HistogramElement;



        }


        private double[] QuantizationT8B8(double[] Local_Edge_Histogram)
        {

            double[] Edge_HistogramElement = new double[64];
            double[] ElementsDistance = new double[8];
            double Max = 100000;

            for (int i = 0; i < 8; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable19[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }

            for (int i = 8; i < 16; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable20[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 16; i < 24; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable21[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 24; i < 32; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable22[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 32; i < 40; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable23[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 40; i < 48; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable24[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 48; i < 56; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable25[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 56; i < 64; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable26[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            return Edge_HistogramElement;



        }

        private double[] QuantizationT16B8(double[] Local_Edge_Histogram)
        {

            double[] Edge_HistogramElement = new double[128];
            double[] ElementsDistance = new double[8];
            double Max = 100000;

            for (int i = 0; i < 8; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable3[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }

            for (int i = 8; i < 16; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable4[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 16; i < 24; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable5[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 24; i < 32; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable6[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 32; i < 40; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable7[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 40; i < 48; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable8[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 48; i < 56; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable9[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 56; i < 64; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable10[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 64; i < 72; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable11[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 72; i < 80; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable12[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 80; i < 88; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable13[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 88; i < 96; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable14[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }

            for (int i = 96; i < 104; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable15[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 104; i < 112; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable16[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }



            for (int i = 112; i < 120; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable17[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            for (int i = 120; i < 128; i++) {
                Edge_HistogramElement[i] = 0;
                for (int j = 0; j < 8; j++) {
                    ElementsDistance[j] = Math.Abs(Local_Edge_Histogram[i] - QuantTable18[j]);
                }
                Max = 1;
                for (int j = 0; j < 8; j++) {
                    if (ElementsDistance[j] < Max) {
                        Max = ElementsDistance[j];
                        Edge_HistogramElement[i] = j;
                    }
                }


            }


            return Edge_HistogramElement;



        }

    }

}
