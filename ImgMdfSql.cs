using System.Text;
using System.Data.SqlClient;
using OpenCvSharp;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlUpdateProperty(int id, string key, object val)
        {
            lock (_sqllock) {
                try {
                    var sqltext = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttrId} = @{AppConsts.AttrId}";
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", id);
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

        private static void SqlAdd(Img img, Mat akazedescriptors, Mat akazemirrordescriptors)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                    sb.Append($"{AppConsts.AttrId}, ");
                    sb.Append($"{AppConsts.AttrHash}, ");

                    sb.Append($"{AppConsts.AttrWidth}, ");
                    sb.Append($"{AppConsts.AttrHeight}, ");
                    sb.Append($"{AppConsts.AttrSize}, ");

                    sb.Append($"{AppConsts.AttrAkazeDescriptorsBlob}, ");
                    sb.Append($"{AppConsts.AttrAkazeMirrorDescriptorsBlob}, ");
                    sb.Append($"{AppConsts.AttrAkazeCentroid}, ");
                    sb.Append($"{AppConsts.AttrAkazeMirrorCentroid}, ");
                    sb.Append($"{AppConsts.AttrAkazePairs}, ");

                    sb.Append($"{AppConsts.AttrLastChanged}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}, ");
                    sb.Append($"{AppConsts.AttrNextHash}, ");
                    sb.Append($"{AppConsts.AttrCounter} ");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrId}, ");
                    sb.Append($"@{AppConsts.AttrHash}, ");

                    sb.Append($"@{AppConsts.AttrWidth}, ");
                    sb.Append($"@{AppConsts.AttrHeight}, ");
                    sb.Append($"@{AppConsts.AttrSize}, ");

                    sb.Append($"@{AppConsts.AttrAkazeDescriptorsBlob}, ");
                    sb.Append($"@{AppConsts.AttrAkazeMirrorDescriptorsBlob}, ");
                    sb.Append($"@{AppConsts.AttrAkazeCentroid}, ");
                    sb.Append($"@{AppConsts.AttrAkazeMirrorCentroid}, ");
                    sb.Append($"@{AppConsts.AttrAkazePairs}, ");

                    sb.Append($"@{AppConsts.AttrLastChanged}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}, ");
                    sb.Append($"@{AppConsts.AttrNextHash}, ");
                    sb.Append($"@{AppConsts.AttrCounter}");
                    sb.Append(')');
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    sqlCommand.CommandText = sb.ToString();
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrId}", img.Id);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHash}", img.Hash);

                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrWidth}", img.Width);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHeight}", img.Height);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrSize}", img.Size);

                    var akazedescriptorsblob = ImageHelper.ArrayFromMat(akazedescriptors);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrAkazeDescriptorsBlob}", akazedescriptorsblob);
                    var akazemirrordescriptorsblob = ImageHelper.ArrayFromMat(akazemirrordescriptors);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrAkazeMirrorDescriptorsBlob}", akazemirrordescriptorsblob);

                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrAkazeCentroid}", img.AkazeCentroid);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrAkazeMirrorCentroid}", img.AkazeMirrorCentroid);


                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrAkazePairs}", img.AkazePairs);

                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastChanged}", img.LastChanged);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNextHash}", img.NextHash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCounter}", img.Counter);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}