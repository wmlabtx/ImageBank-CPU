using OpenCvSharp;
using System.Data.SqlClient;
using System.Text;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Populate()
        {
            LoadImages(null);
            var imgcounter = 0;
            using (var matcollector = new Mat()) {
                foreach (var img in _imgList.Values) {
                    if (img.Id % 250 != 0) {
                        continue;
                    }

                    var filename = FileHelper.NameToFileName(img.Name);
                    var imagedata = FileHelper.ReadData(filename);
                    if (imagedata == null) {
                        continue;
                    }

                    var matpixels = LabHelper.GetColors(imagedata);
                    using (var matfloat = new Mat()) {
                        matpixels.ConvertTo(matfloat, MatType.CV_32F);
                        matcollector.PushBack(matfloat);
                        imgcounter++;
                    }

                    if (matcollector.Rows > 1024 * 1024 * 1024) {
                        break;
                    }
                }

                using (var bestLabels = new Mat())
                using (var matcenters = new Mat()) {
                    Cv2.Kmeans(
                        data: matcollector,
                        k: 256,
                        bestLabels: bestLabels,
                        criteria: new TermCriteria(CriteriaTypes.Eps, maxCount: 1000, epsilon: 1.0),
                        attempts: 3,
                        flags: KMeansFlags.PpCenters,
                        centers: matcenters);
                    matcenters.GetArray(out float[] centers);
                    var buffer = Helper.ArrayFromFloat(centers);
                    SqlVarsUpdateProperty(AppConsts.AttributeLabCenters, buffer);
                }
            }
        }

        public static void DrawPalette()
        {
            using (var centers = new Mat(256, 3, MatType.CV_32F)) {
                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttributeLabCenters} ");
                sb.Append($"FROM {AppConsts.TableVars}");
                var sqltext = sb.ToString();
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            var buffer = (byte[])reader[0];
                            var farray = Helper.ArrayToFloat(buffer);
                            centers.SetArray(farray);
                            break;
                        }
                    }
                }

                using (var matlab = new Mat())
                using (var matrgb = new Mat())
                using (var matpal = new Mat(16 * 64, 16 * 64, MatType.CV_8UC3)) {
                    centers.ConvertTo(matlab, MatType.CV_8U);
                    using (var c3 = matlab.Reshape(3, 256)) {
                        Cv2.CvtColor(c3, matrgb, ColorConversionCodes.Lab2BGR);
                        matrgb.GetArray<Vec3b>(out var pal);
                        for (var y = 0; y < 16; y++) {
                            for (var x = 0; x < 16; x++) {
                                var row = y * 16 + x;
                                Cv2.Rectangle(matpal, new Rect(x * 64, y * 64, 64, 64), new Scalar(pal[row].Item0, pal[row].Item1, pal[row].Item2), -1);
                            }
                        }

                        matpal.SaveImage("matpal.png");
                    }
                }
            }
        }
    }
}
