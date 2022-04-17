using System.Text;
using System.Data.SqlClient;
using System;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void SqlImagesUpdateProperty(int id, string key, object val)
        {
            lock (_sqllock) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttributeId} = @{AppConsts.AttributeId}";
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", id);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                }
            }
        }

        public static void SqlVarsUpdateProperty(string key, object val)
        {
            lock (_sqllock) {
                var sqltext = $"UPDATE {AppConsts.TableVars} SET {key} = @{key}";
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlDeleteImage(int id)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttributeId} = @{AppConsts.AttributeId}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", id);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlAddImage(Img img)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                    sb.Append($"{AppConsts.AttributeId}, ");
                    sb.Append($"{AppConsts.AttributeName}, ");
                    sb.Append($"{AppConsts.AttributeHash}, ");
                    sb.Append($"{AppConsts.AttributeFp}, ");
                    sb.Append($"{AppConsts.AttributeFpFlip}, ");
                    sb.Append($"{AppConsts.AttributeYear}, ");
                    sb.Append($"{AppConsts.AttributeCounter}, ");
                    sb.Append($"{AppConsts.AttributeBestId}, ");
                    sb.Append($"{AppConsts.AttributeBestVDistance}, ");
                    sb.Append($"{AppConsts.AttributeLastView}, ");
                    sb.Append($"{AppConsts.AttributeLastCheck}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttributeId}, ");
                    sb.Append($"@{AppConsts.AttributeName}, ");
                    sb.Append($"@{AppConsts.AttributeHash}, ");
                    sb.Append($"@{AppConsts.AttributeFp}, ");
                    sb.Append($"@{AppConsts.AttributeFpFlip}, ");
                    sb.Append($"@{AppConsts.AttributeYear}, ");
                    sb.Append($"@{AppConsts.AttributeCounter}, ");
                    sb.Append($"@{AppConsts.AttributeBestId}, ");
                    sb.Append($"@{AppConsts.AttributeBestVDistance}, ");
                    sb.Append($"@{AppConsts.AttributeLastView}, ");
                    sb.Append($"@{AppConsts.AttributeLastCheck}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                    var buffer = Helper.ArrayFrom64(img.Fingerprints[0]);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeFp}", buffer);
                    var bufferflip = Helper.ArrayFrom64(img.Fingerprints[1]);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeFpFlip}", bufferflip);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeYear}", img.Year);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeCounter}", img.Counter);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestId}", img.BestId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestVDistance}", img.BestVDistance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastCheck}", img.LastCheck);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void LoadImages(IProgress<string> progress)
        {
            lock (_imglock) {
                _imgList.Clear();

                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttributeId}, "); // 0
                sb.Append($"{AppConsts.AttributeName}, "); // 1
                sb.Append($"{AppConsts.AttributeHash}, "); // 2
                sb.Append($"{AppConsts.AttributeFp}, "); // 3
                sb.Append($"{AppConsts.AttributeFpFlip}, "); // 4
                sb.Append($"{AppConsts.AttributeYear}, "); // 5
                sb.Append($"{AppConsts.AttributeCounter}, "); // 6
                sb.Append($"{AppConsts.AttributeBestId}, "); // 7
                sb.Append($"{AppConsts.AttributeBestVDistance}, "); // 8
                sb.Append($"{AppConsts.AttributeLastView}, "); // 9
                sb.Append($"{AppConsts.AttributeLastCheck} "); // 10
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
                                var fingerprints = new ulong[2][];
                                fingerprints[0] = Helper.ArrayTo64((byte[])reader[3]);
                                fingerprints[1] = Helper.ArrayTo64((byte[])reader[4]);
                                var year = reader.GetInt32(5);
                                var counter = reader.GetInt32(6);
                                var bestid = reader.GetInt32(7);
                                var bestvdistance = reader.GetFloat(8);
                                var lastview = reader.GetDateTime(9);
                                var lastcheck = reader.GetDateTime(10);

                                var img = new Img(
                                    id: id,
                                    name: name,
                                    hash: hash,
                                    fingerprints: fingerprints,
                                    year: year,
                                    counter: counter, 
                                    bestid: bestid,
                                    bestvdistance: bestvdistance,
                                    lastview: lastview,
                                    lastcheck: lastcheck
                                   );

                                AddToMemory(img);

                                if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                    dtn = DateTime.Now;
                                    progress?.Report($"Loading images ({_imgList.Count})...");
                                }
                            }
                        }
                    }

                    progress?.Report("Loading vars...");

                    _id = 0;

                    sb.Length = 0;
                    sb.Append("SELECT ");
                    sb.Append($"{AppConsts.AttributeId}, "); // 0
                    sb.Append($"{AppConsts.AttributeImportLimit} "); // 1
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
        }
    }
}