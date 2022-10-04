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
                    sb.Append($"{AppConsts.AttributeVector}, ");
                    sb.Append($"{AppConsts.AttributeDistance}, ");
                    sb.Append($"{AppConsts.AttributeYear}, ");
                    sb.Append($"{AppConsts.AttributeBestId}, ");
                    sb.Append($"{AppConsts.AttributeLastView}, ");
                    sb.Append($"{AppConsts.AttributeNi}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttributeId}, ");
                    sb.Append($"@{AppConsts.AttributeName}, ");
                    sb.Append($"@{AppConsts.AttributeHash}, ");
                    sb.Append($"@{AppConsts.AttributePalette}, ");
                    sb.Append($"@{AppConsts.AttributeVector}, ");
                    sb.Append($"@{AppConsts.AttributeDistance}, ");
                    sb.Append($"@{AppConsts.AttributeYear}, ");
                    sb.Append($"@{AppConsts.AttributeBestId}, ");
                    sb.Append($"@{AppConsts.AttributeLastView}, ");
                    sb.Append($"@{AppConsts.AttributeNi}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributePalette}", Helper.ArrayFromFloat(img.GetPalette()));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeVector}", Helper.ArrayFromFloat(img.GetVector()));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDistance}", img.Distance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeYear}", img.Year);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestId}", img.BestId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeNi}", Helper.ArrayFrom32(img.GetHistory()));
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void LoadImages(IProgress<string> progress)
        {
            _imgList.Clear();
            _nameList.Clear();
            _hashList.Clear();
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributeId}, "); // 0
            sb.Append($"{AppConsts.AttributeName}, "); // 1
            sb.Append($"{AppConsts.AttributeHash}, "); // 2
            sb.Append($"{AppConsts.AttributePalette}, "); // 3
            sb.Append($"{AppConsts.AttributeDistance}, "); // 4
            sb.Append($"{AppConsts.AttributeYear}, "); // 5
            sb.Append($"{AppConsts.AttributeBestId}, "); // 6
            sb.Append($"{AppConsts.AttributeLastView}, "); // 7
            sb.Append($"{AppConsts.AttributeNi}, "); // 8
            sb.Append($"{AppConsts.AttributeVector} "); // 9
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
                            var distance = reader.GetFloat(4);
                            var year = reader.GetInt32(5);
                            var bestid = reader.GetInt32(6);
                            var lastview = reader.GetDateTime(7);
                            var ni = Helper.ArrayTo32((byte[])reader[8]);
                            var vector = Helper.ArrayToFloat((byte[])reader[9]);
                            var img = new Img(
                                id: id,
                                name: name,
                                hash: hash,
                                palette: palette,
                                vector: vector,
                                distance: distance,
                                year: year,
                                bestid: bestid,
                                lastview: lastview,
                                ni: ni
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
                _importLimit = _imgList.Count;
                _palette = null;
                sb.Length = 0;
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttributeId}, "); // 0
                sb.Append($"{AppConsts.AttributeImportLimit}, "); // 1
                sb.Append($"{AppConsts.AttributeLabCenters} "); // 2
                sb.Append($"FROM {AppConsts.TableVars}");
                sqltext = sb.ToString();
                lock (_sqllock) {
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                        using (var reader = sqlCommand.ExecuteReader()) {
                            while (reader.Read()) {
                                _id = reader.GetInt32(0);
                                _importLimit = reader.GetInt32(1);                                    
                                _palette = Helper.ArrayToFloat((byte[])reader[2]);
                                break;
                            }
                        }
                    }
                }

                progress?.Report("Database loaded");

                /*
                foreach (var img in _imgList) {
                    if (img.Value.LastView.Year == 2020) {
                        var seconds = _random.Next(0, 60 * 60 * 24 * 364);
                        var lastview = new DateTime(2020, 1, 1).AddSeconds(seconds);
                        img.Value.SetLastView(lastview);
                    }
                }
                */
            }
        }

        public static void SavePalette()
        {
            var buffer = Helper.ArrayFromFloat(_palette);
            SqlVarsUpdateProperty(AppConsts.AttributeLabCenters, buffer);
        }
    }
}