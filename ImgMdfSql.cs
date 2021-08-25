using System.Text;
using System.Data.SqlClient;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void SqlImagesUpdateProperty(string name, string key, object val)
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
                    sb.Append($"{AppConsts.AttrKiMirror}, ");
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
                    sb.Append($"@{AppConsts.AttrKiMirror}, ");
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
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKi}", Helper.ArrayFrom16(img.Ki[0]));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKiMirror}", Helper.ArrayFrom16(img.Ki[1]));
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
            sb.Append($"{AppConsts.AttrKiMirror}, "); // 8
            sb.Append($"{AppConsts.AttrNextHash}, "); // 9
            sb.Append($"{AppConsts.AttrSim}, "); // 10
            sb.Append($"{AppConsts.AttrLastChanged}, "); // 11
            sb.Append($"{AppConsts.AttrLastView}, "); // 12
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 13
            sb.Append($"{AppConsts.AttrGeneration} "); // 14
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
                            var ki = new short[2][];
                            ki[0] = Helper.ArrayTo16((byte[])reader[7]);
                            ki[1] = Helper.ArrayTo16((byte[])reader[8]);
                            var nexthash = reader.GetString(9);
                            var sim = reader.GetFloat(10);
                            var lastchanged = reader.GetDateTime(11);
                            var lastview = reader.GetDateTime(12);
                            var lastcheck = reader.GetDateTime(13);
                            var generation = reader.GetInt32(14);

                            var img = new Img(
                                name: name,
                                hash: hash,
                                width: width,
                                height: height,
                                size: size,
                                datetaken: datetaken,
                                metadata: metadata,
                                ki: ki,
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
                                if (progress != null) {
                                    progress.Report($"Loading images ({_imgList.Count})...");
                                }
                            }
                        }
                    }
                }

                _nodes.Clear();
                sb.Clear();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrNodeId}, "); // 0
                sb.Append($"{AppConsts.AttrDescriptor}, "); // 1
                sb.Append($"{AppConsts.AttrWeight} "); // 2
                sb.Append($"FROM {AppConsts.TableNodes}");
                sqltext = sb.ToString();
                if (progress != null) {
                    progress.Report("Loading nodes...");
                }

                lock (_sqllock) {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = sqltext;
                        using (var reader = sqlCommand.ExecuteReader()) {
                            var dtn = DateTime.Now;
                            while (reader.Read()) {
                                var nodeid = reader.GetInt32(0);
                                var descriptor = Helper.ArrayToFloat((byte[])reader[1]);
                                var weight = reader.GetInt32(2);
                                var node = new Node(
                                    descriptor: descriptor,
                                    weight: weight
                                   );

                                _nodes.Add(nodeid, node);
                            }
                        }
                    }
                }

                if (progress != null) {
                    progress.Report("Database loaded");
                }
            }
        }

        public static void SqlAddNode(int nodeid, Node node)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableNodes} (");
                    sb.Append($"{AppConsts.AttrNodeId},");
                    sb.Append($"{AppConsts.AttrDescriptor},");
                    sb.Append($"{AppConsts.AttrWeight}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrNodeId},");
                    sb.Append($"@{AppConsts.AttrDescriptor},");
                    sb.Append($"@{AppConsts.AttrWeight}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDescriptor}", Helper.ArrayFromFloat(node.Descriptor));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrWeight}", node.Weight);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void SqlUpdateNode(int nodeid, Node node)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"UPDATE {AppConsts.TableNodes} SET ");
                    sb.Append($"{AppConsts.AttrDescriptor} = @{AppConsts.AttrDescriptor}, ");
                    sb.Append($"{AppConsts.AttrWeight} = @{AppConsts.AttrWeight} ");
                    sb.Append($"WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}");
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDescriptor}", Helper.ArrayFromFloat(node.Descriptor));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrWeight}", node.Weight);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void SqlTruncateNodes()
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"TRUNCATE TABLE {AppConsts.TableNodes}";
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
    }
}