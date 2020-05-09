using System;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Load(IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            progress.Report("Loading model...");

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrId}, "); // 0
            sb.Append($"{AppConsts.AttrFolder}, "); // 1
            sb.Append($"{AppConsts.AttrLastView}, "); // 2
            sb.Append($"{AppConsts.AttrNextId}, "); // 3
            sb.Append($"{AppConsts.AttrDistance}, "); // 4
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 5
            sb.Append($"{AppConsts.AttrVector}, "); // 6
            sb.Append($"{AppConsts.AttrCounter}, "); // 7
            sb.Append($"{AppConsts.AttrLastModified} "); // 8
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dt = DateTime.Now;
                        while (reader.Read()) {
                            var id = reader.GetString(0);
                            var folder = reader.GetString(1);
                            var lastview = reader.GetDateTime(2);
                            var nextid = reader.GetString(3);
                            var distance = reader.GetFloat(4);
                            var lastcheck = reader.GetDateTime(5);
                            var vector = (byte[])reader[6];
                            var counter = reader.GetInt32(7);
                            var lastmodified = reader.GetDateTime(8);
                            var img = new Img(
                                id: id,
                                folder: folder,
                                lastview: lastview,
                                nextid: nextid,
                                distance: distance,
                                lastcheck: lastcheck,
                                vector: vector,
                                counter: counter,
                                lastmodified: lastmodified);

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                                dt = DateTime.Now;
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