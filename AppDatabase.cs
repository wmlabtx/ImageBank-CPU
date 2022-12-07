using System;
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

        public static void ImageUpdateProperty(string hash, string key, object val)
        {
            try {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttributeHash} = @{AppConsts.AttributeHash}";
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", hash);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (SqlException) {
            }
        }

        public static void DeleteImage(string hash)
        {
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttributeHash} = @{AppConsts.AttributeHash}";
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", hash);
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void AddImage(Img img)
        {
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                sb.Append($"{AppConsts.AttributeName}, ");
                sb.Append($"{AppConsts.AttributeHash}, ");
                sb.Append($"{AppConsts.AttributeCounter}, ");
                sb.Append($"{AppConsts.AttributeLastView}, ");
                sb.Append($"{AppConsts.AttributeBestHash}, ");
                sb.Append($"{AppConsts.AttributeDistance}, ");
                sb.Append($"{AppConsts.AttributeLastCheck}, ");
                sb.Append($"{AppConsts.AttributeOrientation}, ");
                sb.Append($"{AppConsts.AttributeVector}");
                sb.Append(") VALUES (");
                sb.Append($"@{AppConsts.AttributeName}, ");
                sb.Append($"@{AppConsts.AttributeHash}, ");
                sb.Append($"@{AppConsts.AttributeCounter}, ");
                sb.Append($"@{AppConsts.AttributeLastView}, ");
                sb.Append($"@{AppConsts.AttributeBestHash}, ");
                sb.Append($"@{AppConsts.AttributeDistance}, ");
                sb.Append($"@{AppConsts.AttributeLastCheck}, ");
                sb.Append($"@{AppConsts.AttributeOrientation}, ");
                sb.Append($"@{AppConsts.AttributeVector}");
                sb.Append(')');
                sqlCommand.CommandText = sb.ToString();
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeCounter}", img.Counter);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestHash}", img.BestHash);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDistance}", img.Distance);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastCheck}", img.LastCheck);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeOrientation}", Helper.RotateFlipTypeToByte(img.Orientation));
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeVector}", img.GetVector());
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void LoadImages(IProgress<string> progress)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributeName}, "); // 0
            sb.Append($"{AppConsts.AttributeHash}, "); // 1
            sb.Append($"{AppConsts.AttributeCounter}, "); // 2
            sb.Append($"{AppConsts.AttributeLastView}, "); // 3
            sb.Append($"{AppConsts.AttributeBestHash}, "); // 4
            sb.Append($"{AppConsts.AttributeDistance}, "); // 5
            sb.Append($"{AppConsts.AttributeLastCheck}, "); // 6
            sb.Append($"{AppConsts.AttributeOrientation}, "); // 7
            sb.Append($"{AppConsts.AttributeVector} "); // 8
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandText = sqltext;
                using (var reader = sqlCommand.ExecuteReader()) {
                    var dtn = DateTime.Now;
                    while (reader.Read()) {
                        var name = reader.GetString(0);
                        var hash = reader.GetString(1);
                        var counter = reader.GetInt32(2);
                        var lastview = reader.GetDateTime(3);
                        var besthash = reader.GetString(4);
                        var distance = reader.GetFloat(5);
                        var lastcheck = reader.GetDateTime(6);
                        var orientation = Helper.ByteToRotateFlipType(reader.GetByte(7));
                        var vector = (byte[])reader[8];
                        var img = new Img(
                            name: name,
                            hash: hash,
                            counter: counter,
                            lastview: lastview,
                            besthash: besthash,
                            distance: distance,
                            lastcheck: lastcheck,
                            orientation: orientation,
                            vector: vector
                            );

                        AppImgs.Add(img);

                        if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                            dtn = DateTime.Now;
                            var count = AppImgs.Count();
                            progress?.Report($"Loading images ({count}){AppConsts.CharEllipsis}");
                        }
                    }
                }
            }

            progress?.Report("Database loaded");
        }
    }
}
