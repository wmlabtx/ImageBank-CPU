using System.Text;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlUpdateProperty(int id, string key, object val)
        {
            lock (_sqllock) {
                var sqltext = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttrId} = @{AppConsts.AttrId}";
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", id);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void SqlUpdateVar(string key, object val)
        {
            lock (_sqllock) {
                var sqltext = $"UPDATE {AppConsts.TableVars} SET {key} = @{key}";
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{key}", val);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlDelete(int id)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttrId} = @{AppConsts.AttrId}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", id);
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
                    sb.Append($"{AppConsts.AttrChecksum}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrNextId}, ");
                    sb.Append($"{AppConsts.AttrDistance}, ");
                    sb.Append($"{AppConsts.AttrLastChange}, ");
                    sb.Append($"{AppConsts.AttrGeneration}, ");
                    sb.Append($"{AppConsts.AttrLastId}, ");
                    sb.Append($"{AppConsts.AttrLastFind}, ");
                    sb.Append($"{AppConsts.AttrVector}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrId}, ");
                    sb.Append($"@{AppConsts.AttrName}, ");
                    sb.Append($"@{AppConsts.AttrChecksum}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrNextId}, ");
                    sb.Append($"@{AppConsts.AttrDistance}, ");
                    sb.Append($"@{AppConsts.AttrLastChange}, ");
                    sb.Append($"@{AppConsts.AttrGeneration}, ");
                    sb.Append($"@{AppConsts.AttrLastId}, ");
                    sb.Append($"@{AppConsts.AttrLastFind}, ");
                    sb.Append($"@{AppConsts.AttrVector}");
                    sb.Append(")");
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChecksum}", img.Checksum);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNextId}", img.NextId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDistance}", img.Distance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastChange}", img.LastChange);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrGeneration}", img.Generation);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastId}", img.LastId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastFind}", img.LastFind);
                    var buffer = Helper.VectorToBuffer(img.Vector());
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrVector}", buffer);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static int SqlGetIdByName(string name)
        {
            Contract.Requires(name != null);
            lock (_sqllock) {
                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrId} ");
                sb.Append($"FROM {AppConsts.TableImages} ");
                sb.Append($"WHERE {AppConsts.AttrName} = @{AppConsts.AttrName}");
                var sqltext = sb.ToString();
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", name);
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            var id = reader.GetInt32(0);
                            return id;
                        }
                    }
                }
            }

            return 0;
        }

        private static int SqlGetIdByChecksum(string checksum)
        {
            Contract.Requires(checksum != null);
            lock (_sqllock) {
                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrId} ");
                sb.Append($"FROM {AppConsts.TableImages} ");
                sb.Append($"WHERE {AppConsts.AttrChecksum} = @{AppConsts.AttrChecksum}");
                var sqltext = sb.ToString();
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChecksum}", checksum);
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            var id = reader.GetInt32(0);
                            return id;
                        }
                    }
                }
            }

            return 0;
        }
    }
}