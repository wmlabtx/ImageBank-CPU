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
            sb.Append($"{AppConsts.AttrLastAdded}, "); // 4
            sb.Append($"{AppConsts.AttrLastView}, "); // 5
            sb.Append($"{AppConsts.AttrCounter}, "); // 6
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 7
            sb.Append($"{AppConsts.AttrNextHash}, "); // 8
            sb.Append($"{AppConsts.AttrDistance} "); // 9
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
                            var lastadded = reader.GetDateTime(4);
                            var lastview = reader.GetDateTime(5);
                            var counter = reader.GetByte(6);
                            var lastcheck = reader.GetDateTime(7);
                            var nexthash = reader.GetString(8);
                            var distance = reader.GetFloat(9);
                            var img = new Img(
                                name: name,
                                folder: folder,
                                hash: hash,
                                blob: blob,
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

        /*
        public static byte[] LoadBlob(string name)
        {
            byte[] blob = null;
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrDescriptors} ");
            sb.Append($"FROM {AppConsts.TableImages} ");
            sb.Append($"WHERE {AppConsts.AttrName} = @{AppConsts.AttrName}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", name);
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            blob = (byte[])reader[0];
                            break;
                        }
                    }
                }
            }

            return blob;
        }
        */
    }
}