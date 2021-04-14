using System.Text;
using System.Data.SqlClient;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlUpdateProperty(string name, string key, object val)
        {
            lock (_sqllock) {
                try {
                    var sqltext = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttrName} = @{AppConsts.AttrName}";
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", name);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                }
            }
        }

        public static void SqlUpdateVar(string key, object val)
        {
            lock (_sqllock) {
                var sqltext = $"UPDATE {AppConsts.TableVars} SET {key} = @{key}";
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlDelete(string name)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttrName} = @{AppConsts.AttrName}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", name);
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
                    sb.Append($"{AppConsts.AttrFolder}, ");
                    sb.Append($"{AppConsts.AttrHash}, ");

                    sb.Append($"{AppConsts.AttrWidth}, ");
                    sb.Append($"{AppConsts.AttrHeight}, ");
                    sb.Append($"{AppConsts.AttrSize}, ");

                    sb.Append($"{AppConsts.AttrColorDescriptors}, ");
                    sb.Append($"{AppConsts.AttrColorDistance}, ");
                    sb.Append($"{AppConsts.AttrPerceptiveDescriptorsBlob}, ");
                    sb.Append($"{AppConsts.AttrPerceptiveDistance}, ");
                    sb.Append($"{AppConsts.AttrOrbDescriptorsBlob}, ");
                    sb.Append($"{AppConsts.AttrOrbKeyPointsBlob}, ");
                    sb.Append($"{AppConsts.AttrOrbDistance}, ");

                    sb.Append($"{AppConsts.AttrLastChanged}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}, ");
                    sb.Append($"{AppConsts.AttrNextHash}, ");

                    sb.Append($"{AppConsts.AttrCounter}, ");
                    sb.Append($"{AppConsts.AttrLastId}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrId}, ");
                    sb.Append($"@{AppConsts.AttrName}, ");
                    sb.Append($"@{AppConsts.AttrFolder}, ");
                    sb.Append($"@{AppConsts.AttrHash}, ");

                    sb.Append($"@{AppConsts.AttrWidth}, ");
                    sb.Append($"@{AppConsts.AttrHeight}, ");
                    sb.Append($"@{AppConsts.AttrSize}, ");

                    sb.Append($"@{AppConsts.AttrColorDescriptors}, ");
                    sb.Append($"@{AppConsts.AttrColorDistance}, ");
                    sb.Append($"@{AppConsts.AttrPerceptiveDescriptorsBlob}, ");
                    sb.Append($"@{AppConsts.AttrPerceptiveDistance}, ");
                    sb.Append($"@{AppConsts.AttrOrbDescriptorsBlob}, ");
                    sb.Append($"@{AppConsts.AttrOrbKeyPointsBlob}, ");
                    sb.Append($"@{AppConsts.AttrOrbDistance}, ");

                    sb.Append($"@{AppConsts.AttrLastChanged}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}, ");
                    sb.Append($"@{AppConsts.AttrNextHash}, ");

                    sb.Append($"@{AppConsts.AttrCounter}, ");
                    sb.Append($"@{AppConsts.AttrLastId}");
                    sb.Append(')');
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    sqlCommand.CommandText = sb.ToString();
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrFolder}", img.Folder);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHash}", img.Hash);

                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrWidth}", img.Width);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHeight}", img.Height);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrSize}", img.Size);

                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrColorDescriptors}", img.ColorDescriptors);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrColorDistance}", img.ColorDistance);

                    var perceptivedescriptorsblob = ImageHelper.ArrayFrom64(img.PerceptiveDescriptors);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrPerceptiveDescriptorsBlob}", perceptivedescriptorsblob);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrPerceptiveDistance}", img.PerceptiveDistance);

                    var orbdescriptorsblob = ImageHelper.ArrayFromMat(img.OrbDescriptors);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrOrbDescriptorsBlob}", orbdescriptorsblob);
                    var orbkeypointsblob = ImageHelper.ArrayFromKeyPoints(img.OrbKeyPoints);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrOrbKeyPointsBlob}", orbkeypointsblob);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrOrbDistance}", img.OrbDistance);

                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastChanged}", img.LastChanged);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNextHash}", img.NextHash);

                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCounter}", img.Counter);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastId}", img.LastId);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}