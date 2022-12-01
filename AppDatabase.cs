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

        public static void AddImage(Img img)
        {
            using (var sqlCommand = _sqlConnection.CreateCommand()) {
                sqlCommand.Connection = _sqlConnection;
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                sb.Append($"{AppConsts.AttributeId}, ");
                sb.Append($"{AppConsts.AttributeName}, ");
                sb.Append($"{AppConsts.AttributeHash}, ");
                sb.Append($"{AppConsts.AttributeYear}, ");
                sb.Append($"{AppConsts.AttributeLastView}, ");
                sb.Append($"{AppConsts.AttributeFamilyId}, ");
                sb.Append($"{AppConsts.AttributeHist}");
                sb.Append(") VALUES (");
                sb.Append($"@{AppConsts.AttributeId}, ");
                sb.Append($"@{AppConsts.AttributeName}, ");
                sb.Append($"@{AppConsts.AttributeHash}, ");
                sb.Append($"@{AppConsts.AttributeYear}, ");
                sb.Append($"@{AppConsts.AttributeLastView}, ");
                sb.Append($"@{AppConsts.AttributeFamilyId}, ");
                sb.Append($"@{AppConsts.AttributeHist}");
                sb.Append(')');
                sqlCommand.CommandText = sb.ToString();
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeId}", img.Id);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeName}", img.Name);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHash}", img.Hash);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeYear}", img.Year);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeLastView}", img.LastView);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeFamilyId}", img.FamilyId);
                sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttributeHist}", Helper.ArrayFromFloat(img.GetHist()));
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
            sb.Append($"{AppConsts.AttributeYear}, "); // 3
            sb.Append($"{AppConsts.AttributeLastView}, "); // 4
            sb.Append($"{AppConsts.AttributeFamilyId}, "); // 5
            sb.Append($"{AppConsts.AttributeHist} "); // 6
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
                        var year = reader.GetInt32(3);
                        var lastview = reader.GetDateTime(4);
                        var familyid = reader.GetInt32(5);
                        var buffer = (byte[])reader[6];
                        var hist = Helper.ArrayToFloat(buffer);
                        var img = new Img(
                            id: id,
                            name: name,
                            hash: hash,
                            year: year,
                            lastview: lastview,
                            familyid: familyid,
                            hist: hist
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
            sb.Append($"{AppConsts.AttributeId}, "); // 0
            sb.Append($"{AppConsts.AttributeFamilyId}, "); // 1
            sb.Append($"{AppConsts.AttributePalette} "); // 2
            sb.Append($"FROM {AppConsts.TableVars}");
            sqltext = sb.ToString();
            using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                using (var reader = sqlCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        var id = reader.GetInt32(0);
                        var familyid = reader.GetInt32(1);
                        var palette = (byte[])reader[2];
                        AppVars.SetVars(id, familyid);
                        AppPalette.Set(palette);
                        break;
                    }
                }
            }

            progress?.Report("Loading families...");

            sb.Length = 0;
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttributeFamilyId}, "); // 0
            sb.Append($"{AppConsts.AttributeDescription} "); // 1
            sb.Append($"FROM {AppConsts.TableFamilies}");
            sqltext = sb.ToString();
            using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                using (var reader = sqlCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        var familyid = reader.GetInt32(0);
                        var description = reader.GetString(1);
                        AppVars.AddFamily(familyid, description);
                    }
                }
            }

            progress?.Report("Database loaded");
        }
    }
}
