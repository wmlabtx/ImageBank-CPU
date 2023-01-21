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
                        sb.Append($"{AppConsts.AttributeHash}, ");
                        sb.Append($"{AppConsts.AttributeFolder}, ");
                        sb.Append($"{AppConsts.AttributeDateTaken}, ");
                        sb.Append($"{AppConsts.AttributeHistogram}, ");
                        sb.Append($"{AppConsts.AttributeVector}, ");
                        sb.Append($"{AppConsts.AttributeLastView}, ");
                        sb.Append($"{AppConsts.AttributeOrientation}, ");
                        sb.Append($"{AppConsts.AttributeBestHash}");
                        sb.Append(") VALUES (");
                        sb.Append($"@{AppConsts.AttributeHash}, ");
                        sb.Append($"@{AppConsts.AttributeFolder}, ");
                        sb.Append($"@{AppConsts.AttributeDateTaken}, ");
                        sb.Append($"@{AppConsts.AttributeHistogram}, ");
                        sb.Append($"@{AppConsts.AttributeVector}, ");
                        sb.Append($"@{AppConsts.AttributeLastView}, ");
                        sb.Append($"@{AppConsts.AttributeOrientation}, ");
                        sb.Append($"@{AppConsts.AttributeBestHash}");
                        sb.Append(')');
                        sqlCommand.CommandText = sb.ToString();
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeFolder}", img.Folder);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeDateTaken}", img.DateTaken);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHistogram}", Helper.ArrayFromFloat(img.GetHistogram()));
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeVector}", img.GetVector());
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeOrientation}", Helper.RotateFlipTypeToByte(img.Orientation));
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeBestHash}", img.BestHash);
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
            sb.Append($"{AppConsts.AttributeHash}, "); // 0
            sb.Append($"{AppConsts.AttributeFolder}, "); // 1
            sb.Append($"{AppConsts.AttributeDateTaken}, "); // 2
            sb.Append($"{AppConsts.AttributeHistogram}, "); // 3
            sb.Append($"{AppConsts.AttributeVector}, "); // 4
            sb.Append($"{AppConsts.AttributeLastView}, "); // 5
            sb.Append($"{AppConsts.AttributeOrientation}, "); // 6
            sb.Append($"{AppConsts.AttributeBestHash} "); // 7
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandText = sqltext;
                using (var reader = sqlCommand.ExecuteReader()) {
                    var dtn = DateTime.Now;
                    while (reader.Read()) {
                        var hash = reader.GetString(0);
                        var folder = reader.GetString(1);
                        var datetaken = reader.GetDateTime(2);
                        var histogram = Helper.ArrayToFloat((byte[])reader[3]);
                        var vector = (byte[])reader[4];
                        var lastview = reader.GetDateTime(5);
                        var orientation = Helper.ByteToRotateFlipType(reader.GetByte(6));
                        var besthash = reader.GetString(7);
                        var img = new Img(
                            hash: hash,
                            folder: folder,
                            datetaken: datetaken,
                            histogram: histogram,
                            vector: vector,
                            lastview: lastview,
                            orientation: orientation,
                            besthash: besthash
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

            // AppImgs.Populate(AppVars.Progress);

            progress?.Report("Database loaded");
        }
    }
}
