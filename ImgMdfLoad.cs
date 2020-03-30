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
            sb.Append($"{AppConsts.AttrChecksum}, "); // 1
            sb.Append($"{AppConsts.AttrLastView}, "); // 2
            sb.Append($"{AppConsts.AttrNextId}, "); // 3
            sb.Append($"{AppConsts.AttrSim}, "); // 4
            sb.Append($"{AppConsts.AttrLastId}, "); // 5
            sb.Append($"{AppConsts.AttrVector}, "); // 6
            sb.Append($"{AppConsts.AttrCounter}, "); // 7
            sb.Append($"{AppConsts.AttrFormat} "); // 8
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dt = DateTime.Now;
                        while (reader.Read()) {
                            var id = reader.GetInt32(0);
                            var checksum = reader.GetString(1);
                            var lastview = reader.GetDateTime(2);
                            var nextid = reader.GetInt32(3);
                            var sim = reader.GetFloat(4);
                            var lastid = reader.GetInt32(5);
                            var buffer = (byte[])reader[6];
                            var vector = new ulong[buffer.Length / sizeof(ulong)];
                            Buffer.BlockCopy(buffer, 0, vector, 0, buffer.Length);
                            var counter = reader.GetInt32(7);
                            var format = reader.GetInt32(8);
                            var img = new Img(
                                id: id,
                                checksum: checksum,
                                lastview: lastview,
                                nextid: nextid,
                                sim: sim,
                                lastid: lastid,
                                vector: vector,
                                format: (MagicFormat)format,
                                counter: counter);

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

            /*
            lock (_imglock) {
                var list = _imgList.Select(e => e.Value).ToArray();
                foreach (var img in list) { 
                if (img.Format == 237) {
                        Delete(img.Id);
                    }
                }
            }
            */
        }
    }
}