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
                    sb.Append($"{AppConsts.AttributePalette}, ");
                    sb.Append($"{AppConsts.AttributeSceneId}, ");
                    sb.Append($"{AppConsts.AttributeDistance}, ");
                    sb.Append($"{AppConsts.AttributeYear}, ");
                    sb.Append($"{AppConsts.AttributeBestId}, ");
                    sb.Append($"{AppConsts.AttributeLastView}, ");
                    sb.Append($"{AppConsts.AttributeLastCheck}, ");
                    sb.Append($"{AppConsts.AttributeHistory}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttributeId}, ");
                    sb.Append($"@{AppConsts.AttributeName}, ");
                    sb.Append($"@{AppConsts.AttributeHash}, ");
                    sb.Append($"@{AppConsts.AttributePalette}, ");
                    sb.Append($"@{AppConsts.AttributeSceneId}, ");
                    sb.Append($"@{AppConsts.AttributeDistance}, ");
                    sb.Append($"@{AppConsts.AttributeYear}, ");
                    sb.Append($"@{AppConsts.AttributeBestId}, ");
                    sb.Append($"@{AppConsts.AttributeLastView}, ");
                    sb.Append($"@{AppConsts.AttributeLastCheck}, ");
                    sb.Append($"@{AppConsts.AttributeHistory}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributePalette}", Helper.ArrayFromFloat(img.GetPalette()));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeSceneId}", img.SceneId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDistance}", img.Distance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeYear}", img.Year);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestId}", img.BestId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastCheck}", img.LastCheck);
                    var bufferhistory = Helper.ArrayFrom32(img.GetHistory());
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHistory}", bufferhistory);
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
                sb.Append($"{AppConsts.AttributePalette}, "); // 3
                sb.Append($"{AppConsts.AttributeSceneId}, "); // 4
                sb.Append($"{AppConsts.AttributeDistance}, "); // 5
                sb.Append($"{AppConsts.AttributeYear}, "); // 6
                sb.Append($"{AppConsts.AttributeBestId}, "); // 7
                sb.Append($"{AppConsts.AttributeLastView}, "); // 8
                sb.Append($"{AppConsts.AttributeLastCheck}, "); // 9
                sb.Append($"{AppConsts.AttributeHistory} "); // 10
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
                                var palette = Helper.ArrayToFloat((byte[])reader[3]);
                                var sceneid = reader.GetInt32(4);
                                var distance = reader.GetFloat(5);
                                var year = reader.GetInt32(6);
                                var bestid = reader.GetInt32(7);
                                var lastview = reader.GetDateTime(8);
                                var lastcheck = reader.GetDateTime(9);
                                var history = Helper.ArrayTo32((byte[])reader[10]);
                                var img = new Img(
                                    id: id,
                                    name: name,
                                    hash: hash,
                                    palette: palette,
                                    sceneid: sceneid,
                                    distance: distance,
                                    year: year,
                                    bestid: bestid,
                                    lastview: lastview,
                                    lastcheck: lastcheck,
                                    history: history
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
                    _sceneid = 0;

                    sb.Length = 0;
                    sb.Append("SELECT ");
                    sb.Append($"{AppConsts.AttributeId}, "); // 0
                    sb.Append($"{AppConsts.AttributeImportLimit}, "); // 1
                    sb.Append($"{AppConsts.AttributeLabCenters}, "); // 2
                    sb.Append($"{AppConsts.AttributeSceneId} "); // 2
                    sb.Append($"FROM {AppConsts.TableVars}");
                    sqltext = sb.ToString();
                    lock (_sqllock) {
                        using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                            using (var reader = sqlCommand.ExecuteReader()) {
                                while (reader.Read()) {
                                    _id = reader.GetInt32(0);
                                    _importLimit = reader.GetInt32(1);                                    
                                    var buffer = (byte[])reader[2];
                                    _palette = Helper.ArrayToFloat(buffer);
                                    _sceneid = reader.GetInt32(3);
                                    break;
                                }
                            }
                        }
                    }

                    progress?.Report("Database loaded");
                }
            }
        }

        public static void LoadPalette()
        {
            lock (_imglock) {
                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttributeLabCenters} "); // 0
                sb.Append($"FROM {AppConsts.TableVars}");
                var sqltext = sb.ToString();
                lock (_sqllock) {
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                        using (var reader = sqlCommand.ExecuteReader()) {
                            while (reader.Read()) {
                                var buffer = (byte[])reader[0];
                                _palette = Helper.ArrayToFloat(buffer);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void SavePalette()
        {
            var buffer = Helper.ArrayFromFloat(_palette);
            SqlVarsUpdateProperty(AppConsts.AttributeLabCenters, buffer);
        }
    }
}