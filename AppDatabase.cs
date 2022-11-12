﻿using System;
using System.Data.SqlClient;
using System.Text;

namespace ImageBank
{
    public static class AppDatabase
    {
        private static readonly SqlConnection _sqlConnection;

        static AppDatabase()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=300";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public static void ImageUpdateProperty(int id, string key, object val)
        {
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

        public static void ClusterUpdateProperty(int id, string key, object val)
        {
            try {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"UPDATE {AppConsts.TableClusters} SET {key} = @{key} WHERE {AppConsts.AttributeId} = @{AppConsts.AttributeId}";
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", id);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (SqlException) {
            }
        }

        public static void ImageSetVector(int id, float[] vector)
        {
            var buffer = Helper.ArrayFromFloat(vector);
            ImageUpdateProperty(id, AppConsts.AttributeVector, buffer);
        }

        public static float[] ImageGetVector(int id)
        {
            float[] vector = null;
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributeVector} "); // 0
            sb.Append($"FROM {AppConsts.TableImages} ");
            sb.Append($"WHERE {AppConsts.AttributeId} = @{AppConsts.AttributeId}");
            var sqltext = sb.ToString();
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandText = sqltext;
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", id);
                using (var reader = sqlCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        vector = Helper.ArrayToFloat((byte[])reader[0]);
                        break;
                    }
                }
            }

            return vector;
        }

        public static void VarsUpdateProperty(string key, object val)
        {
            var sqltext = $"UPDATE {AppConsts.TableVars} SET {key} = @{key}";
            using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                sqlCommand.Parameters.AddWithValue($"@{key}", val);
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void DeleteImage(int id)
        {
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttributeId} = @{AppConsts.AttributeId}";
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", id);
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void DeleteCluster(int id)
        {
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableClusters} WHERE {AppConsts.AttributeId} = @{AppConsts.AttributeId}";
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", id);
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void AddImage(Img img, float[] vector)
        {
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
                sb.Append($"{AppConsts.AttributeLastCheck}, ");
                sb.Append($"{AppConsts.AttributeClusterId}");
                sb.Append(") VALUES (");
                sb.Append($"@{AppConsts.AttributeId}, ");
                sb.Append($"@{AppConsts.AttributeName}, ");
                sb.Append($"@{AppConsts.AttributeHash}, ");
                sb.Append($"@{AppConsts.AttributeVector}, ");
                sb.Append($"@{AppConsts.AttributeDistance}, ");
                sb.Append($"@{AppConsts.AttributeYear}, ");
                sb.Append($"@{AppConsts.AttributeBestId}, ");
                sb.Append($"@{AppConsts.AttributeLastView}, ");
                sb.Append($"@{AppConsts.AttributeLastCheck}, ");
                sb.Append($"@{AppConsts.AttributeClusterId}");
                sb.Append(')');
                sqlCommand.CommandText = sb.ToString();
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", img.Id);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeVector}", Helper.ArrayFromFloat(vector));
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDistance}", img.Distance);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeYear}", img.Year);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestId}", img.BestId);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastCheck}", img.LastCheck);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeClusterId}", img.ClusterId);
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void AddCluster(Cluster cluster)
        {
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO {AppConsts.TableClusters} (");
                sb.Append($"{AppConsts.AttributeId}, ");
                sb.Append($"{AppConsts.AttributeCounter}, ");
                sb.Append($"{AppConsts.AttributeAge}, ");
                sb.Append($"{AppConsts.AttributeVector}");
                sb.Append(") VALUES (");
                sb.Append($"@{AppConsts.AttributeId}, ");
                sb.Append($"@{AppConsts.AttributeCounter}, ");
                sb.Append($"@{AppConsts.AttributeAge}, ");
                sb.Append($"@{AppConsts.AttributeVector}");
                sb.Append(')');
                sqlCommand.CommandText = sb.ToString();
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", cluster.Id);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeCounter}", cluster.Counter);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeAge}", cluster.Age);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeVector}", Helper.ArrayFromFloat(cluster.GetVector()));
                sqlCommand.ExecuteNonQuery();
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
            sb.Append($"{AppConsts.AttributeClusterId} "); // 8
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
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
                        var clusterid = reader.GetInt32(8);
                        var img = new Img(
                            id: id,
                            name: name,
                            hash: hash,
                            distance: distance,
                            year: year,
                            bestid: bestid,
                            lastview: lastview,
                            lastcheck: lastcheck,
                            clusterid: clusterid
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
            using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                using (var reader = sqlCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        var id = reader.GetInt32(0);
                        AppVars.SetId(id);
                        break;
                    }
                }
            }

            progress?.Report("Loading clusters...");

            sb.Length = 0;
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributeId}, "); // 0
            sb.Append($"{AppConsts.AttributeCounter}, "); // 1
            sb.Append($"{AppConsts.AttributeAge}, "); // 2
            sb.Append($"{AppConsts.AttributeVector} "); // 3
            sb.Append($"FROM {AppConsts.TableClusters}");
            sqltext = sb.ToString();
            using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                using (var reader = sqlCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        var id = reader.GetInt32(0);
                        var counter = reader.GetInt32(1);
                        var age = reader.GetInt32(2);
                        var vector = Helper.ArrayToFloat((byte[])reader[3]);
                        var cluster = new Cluster(id, counter, age, vector);
                        AppClusters.Add(cluster);
                    }
                }
            }

            progress?.Report("Database loaded");
        }
    }
}
