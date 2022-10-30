using System;
using System.Data.SqlClient;
using System.Text;

namespace ImageBank
{
    public static class AppDatabase
    {
        private static readonly object _sqllock = new object();
        private static readonly SqlConnection _sqlConnection;

        static AppDatabase()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=300";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public static void ImageUpdateProperty(int id, string key, object val)
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

        public static void VarsUpdateProperty(string key, object val)
        {
            lock (_sqllock) {
                var sqltext = $"UPDATE {AppConsts.TableVars} SET {key} = @{key}";
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteImage(int id)
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

        public static void DeletePair(int id)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"DELETE FROM {AppConsts.TablePairs} WHERE {AppConsts.AttributeIdX} = @{AppConsts.AttributeIdX} OR {AppConsts.AttributeIdY} = @{AppConsts.AttributeIdY}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeIdX}", id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeIdY}", id);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void AddImage(Img img)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                    sb.Append($"{AppConsts.AttributeId}, ");
                    sb.Append($"{AppConsts.AttributeName}, ");
                    sb.Append($"{AppConsts.AttributeHash}, ");
                    sb.Append($"{AppConsts.AttributeVector}, ");
                    sb.Append($"{AppConsts.AttributeDistance}, ");
                    sb.Append($"{AppConsts.AttributeYear}, ");
                    sb.Append($"{AppConsts.AttributeBestId}, ");
                    sb.Append($"{AppConsts.AttributeLastView}, ");
                    sb.Append($"{AppConsts.AttributeLastCheck}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttributeId}, ");
                    sb.Append($"@{AppConsts.AttributeName}, ");
                    sb.Append($"@{AppConsts.AttributeHash}, ");
                    sb.Append($"@{AppConsts.AttributeVector}, ");
                    sb.Append($"@{AppConsts.AttributeDistance}, ");
                    sb.Append($"@{AppConsts.AttributeYear}, ");
                    sb.Append($"@{AppConsts.AttributeBestId}, ");
                    sb.Append($"@{AppConsts.AttributeLastView}, ");
                    sb.Append($"@{AppConsts.AttributeLastCheck}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeVector}", Helper.ArrayFromFloat(img.GetVector()));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDistance}", img.Distance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeYear}", img.Year);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestId}", img.BestId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastCheck}", img.LastCheck);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void AddPair(int idx, int idy, float distance)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TablePairs} (");
                    sb.Append($"{AppConsts.AttributeIdX}, ");
                    sb.Append($"{AppConsts.AttributeIdY}, ");
                    sb.Append($"{AppConsts.AttributeDistance}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttributeIdX}, ");
                    sb.Append($"@{AppConsts.AttributeIdY}, ");
                    sb.Append($"@{AppConsts.AttributeDistance}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeIdX}", idx);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeIdY}", idy);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDistance}", distance);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void LoadImages(IProgress<string> progress)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributeId}, "); // 0
            sb.Append($"{AppConsts.AttributeName}, "); // 1
            sb.Append($"{AppConsts.AttributeHash}, "); // 2
            sb.Append($"{AppConsts.AttributeDistance}, "); // 3
            sb.Append($"{AppConsts.AttributeYear}, "); // 4
            sb.Append($"{AppConsts.AttributeBestId}, "); // 5
            sb.Append($"{AppConsts.AttributeLastView}, "); // 6
            sb.Append($"{AppConsts.AttributeLastCheck}, "); // 7
            sb.Append($"{AppConsts.AttributeVector} "); // 8
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
                            var distance = reader.GetFloat(3);
                            var year = reader.GetInt32(4);
                            var bestid = reader.GetInt32(5);
                            var lastview = reader.GetDateTime(6);
                            var lastcheck = reader.GetDateTime(7);
                            var vector = Helper.ArrayToFloat((byte[])reader[8]);
                            var img = new Img(
                                id: id,
                                name: name,
                                hash: hash,
                                vector: vector,
                                distance: distance,
                                year: year,
                                bestid: bestid,
                                lastview: lastview,
                                lastcheck: lastcheck
                                );

                            AppImgs.Add(img);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                dtn = DateTime.Now;
                                var count = AppImgs.Count();
                                progress?.Report($"Loading images ({count})...");
                            }
                        }
                    }
                }

                progress?.Report("Loading vars...");

                sb.Length = 0;
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttributeId} "); // 0
                sb.Append($"FROM {AppConsts.TableVars}");
                sqltext = sb.ToString();
                lock (_sqllock) {
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                        using (var reader = sqlCommand.ExecuteReader()) {
                            while (reader.Read()) {
                                var id = reader.GetInt32(0);
                                AppVars.SetId(id);
                                break;
                            }
                        }
                    }
                }

                progress?.Report("Loading pairs...");

                sb.Length = 0;
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttributeIdX}, "); // 0
                sb.Append($"{AppConsts.AttributeIdY}, "); // 1
                sb.Append($"{AppConsts.AttributeDistance} "); // 2
                sb.Append($"FROM {AppConsts.TablePairs}");
                sqltext = sb.ToString();
                lock (_sqllock) {
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                        using (var reader = sqlCommand.ExecuteReader()) {
                            var dtn = DateTime.Now;
                            while (reader.Read()) {
                                var idx = reader.GetInt32(0);
                                var idy = reader.GetInt32(1);
                                var distance = reader.GetFloat(2);
                                AppImgs.AddPair(idx, idy, distance, false);
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
    }
}
