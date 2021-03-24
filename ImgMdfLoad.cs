using System;
using System.Data.SqlClient;
using System.Linq;
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
            sb.Append($"{AppConsts.AttrHashes}, "); // 4
            sb.Append($"{AppConsts.AttrLastChanged}, "); // 5
            sb.Append($"{AppConsts.AttrLastView}, "); // 6
            sb.Append($"{AppConsts.AttrCounter}, "); // 7
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 8
            sb.Append($"{AppConsts.AttrNextHash}, "); // 9
            sb.Append($"{AppConsts.AttrDiff}, "); // 10
            sb.Append($"{AppConsts.AttrWidth}, "); // 11
            sb.Append($"{AppConsts.AttrHeight}, "); // 12
            sb.Append($"{AppConsts.AttrSize}, "); // 13
            sb.Append($"{AppConsts.AttrId}, "); // 14
            sb.Append($"{AppConsts.AttrLastId}, "); // 15
            sb.Append($"{AppConsts.AttrDistance} "); // 16

            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var name = reader.GetString(0);
                            var folder = reader.GetString(1);
                            var hash = reader.GetString(2);
                            var blob = (byte[])reader[3];
                            var pblob = (byte[])reader[4];
                            var lastchanged = reader.GetDateTime(5);
                            var lastview = reader.GetDateTime(6);
                            var counter = reader.GetInt32(7);
                            var lastcheck = reader.GetDateTime(8);
                            var nexthash = reader.GetString(9);
                            var diff = (byte[])reader[10];
                            var width = reader.GetInt32(11);
                            var height = reader.GetInt32(12);
                            var size = reader.GetInt32(13);
                            var id = reader.GetInt32(14);
                            var lastid = reader.GetInt32(15);
                            var distance = reader.GetInt32(16);
                            var img = new Img(
                                name: name,
                                folder: folder,
                                hash: hash,
                                blob: blob,
                                pblob: pblob,
                                lastchanged: lastchanged,
                                lastview: lastview,
                                counter: counter,
                                lastcheck: lastcheck,
                                nexthash: nexthash,
                                diff: diff,
                                width: width,
                                height: height,
                                size: size,
                                id: id,
                                lastid: lastid,
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

            lock (_sqllock) {
                foreach (var img in _imgList.Select(e => e.Value).ToArray()) {
                    if (img.LastView < _viewnow) {
                        img.LastView = _viewnow;
                    }
                }
            }
        }
    }
}