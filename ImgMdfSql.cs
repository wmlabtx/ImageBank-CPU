using System.Text;
using System.Data.SqlClient;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlImagesUpdateProperty(int id, string key, object val)
        {
            lock (_sqllock) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttrId} = @{AppConsts.AttrId}";
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", id);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                }
            }
        }

        private static void SqlDelete(int id)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttrId} = @{AppConsts.AttrId}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", id);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlAdd(Img img)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                    sb.Append($"{AppConsts.AttrId}, ");
                    sb.Append($"{AppConsts.AttrName}, ");
                    sb.Append($"{AppConsts.AttrHash}, ");
                    sb.Append($"{AppConsts.AttrPHashEx}, ");
                    sb.Append($"{AppConsts.AttrVector}, ");
                    sb.Append($"{AppConsts.AttrYear}, ");
                    sb.Append($"{AppConsts.AttrHistory}, ");
                    sb.Append($"{AppConsts.AttrBestId}, ");
                    sb.Append($"{AppConsts.AttrBestPDistance}, ");
                    sb.Append($"{AppConsts.AttrBestVDistance}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrId}, ");
                    sb.Append($"@{AppConsts.AttrName}, ");
                    sb.Append($"@{AppConsts.AttrHash}, ");
                    sb.Append($"@{AppConsts.AttrPHashEx}, ");
                    sb.Append($"@{AppConsts.AttrVector}, ");
                    sb.Append($"@{AppConsts.AttrYear}, ");
                    sb.Append($"@{AppConsts.AttrHistory}, ");
                    sb.Append($"@{AppConsts.AttrBestId}, ");
                    sb.Append($"@{AppConsts.AttrBestPDistance}, ");
                    sb.Append($"@{AppConsts.AttrBestVDistance}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHash}", img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrPHashEx}", img.PHashEx.ToArray());
                    var array = Helper.ArrayFrom32(img.Vector);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrVector}", array);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrYear}", img.Year);
                    var historyarray = img.History.Select(e => e.Key).ToArray();
                    var historybuffer = new byte[img.History.Count * sizeof(int)];
                    Buffer.BlockCopy(historyarray, 0, historybuffer, 0, historybuffer.Length);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHistory}", historybuffer);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrBestId}", img.BestId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrBestPDistance}", img.BestPDistance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrBestVDistance}", img.BestVDistance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void LoadImgs(IProgress<string> progress)
        {
            lock (_imglock) {
                _imgList.Clear();

                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrId}, "); // 0
                sb.Append($"{AppConsts.AttrName}, "); // 1
                sb.Append($"{AppConsts.AttrHash}, "); // 2
                sb.Append($"{AppConsts.AttrPHashEx}, "); // 3
                sb.Append($"{AppConsts.AttrVector}, "); // 4
                sb.Append($"{AppConsts.AttrYear}, "); // 5
                sb.Append($"{AppConsts.AttrHistory}, "); // 6
                sb.Append($"{AppConsts.AttrBestId}, "); // 7
                sb.Append($"{AppConsts.AttrBestPDistance}, "); // 8
                sb.Append($"{AppConsts.AttrBestVDistance}, "); // 9
                sb.Append($"{AppConsts.AttrLastView}, "); // 10
                sb.Append($"{AppConsts.AttrLastCheck} "); // 11
                sb.Append($"FROM {AppConsts.TableImages}");
                var sqltext = sb.ToString();
                lock (_sqllock) {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = sqltext;
                        using (var reader = sqlCommand.ExecuteReader()) {
                            var dtn = DateTime.Now;
                            while (reader.Read()) {
                                var id = reader.GetInt32(0);
                                var name = reader.GetString(1);
                                var hash = reader.GetString(2);
                                var phashexbuffer = (byte[])reader[3];
                                var phashex = new PHashEx(phashexbuffer, 0);
                                var array = (byte[])reader[4];
                                var vector = Helper.ArrayTo32(array);
                                var year = reader.GetInt32(5);
                                var historybuffer = (byte[])reader[6];
                                var historyarray = new int[historybuffer.Length / sizeof(int)];
                                Buffer.BlockCopy(historybuffer, 0, historyarray, 0, historybuffer.Length);
                                var history = new SortedList<int, int>(historyarray.ToDictionary(e => e));
                                var bestid = reader.GetInt32(7);
                                var bestpdistance = reader.GetInt32(8);
                                var bestvdistance = reader.GetFloat(9);
                                var lastview = reader.GetDateTime(10);
                                var lastcheck = reader.GetDateTime(11);

                                var img = new Img(
                                    id: id,
                                    name: name,
                                    hash: hash,
                                    phashex: phashex,
                                    vector: vector,
                                    year: year,
                                    history: history, 
                                    bestid: bestid,
                                    bestpdistance: bestpdistance,
                                    bestvdistance: bestvdistance,
                                    lastview: lastview,
                                    lastcheck: lastcheck
                                   );

                                AddToMemory(img);

                                if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                    dtn = DateTime.Now;
                                    if (progress != null) {
                                        progress.Report($"Loading images ({_imgList.Count})...");
                                    }
                                }
                            }
                        }
                    }

                    if (progress != null) {
                        progress.Report("Loading vars...");
                    }

                    _id = 0;

                    sb.Length = 0;
                    sb.Append("SELECT ");
                    sb.Append($"{AppConsts.AttrId}, "); // 0
                    sb.Append($"{AppConsts.AttrImportLimit} "); // 1
                    sb.Append($"FROM {AppConsts.TableVars}");
                    sqltext = sb.ToString();
                    lock (_sqllock) {
                        using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                            using (var reader = sqlCommand.ExecuteReader()) {
                                while (reader.Read()) {
                                    _id = reader.GetInt32(0);
                                    _importLimit = reader.GetInt32(1);
                                    break;
                                }
                            }
                        }
                    }

                    if (progress != null) {
                        progress.Report("Database loaded");
                    }
                }
            }

            SiftHelper.LoadNodes();
        }

        public static void SqlUpdateVar(string key, object val)
        {
            lock (_sqllock) {
                var sqltext = $"UPDATE {AppConsts.TableVars} SET {key} = @{key}";
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}