using System.Text;
using System.Data.SqlClient;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlUpdateProperty(string id, string key, object val)
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

        private static void SqlDelete(string id)
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
                    sb.Append($"{AppConsts.AttrFolder}, ");
                    sb.Append($"{AppConsts.AttrNextId}, ");
                    sb.Append($"{AppConsts.AttrDistance}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrVector}, ");
                    sb.Append($"{AppConsts.AttrCounter}, ");
                    sb.Append($"{AppConsts.AttrLastModified}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrId}, ");
                    sb.Append($"@{AppConsts.AttrFolder}, ");
                    sb.Append($"@{AppConsts.AttrNextId}, ");
                    sb.Append($"@{AppConsts.AttrDistance}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrVector}, ");
                    sb.Append($"@{AppConsts.AttrCounter}, ");
                    sb.Append($"@{AppConsts.AttrLastModified}");
                    sb.Append(")");
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrFolder}", img.Folder);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNextId}", img.NextId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDistance}", img.Distance);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrVector}", img.GetVector());
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCounter}", img.Counter);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastModified}", img.LastModified);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}