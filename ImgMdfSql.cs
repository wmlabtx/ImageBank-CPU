using System.Text;
using System.Data.SqlClient;
using System;
using OpenCvSharp.Dnn;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlUpdateProperty(string name, string key, object val)
        {
            lock (_sqllock) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = $"UPDATE {AppConsts.TableImages} SET {key} = @{key} WHERE {AppConsts.AttrName} = @{AppConsts.AttrName}";
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
            if (img.Name == null || img.Name.Length == 0 || img.Name.Length != 10) {
                throw new ArgumentException("name == null || name.Length == 0 || name.Length != 10");
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

            if (img.NextHash == null || img.NextHash.Length != 32) {
                throw new ArgumentException("nexthash == null || nexthash.Length != 32");
            }

            if (img.Sim < 0f || img.Sim > 1f) {
                throw new ArgumentException("img.Sim < 0f || img.Sim > 1f");
            }

            if (img.Generation < 0 || img.Generation > 99) {
                throw new ArgumentException("generation < 0 || generation > 99");
            }

            ImageHelper.FromFeaturePoints(img.Fp, out var ki, out var kx, out var ky, out var ka, out var ks);
            ImageHelper.FromFeaturePoints(img.FpMirror, out var kimirror, out var kxmirror, out var kymirror, out var kamirror, out var ksmirror);

            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableImages} (");
                    sb.Append($"{AppConsts.AttrName}, ");
                    sb.Append($"{AppConsts.AttrHash}, ");
                    sb.Append($"{AppConsts.AttrWidth}, ");
                    sb.Append($"{AppConsts.AttrHeight}, ");
                    sb.Append($"{AppConsts.AttrSize}, ");
                    sb.Append($"{AppConsts.AttrDateTaken}, ");
                    sb.Append($"{AppConsts.AttrMetadata}, ");
                    sb.Append($"{AppConsts.AttrKi}, ");
                    sb.Append($"{AppConsts.AttrKx}, ");
                    sb.Append($"{AppConsts.AttrKy}, ");
                    sb.Append($"{AppConsts.AttrKa}, ");
                    sb.Append($"{AppConsts.AttrKs}, ");
                    sb.Append($"{AppConsts.AttrKiMirror}, ");
                    sb.Append($"{AppConsts.AttrKxMirror}, ");
                    sb.Append($"{AppConsts.AttrKyMirror}, ");
                    sb.Append($"{AppConsts.AttrKaMirror}, ");
                    sb.Append($"{AppConsts.AttrKsMirror}, ");
                    sb.Append($"{AppConsts.AttrNextHash}, ");
                    sb.Append($"{AppConsts.AttrSim}, ");
                    sb.Append($"{AppConsts.AttrLastChanged}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}, ");
                    sb.Append($"{AppConsts.AttrGeneration} ");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrName}, ");
                    sb.Append($"@{AppConsts.AttrHash}, ");
                    sb.Append($"@{AppConsts.AttrWidth}, ");
                    sb.Append($"@{AppConsts.AttrHeight}, ");
                    sb.Append($"@{AppConsts.AttrSize}, ");
                    sb.Append($"@{AppConsts.AttrDateTaken}, ");
                    sb.Append($"@{AppConsts.AttrMetadata}, ");
                    sb.Append($"@{AppConsts.AttrKi}, ");
                    sb.Append($"@{AppConsts.AttrKx}, ");
                    sb.Append($"@{AppConsts.AttrKy}, ");
                    sb.Append($"@{AppConsts.AttrKa}, ");
                    sb.Append($"@{AppConsts.AttrKs}, ");
                    sb.Append($"@{AppConsts.AttrKiMirror}, ");
                    sb.Append($"@{AppConsts.AttrKxMirror}, ");
                    sb.Append($"@{AppConsts.AttrKyMirror}, ");
                    sb.Append($"@{AppConsts.AttrKaMirror}, ");
                    sb.Append($"@{AppConsts.AttrKsMirror}, ");
                    sb.Append($"@{AppConsts.AttrNextHash}, ");
                    sb.Append($"@{AppConsts.AttrSim}, ");
                    sb.Append($"@{AppConsts.AttrLastChanged}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}, ");
                    sb.Append($"@{AppConsts.AttrGeneration}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHash}", img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrWidth}", img.Width);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHeight}", img.Height);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrSize}", img.Size);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDateTaken}", img.DateTaken ?? new DateTime(1980, 1, 1));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrMetadata}", img.MetaData.Substring(0, Math.Min(250, img.MetaData.Length)));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKi}", Helper.ShortToBuffer(ki));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKx}", Helper.ShortToBuffer(kx));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKy}", Helper.ShortToBuffer(ky));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKa}", Helper.ShortToBuffer(ka));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKs}", Helper.ShortToBuffer(ks));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKiMirror}", Helper.ShortToBuffer(kimirror));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKxMirror}", Helper.ShortToBuffer(kxmirror));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKyMirror}", Helper.ShortToBuffer(kymirror));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKaMirror}", Helper.ShortToBuffer(kamirror));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKsMirror}", Helper.ShortToBuffer(ksmirror));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNextHash}", img.NextHash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrSim}", img.Sim);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastChanged}", img.LastChanged);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrGeneration}", img.Generation);
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
            sb.Append($"{AppConsts.AttrName}, "); // 0
            sb.Append($"{AppConsts.AttrHash}, "); // 1
            sb.Append($"{AppConsts.AttrWidth}, "); // 2
            sb.Append($"{AppConsts.AttrHeight}, "); // 3
            sb.Append($"{AppConsts.AttrSize}, "); // 4
            sb.Append($"{AppConsts.AttrDateTaken}, "); // 5
            sb.Append($"{AppConsts.AttrMetadata}, "); // 6
            sb.Append($"{AppConsts.AttrKi}, "); // 7
            sb.Append($"{AppConsts.AttrKx}, "); // 8
            sb.Append($"{AppConsts.AttrKy}, "); // 9
            sb.Append($"{AppConsts.AttrKa}, "); // 10
            sb.Append($"{AppConsts.AttrKs}, "); // 11
            sb.Append($"{AppConsts.AttrKiMirror}, "); // 12
            sb.Append($"{AppConsts.AttrKxMirror}, "); // 13
            sb.Append($"{AppConsts.AttrKyMirror}, "); // 14
            sb.Append($"{AppConsts.AttrKaMirror}, "); // 15
            sb.Append($"{AppConsts.AttrKsMirror}, "); // 16
            sb.Append($"{AppConsts.AttrNextHash}, "); // 17
            sb.Append($"{AppConsts.AttrSim}, "); // 18
            sb.Append($"{AppConsts.AttrLastChanged}, "); // 19
            sb.Append($"{AppConsts.AttrLastView}, "); // 20
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 21
            sb.Append($"{AppConsts.AttrGeneration} "); // 22
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = sqltext;
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var name = reader.GetString(0);
                            var hash = reader.GetString(1);
                            var width = reader.GetInt32(2);
                            var height = reader.GetInt32(3);
                            var size = reader.GetInt32(4);
                            var dt = reader.GetDateTime(5);
                            DateTime? datetaken = null;
                            if (dt.Year > 1980) {
                                datetaken = dt;
                            }

                            var metadata = reader.GetString(6);
                            var ki = Helper.ShortFromBuffer((byte[])reader[7]);
                            var kx = Helper.ShortFromBuffer((byte[])reader[8]);
                            var ky = Helper.ShortFromBuffer((byte[])reader[9]);
                            var ka = Helper.ShortFromBuffer((byte[])reader[10]);
                            var ks = Helper.ShortFromBuffer((byte[])reader[11]);
                            var kimirror = Helper.ShortFromBuffer((byte[])reader[12]);
                            var kxmirror = Helper.ShortFromBuffer((byte[])reader[13]);
                            var kymirror = Helper.ShortFromBuffer((byte[])reader[14]);
                            var kamirror = Helper.ShortFromBuffer((byte[])reader[15]);
                            var ksmirror = Helper.ShortFromBuffer((byte[])reader[16]);
                            var nexthash = reader.GetString(17);
                            var sim = reader.GetFloat(18);
                            var lastchanged = reader.GetDateTime(19);
                            var lastview = reader.GetDateTime(20);
                            var lastcheck = reader.GetDateTime(21);
                            var generation = reader.GetInt32(22);

                            var fp = ImageHelper.ToFeaturePoints(ki, kx, ky, ka, ks);
                            var fpmirror = ImageHelper.ToFeaturePoints(kimirror, kxmirror, kymirror, kamirror, ksmirror);

                            var img = new Img(
                                name: name,
                                hash: hash,
                                width: width,
                                height: height,
                                size: size,
                                datetaken: datetaken,
                                metadata: metadata,
                                fp: fp,
                                fpmirror: fpmirror,
                                nexthash: nexthash,
                                sim: sim,
                                lastchanged: lastchanged,
                                lastview: lastview,
                                lastcheck: lastcheck,
                                generation: generation
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