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
            if (!HelperMl.Init()) {
                throw new Exception();
            }

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrId}, "); // 0
            sb.Append($"{AppConsts.AttrName}, "); // 1
            sb.Append($"{AppConsts.AttrChecksum}, "); // 2
            sb.Append($"{AppConsts.AttrGeneration}, "); // 3
            sb.Append($"{AppConsts.AttrLastView}, "); // 4
            sb.Append($"{AppConsts.AttrNextId}, "); // 5
            sb.Append($"{AppConsts.AttrDistance}, "); // 6
            sb.Append($"{AppConsts.AttrLastId}, "); // 7
            sb.Append($"{AppConsts.AttrLastChange}, "); // 8
            sb.Append($"{AppConsts.AttrLastFind}, "); // 9
            sb.Append($"{AppConsts.AttrVector} "); // 10
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dt = DateTime.Now;
                        while (reader.Read()) {
                            var id = reader.GetInt32(0);
                            var name = reader.GetString(1);
                            var checksum = reader.GetString(2);
                            var generation = reader.GetInt32(3);
                            var lastview = reader.GetDateTime(4);
                            var nextid = reader.GetInt32(5);
                            var distance = reader.GetFloat(6);
                            var lastid = reader.GetInt32(7);
                            var lastchange = reader.GetDateTime(8);
                            var lastfind = reader.GetDateTime(9);
                            var orbvbuffer = (byte[])reader[10];
                            var vector = Helper.BufferToVector(orbvbuffer);
                            var img = new Img(
                                id: id,
                                name: name,
                                checksum: checksum,
                                generation: generation,
                                lastview: lastview,
                                nextid: nextid,
                                distance: distance,
                                lastid: lastid,
                                lastchange: lastchange,
                                lastfind: lastfind,
                                vector: vector);

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

            sb.Length = 0;
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrId} "); // 0
            sb.Append($"FROM {AppConsts.TableVars}");
            sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            _id = reader.GetInt32(0);
                            break;
                        }
                    }
                }
            }
            
            progress.Report("Database loaded");
        }
    }
}