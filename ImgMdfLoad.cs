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
            /*
            if (!HelperMl.Init()) {
                throw new Exception();
            }
            */

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrId}, "); // 0
            sb.Append($"{AppConsts.AttrChecksum}, "); // 1
            sb.Append($"{AppConsts.AttrFamily}, "); // 2
            sb.Append($"{AppConsts.AttrLastView}, "); // 3
            sb.Append($"{AppConsts.AttrNextId}, "); // 4
            sb.Append($"{AppConsts.AttrDistance}, "); // 5
            sb.Append($"{AppConsts.AttrLastId}, "); // 6
            sb.Append($"{AppConsts.AttrVector}, "); // 7
            sb.Append($"{AppConsts.AttrHistory} "); // 8
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dt = DateTime.Now;
                        while (reader.Read()) {
                            var id = reader.GetInt32(0);
                            var checksum = reader.GetString(1);
                            var family = reader.GetInt32(2);
                            var lastview = reader.GetDateTime(3);
                            var nextid = reader.GetInt32(4);
                            var distance = reader.GetFloat(5);
                            var lastid = reader.GetInt32(6);
                            var vbuffer = (byte[])reader[7];
                            var vector = Helper.ConvertBufferToMat(vbuffer);
                            var history = (byte[])reader[8];
                            var img = new Img(
                                id: id,
                                checksum: checksum,
                                family: family,
                                lastview: lastview,
                                nextid: nextid,
                                distance: distance,
                                lastid: lastid,
                                vector: vector,
                                history : history);

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                                dt = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }
            }

            progress.Report("Loading vars...");

            _id = 0;
            _family = 0;

            sb.Length = 0;
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrId}, "); // 0
            sb.Append($"{AppConsts.AttrFamily} "); // 1
            sb.Append($"FROM {AppConsts.TableVars}");
            sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            _id = reader.GetInt32(0);
                            _family = reader.GetInt32(1);
                            break;
                        }
                    }
                }
            }
            
            progress.Report("Database loaded");
        }
    }
}