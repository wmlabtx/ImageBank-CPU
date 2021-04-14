using System;
using System.Data.SqlClient;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void LoadImgs(IProgress<string> progress)
        {
            progress.Report("Loading model...");

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            
            sb.Append($"{AppConsts.AttrId}, "); // 0
            sb.Append($"{AppConsts.AttrName}, "); // 1
            sb.Append($"{AppConsts.AttrFolder}, "); // 2
            sb.Append($"{AppConsts.AttrHash}, "); // 3

            sb.Append($"{AppConsts.AttrWidth}, "); // 4
            sb.Append($"{AppConsts.AttrHeight}, "); // 5
            sb.Append($"{AppConsts.AttrSize}, "); // 6

            sb.Append($"{AppConsts.AttrColorDescriptors}, "); // 7
            sb.Append($"{AppConsts.AttrColorDistance}, "); // 8
            sb.Append($"{AppConsts.AttrPerceptiveDescriptorsBlob}, "); // 9
            sb.Append($"{AppConsts.AttrPerceptiveDistance}, "); // 10
            sb.Append($"{AppConsts.AttrOrbDescriptorsBlob}, "); // 11
            sb.Append($"{AppConsts.AttrOrbDistance}, "); // 12

            sb.Append($"{AppConsts.AttrLastChanged}, "); // 13
            sb.Append($"{AppConsts.AttrLastView}, "); // 14
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 15
            sb.Append($"{AppConsts.AttrNextHash}, "); // 16

            sb.Append($"{AppConsts.AttrCounter}, "); // 17
            sb.Append($"{AppConsts.AttrLastId}, "); // 18
            
            sb.Append($"{AppConsts.AttrOrbKeyPointsBlob} "); // 19

            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var id = reader.GetInt32(0);
                            var name = reader.GetString(1);
                            var folder = reader.GetString(2);
                            var hash = reader.GetString(3);

                            var width = reader.GetInt32(4);
                            var height = reader.GetInt32(5);
                            var size = reader.GetInt32(6);

                            var colordescriptors = (byte[])reader[7];
                            var colordistance = reader.GetFloat(8);
                            var perceptivedescriptorsblob = (byte[])reader[9];
                            var perceptivedescriptors = ImageHelper.ArrayTo64(perceptivedescriptorsblob);
                            var perceptivedistance = reader.GetInt32(10);
                            var orbdescriptorsblob = (byte[])reader[11];
                            var orbdescriptors = ImageHelper.ArrayToMat(orbdescriptorsblob);
                            var orbkeypointsblob = (byte[])reader[19];
                            var orbkeypoints = ImageHelper.ArrayToKeyPoints(orbkeypointsblob);
                            var orbdistance = reader.GetFloat(12);

                            var lastchanged = reader.GetDateTime(13);
                            var lastview = reader.GetDateTime(14);
                            var lastcheck = reader.GetDateTime(15);
                            var nexthash = reader.GetString(16);

                            var counter = reader.GetInt32(17);
                            var lastid = reader.GetInt32(18);

                            var img = new Img(
                                id: id,
                                name: name,
                                folder: folder,
                                hash: hash,

                                width: width,
                                height: height,
                                size: size,

                                colordescriptors: colordescriptors,
                                colordistance: colordistance,
                                perceptivedescriptors: perceptivedescriptors,
                                perceptivedistance: perceptivedistance,
                                orbdescriptors: orbdescriptors,
                                orbkeypoints: orbkeypoints,
                                orbdistance: orbdistance,

                                lastchanged: lastchanged,
                                lastview: lastview,
                                lastcheck: lastcheck,
                                nexthash: nexthash,

                                lastid: lastid,
                                counter: counter
                               );

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                dtn = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }

                progress.Report("Loading vars...");

                _id = 0;

                sb.Length = 0;
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrId} "); // 0
                sb.Append($"FROM {AppConsts.TableVars}");
                sqltext = sb.ToString();
                lock (_sqllock) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection))
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    {
                        using (var reader = sqlCommand.ExecuteReader()) {
                            while (reader.Read()) {
                                _id = reader.GetInt32(0);
                                break;
                            }
                        }
                    }
                }
            }

            progress.Report("Database loaded");

            /*
            lock (_sqllock) {
                foreach (var img in _imgList.Select(e => e.Value).ToArray()) {
                    if (img.LastView < _viewnow) {
                        img.LastView = _viewnow;
                    }
                }
            }
            */
        }
    }
}