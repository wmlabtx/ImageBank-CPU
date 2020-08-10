using System.Text;
using System.Data.SqlClient;
using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlUpdateProperty(string name, string key, object val)
        {
            lock (_sqllock) {
                try {
                    var sqltext = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttrName} = @{AppConsts.AttrName}";
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", name);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
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
                    sb.Append($"{AppConsts.AttrName}, ");
                    sb.Append($"{AppConsts.AttrFolder}, ");
                    sb.Append($"{AppConsts.AttrPath}, ");
                    sb.Append($"{AppConsts.AttrHash}, ");
                    sb.Append($"{AppConsts.AttrPHash}, ");
                    sb.Append($"{AppConsts.AttrCounter}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrWidth}, ");
                    sb.Append($"{AppConsts.AttrHeigth}, ");
                    sb.Append($"{AppConsts.AttrSize}, ");
                    sb.Append($"{AppConsts.AttrDescriptors}, ");
                    sb.Append($"{AppConsts.AttrScd}, ");
                    sb.Append($"{AppConsts.AttrDt}, ");
                    sb.Append($"{AppConsts.AttrDv}, ");
                    sb.Append($"{AppConsts.AttrNextName}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrName}, ");
                    sb.Append($"@{AppConsts.AttrFolder}, ");
                    sb.Append($"@{AppConsts.AttrPath}, ");
                    sb.Append($"@{AppConsts.AttrHash}, ");
                    sb.Append($"@{AppConsts.AttrPHash}, ");
                    sb.Append($"@{AppConsts.AttrCounter}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrWidth}, ");
                    sb.Append($"@{AppConsts.AttrHeigth}, ");
                    sb.Append($"@{AppConsts.AttrSize}, ");
                    sb.Append($"@{AppConsts.AttrDescriptors}, ");
                    sb.Append($"@{AppConsts.AttrScd}, ");
                    sb.Append($"@{AppConsts.AttrDt}, ");
                    sb.Append($"@{AppConsts.AttrDv}, ");
                    sb.Append($"@{AppConsts.AttrNextName}");
                    sb.Append(")");
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrFolder}", img.Folder);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrPath}", img.Path);
                    var bhash = BitConverter.GetBytes(img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHash}", bhash);
                    var bphash = BitConverter.GetBytes(img.PHash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrPHash}", bphash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCounter}", img.Counter);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrWidth}", img.Width);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHeigth}", img.Heigth);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrSize}", img.Size);
                    var bdescriptors = Helper.DescriptorsToBuffer(img.GetDescriptors());
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDescriptors}", bdescriptors);
                    var bscd = img.Scd.GetBuffer();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrScd}", bscd);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDt}", img.Dt);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDv}", img.Dv);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNextName}", img.NextName);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}