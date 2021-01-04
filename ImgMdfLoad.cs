using System;
using System.Data.SqlClient;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Load(IProgress<string> progress)
        {
            progress.Report("Loading model...");

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrName}, "); // 0
            sb.Append($"{AppConsts.AttrFolder}, "); // 1
            sb.Append($"{AppConsts.AttrLastView}, "); // 2
            sb.Append($"{AppConsts.AttrWidth}, "); // 3
            sb.Append($"{AppConsts.AttrHeigth}, "); // 4
            sb.Append($"{AppConsts.AttrSize}, "); // 5
            sb.Append($"{AppConsts.AttrDescriptors}, "); // 6
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 7
            sb.Append($"{AppConsts.AttrLastAdded}, "); // 8
            sb.Append($"{AppConsts.AttrNextName}, "); // 9
            sb.Append($"{AppConsts.AttrSim}, "); // 10
            sb.Append($"{AppConsts.AttrFamily}, "); // 11
            sb.Append($"{AppConsts.AttrCounter}, "); // 12
            sb.Append($"{AppConsts.AttrHash} "); // 13
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var name = reader.GetString(0);
                            var folder = reader.GetInt32(1);
                            var lastview = reader.GetDateTime(2);
                            var width = reader.GetInt32(3);
                            var heigth = reader.GetInt32(4);
                            var size = reader.GetInt32(5);
                            var array = (byte[])reader[6];
                            var descriptors = ImageHelper.ArrayToDescriptors(array);
                            var lastcheck = reader.GetDateTime(7);
                            var lastadded = reader.GetDateTime(8);
                            var nextname = reader.GetString(9);
                            var sim = reader.GetFloat(10);
                            var family = reader.GetString(11);
                            var counter = reader.GetByte(12);
                            var hash = reader.GetString(13);
                            var img = new Img(
                                name: name,
                                hash: hash,
                                width: width,
                                heigth: heigth,
                                size: size,
                                descriptors: descriptors,
                                folder: folder,
                                lastview: lastview,
                                lastcheck: lastcheck,
                                lastadded: lastadded,
                                nextname: nextname,
                                sim: sim,
                                family: family,
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
            }

            progress.Report("Database loaded");
        }
    }
}