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
            sb.Append($"{AppConsts.AttrName}, "); // 0
            sb.Append($"{AppConsts.AttrFolder}, "); // 1
            sb.Append($"{AppConsts.AttrHash}, "); // 2
            sb.Append($"{AppConsts.AttrDescriptors}, "); // 3
            sb.Append($"{AppConsts.AttrMapDescriptors}, "); // 4
            sb.Append($"{AppConsts.AttrPhash}, "); // 5
            sb.Append($"{AppConsts.AttrLastAdded}, "); // 6
            sb.Append($"{AppConsts.AttrLastView}, "); // 7
            sb.Append($"{AppConsts.AttrCounter}, "); // 8
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 9
            sb.Append($"{AppConsts.AttrNextHash}, "); // 10
            sb.Append($"{AppConsts.AttrDistance} "); // 11
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var name = reader.GetString(0);
                            var folder = reader.GetString(1);
                            var hash = reader.GetString(2);
                            var blob = (byte[])reader[3];
                            var mapdescriptors = (byte[])reader[4];
                            var phashbuffer = (byte[])reader[5];
                            var phash = BitConverter.ToUInt64(phashbuffer, 0);
                            var lastadded = reader.GetDateTime(6);
                            var lastview = reader.GetDateTime(7);
                            var counter = reader.GetInt32(8);
                            var lastcheck = reader.GetDateTime(9);
                            var nexthash = reader.GetString(10);
                            var distance = reader.GetFloat(11);
                            var img = new Img(
                                name: name,
                                folder: folder,
                                hash: hash,
                                blob: blob,
                                mapdescriptors: mapdescriptors,
                                phash: phash,
                                lastadded: lastadded,
                                lastview: lastview,
                                counter: counter,
                                lastcheck: lastcheck,
                                nexthash: nexthash,
                                distance: distance
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