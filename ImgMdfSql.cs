using System.Text;
using System.Data.SqlClient;
using System;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlUpdateProperty(string filename, string key, object val)
        {
            lock (_sqllock) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttrFileName} = @{AppConsts.AttrFileName}";
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrFileName}", filename);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                }
            }
        }

        private static void SqlDelete(string filename)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableImages} WHERE {AppConsts.AttrFileName} = @{AppConsts.AttrFileName}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrFileName}", filename);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlAdd(Img img)
        {
            if (img.FileName == null || img.FileName.Length == 0 || img.FileName.Length > 128) {
                throw new ArgumentException("filename == null || filename.Length == 0 || filename.Length > 128");
            }

            if (img.Hash == null || img.Hash.Length != 32) {
                throw new ArgumentException("hash == null || hash.Length != 32");
            }

            if (img.Width <= 0) {
                throw new ArgumentException("width <= 0");
            }

            if (img.Height <= 0) {
                throw new ArgumentException("height <= 0");
            }

            if (img.Size <= 0) {
                throw new ArgumentException("size <= 0");
            }

            if (img.KazeOne == null || img.KazeOne.Length == 0 || img.KazeOne.Length > AppConsts.MaxDescriptors) {
                throw new ArgumentException("kazeone == null || kazeone.Length == 0 || kazeone.Length > AppConsts.MaxDescriptors");
            }

            if (img.KazeTwo == null || img.KazeTwo.Length == 0 || img.KazeTwo.Length > AppConsts.MaxDescriptors) {
                throw new ArgumentException("kazetwo == null || kazetwo.Length == 0 || kazetwo.Length > AppConsts.MaxDescriptors");
            }

            if (img.NextHash == null || img.NextHash.Length != 32) {
                throw new ArgumentException("nexthash == null || nexthash.Length != 32");
            }

            if (img.KazeMatch < 0 || img.KazeMatch > AppConsts.MaxDescriptors) {
                throw new ArgumentException("kazematch < 0 || kazematch > AppConsts.MaxDescriptors");
            }

            if (img.Counter < 0) {
                throw new ArgumentException("counter < 0");
            }

            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                    sb.Append($"{AppConsts.AttrFileName}, ");
                    sb.Append($"{AppConsts.AttrHash}, ");
                    sb.Append($"{AppConsts.AttrWidth}, ");
                    sb.Append($"{AppConsts.AttrHeight}, ");
                    sb.Append($"{AppConsts.AttrSize}, ");
                    sb.Append($"{AppConsts.AttrKazeOne}, ");
                    sb.Append($"{AppConsts.AttrKazeTwo}, ");
                    sb.Append($"{AppConsts.AttrNextHash}, ");
                    sb.Append($"{AppConsts.AttrKazeMatch}, ");
                    sb.Append($"{AppConsts.AttrLastChanged}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}, ");
                    sb.Append($"{AppConsts.AttrCounter} ");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrFileName}, ");
                    sb.Append($"@{AppConsts.AttrHash}, ");
                    sb.Append($"@{AppConsts.AttrWidth}, ");
                    sb.Append($"@{AppConsts.AttrHeight}, ");
                    sb.Append($"@{AppConsts.AttrSize}, ");
                    sb.Append($"@{AppConsts.AttrKazeOne}, ");
                    sb.Append($"@{AppConsts.AttrKazeTwo}, ");
                    sb.Append($"@{AppConsts.AttrNextHash}, ");
                    sb.Append($"@{AppConsts.AttrKazeMatch}, ");
                    sb.Append($"@{AppConsts.AttrLastChanged}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}, ");
                    sb.Append($"@{AppConsts.AttrCounter}");
                    sb.Append(")");
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrFileName}", img.FileName);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHash}", img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrWidth}", img.Width);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHeight}", img.Height);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrSize}", img.Size);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKazeOne}", img.KazeOne);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKazeTwo}", img.KazeTwo);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNextHash}", img.NextHash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKazeMatch}", img.KazeMatch);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastChanged}", img.LastChanged);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCounter}", img.Counter);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void LoadImgs(IProgress<string> progress)
        {
            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrFileName}, "); // 0
            sb.Append($"{AppConsts.AttrHash}, "); // 1
            sb.Append($"{AppConsts.AttrWidth}, "); // 2
            sb.Append($"{AppConsts.AttrHeight}, "); // 3
            sb.Append($"{AppConsts.AttrSize}, "); // 4
            sb.Append($"{AppConsts.AttrKazeOne}, "); // 5
            sb.Append($"{AppConsts.AttrKazeTwo}, "); // 6
            sb.Append($"{AppConsts.AttrNextHash}, "); // 7
            sb.Append($"{AppConsts.AttrKazeMatch}, "); // 8
            sb.Append($"{AppConsts.AttrLastChanged}, "); // 9
            sb.Append($"{AppConsts.AttrLastView}, "); // 10
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 11
            sb.Append($"{AppConsts.AttrCounter} "); // 12
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = sqltext;
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var filename = reader.GetString(0);
                            var hash = reader.GetString(1);
                            var width = reader.GetInt32(2);
                            var height = reader.GetInt32(3);
                            var size = reader.GetInt32(4);
                            var kazeone = (byte[])reader[5];
                            var kazetwo = (byte[])reader[6];
                            var nexthash = reader.GetString(7);
                            var kazematch = reader.GetInt32(8);
                            var lastchanged = reader.GetDateTime(9);
                            var lastview = reader.GetDateTime(10);
                            var lastcheck = reader.GetDateTime(11);
                            var counter = reader.GetInt32(12);

                            var img = new Img(
                                filename: filename,
                                hash: hash,
                                width: width,
                                height: height,
                                size: size,
                                kazeone: kazeone,
                                kazetwo: kazetwo,
                                nexthash: nexthash,
                                kazematch: kazematch,
                                lastchanged: lastchanged,
                                lastview: lastview,
                                lastcheck: lastcheck,
                                counter: counter
                               );

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                dtn = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }

                progress.Report("Database loaded");
            }
        }
    }
}