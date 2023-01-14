using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace ImageBank
{
    public static class AppDatabase
    {
        private static readonly SqlConnection _sqlConnection;
        private static readonly object _sqllock = new object();

        static AppDatabase()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=300";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public static void ImageUpdateProperty(string hash, string key, object val)
        {
            if (Monitor.TryEnter(_sqllock, AppConsts.LockTimeout)) {
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
                    throw;
                }
                finally { 
                    Monitor.Exit(_sqllock); 
                }
            }
            else {
                throw new Exception();
            }
        }

        public static void VarsUpdateProperty(string key, object val)
        {
            if (Monitor.TryEnter(_sqllock, AppConsts.LockTimeout)) {
                try {
                    var sqltext = $"UPDATE {AppConsts.TableVars} SET {key} = @{key}";
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                    throw;
                }
                finally {
                    Monitor.Exit(_sqllock);
                }
            }
            else {
                throw new Exception();
            }
        }

        public static void DeleteImage(string hash)
        {
            if (Monitor.TryEnter(_sqllock, AppConsts.LockTimeout)) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttributeHash} = @{AppConsts.AttributeHash}";
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", hash);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                    throw;
                }
                finally {
                    Monitor.Exit(_sqllock);
                }
            }
            else {
                throw new Exception();
            }
        }

        public static void AddImage(Img img)
        {
            if (Monitor.TryEnter(_sqllock, AppConsts.LockTimeout)) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        var sb = new StringBuilder();
                        sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                        sb.Append($"{AppConsts.AttributeName}, ");
                        sb.Append($"{AppConsts.AttributeHash}, ");
                        sb.Append($"{AppConsts.AttributeDateTaken}, ");
                        sb.Append($"{AppConsts.AttributeLastView}, ");
                        sb.Append($"{AppConsts.AttributeOrientation}, ");
                        sb.Append($"{AppConsts.AttributeHistogram}, ");
                        sb.Append($"{AppConsts.AttributeVector}, ");
                        sb.Append($"{AppConsts.AttributeFamily}");
                        sb.Append(") VALUES (");
                        sb.Append($"@{AppConsts.AttributeName}, ");
                        sb.Append($"@{AppConsts.AttributeHash}, ");
                        sb.Append($"@{AppConsts.AttributeDateTaken}, ");
                        sb.Append($"@{AppConsts.AttributeLastView}, ");
                        sb.Append($"@{AppConsts.AttributeOrientation}, ");
                        sb.Append($"@{AppConsts.AttributeHistogram}, ");
                        sb.Append($"@{AppConsts.AttributeVector}, ");
                        sb.Append($"@{AppConsts.AttributeFamily}");
                        sb.Append(')');
                        sqlCommand.CommandText = sb.ToString();
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDateTaken}", img.DateTaken);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeOrientation}", Helper.RotateFlipTypeToByte(img.Orientation));
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHistogram}", Helper.ArrayFromFloat(img.GetHistogram()));
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeVector}", img.GetVector());
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeFamily}", img.Family);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                    throw;
                }
                finally {
                    Monitor.Exit(_sqllock);
                }
            }
            else {
                throw new Exception();
            }
        }

        public static void LoadImages(IProgress<string> progress)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributeName}, "); // 0
            sb.Append($"{AppConsts.AttributeHash}, "); // 1
            sb.Append($"{AppConsts.AttributeLastView}, "); // 2
            sb.Append($"{AppConsts.AttributeOrientation}, "); // 3
            sb.Append($"{AppConsts.AttributeHistogram}, "); // 4
            sb.Append($"{AppConsts.AttributeVector}, "); // 5
            sb.Append($"{AppConsts.AttributeFamily}, "); // 6
            sb.Append($"{AppConsts.AttributeDateTaken} "); // 7
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
                        var lastview = reader.GetDateTime(2);
                        var orientation = Helper.ByteToRotateFlipType(reader.GetByte(3));
                        var histogram = Helper.ArrayToFloat((byte[])reader[4]);
                        var vector = (byte[])reader[5];
                        var family = reader.GetString(6);
                        var datetaken = reader.GetDateTime(7);
                        var img = new Img(
                            name: name,
                            hash: hash,
                            datetaken: datetaken,
                            lastview: lastview,
                            orientation: orientation,
                            histogram: histogram,
                            vector: vector,
                            family: family
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

            progress?.Report("Loading vars...");

            sb.Length = 0;
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributePalette} "); // 0
            sb.Append($"FROM {AppConsts.TableVars}");
            sqltext = sb.ToString();
            using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                using (var reader = sqlCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        var palette = (byte[])reader[0];
                        ColorHelper.Set(palette);
                        break;
                    }
                }
            }

            //AppImgs.Populate();

            progress?.Report("Database loaded");
        }
    }
}
