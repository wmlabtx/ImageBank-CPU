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

        public static void SqlUpdateProperty(int nodeid, string key, object val)
        {
            lock (_sqllock) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = $"UPDATE {AppConsts.TableNodes} SET {key} = @{key} WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}";                        
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
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

        private static void SqlDelete(int nodeid)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"DELETE FROM {AppConsts.TableNodes} WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
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
                    sb.Append($"{AppConsts.AttrVector0}, ");
                    sb.Append($"{AppConsts.AttrNode0}, ");
                    sb.Append($"{AppConsts.AttrVector1}, ");
                    sb.Append($"{AppConsts.AttrNode1}, ");
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
                    sb.Append($"@{AppConsts.AttrVector0}, ");
                    sb.Append($"@{AppConsts.AttrNode0}, ");
                    sb.Append($"@{AppConsts.AttrVector1}, ");
                    sb.Append($"@{AppConsts.AttrNode1}, ");
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
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrVector0}", Helper.FloatToBuffer(img.Vector[0]));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNode0}", img.Node[0]);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrVector1}", Helper.FloatToBuffer(img.Vector[1]));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNode1}", img.Node[1]);
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

        private static void SqlAdd(Node node)
        {
            if (node.NodeId <= 0) {
                throw new ArgumentOutOfRangeException(nameof(SqlAdd), nameof(node.NodeId));
            }

            if (node.PrevId < 0) {
                throw new ArgumentOutOfRangeException(nameof(SqlAdd), nameof(node.PrevId));
            }

            if (node.Core == null) {
                throw new ArgumentOutOfRangeException(nameof(SqlAdd), nameof(node.Core));
            }

            if (node.Radius < 0f) {
                throw new ArgumentOutOfRangeException(nameof(SqlAdd), nameof(node.Radius));
            }

            if (node.ChildId == null || node.ChildId.Length != 2 || node.ChildId[0] < 0 || node.ChildId[1] < 0) {
                throw new ArgumentOutOfRangeException(nameof(SqlAdd), nameof(node.ChildId));
            }

            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableNodes} (");
                    sb.Append($"{AppConsts.AttrNodeId}, ");
                    sb.Append($"{AppConsts.AttrPrevId}, ");
                    sb.Append($"{AppConsts.AttrCore}, ");
                    sb.Append($"{AppConsts.AttrRadius}, ");
                    sb.Append($"{AppConsts.AttrChildId0}, ");
                    sb.Append($"{AppConsts.AttrChildId1}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrNodeId}, ");
                    sb.Append($"@{AppConsts.AttrPrevId}, ");
                    sb.Append($"@{AppConsts.AttrCore}, ");
                    sb.Append($"@{AppConsts.AttrRadius}, ");
                    sb.Append($"@{AppConsts.AttrChildId0}, ");
                    sb.Append($"@{AppConsts.AttrChildId1}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", node.NodeId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrPrevId}", node.PrevId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCore}", Helper.FloatToBuffer(node.Core));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrRadius}", node.Radius);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChildId0}", node.ChildId[0]);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChildId1}", node.ChildId[1]);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void LoadImgs(IProgress<string> progress)
        {
            lock (_imglock) {
                _imgList.Clear();
            }
           
            lock (_sqllock) {
                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrName}, "); // 0
                sb.Append($"{AppConsts.AttrHash}, "); // 1
                sb.Append($"{AppConsts.AttrWidth}, "); // 2
                sb.Append($"{AppConsts.AttrHeight}, "); // 3
                sb.Append($"{AppConsts.AttrSize}, "); // 4
                sb.Append($"{AppConsts.AttrDateTaken}, "); // 5
                sb.Append($"{AppConsts.AttrMetadata}, "); // 6
                sb.Append($"{AppConsts.AttrVector0}, "); // 7
                sb.Append($"{AppConsts.AttrNode0}, "); // 8
                sb.Append($"{AppConsts.AttrVector1}, "); // 9
                sb.Append($"{AppConsts.AttrNode1}, "); // 10
                sb.Append($"{AppConsts.AttrNextHash}, "); // 11
                sb.Append($"{AppConsts.AttrSim}, "); // 12
                sb.Append($"{AppConsts.AttrLastChanged}, "); // 13
                sb.Append($"{AppConsts.AttrLastView}, "); // 14
                sb.Append($"{AppConsts.AttrLastCheck}, "); // 15
                sb.Append($"{AppConsts.AttrGeneration} "); // 16
                sb.Append($"FROM {AppConsts.TableImages}");
                var sqltext = sb.ToString();
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
                            var vector0 = Helper.FloatFromBuffer((byte[])reader[7]);
                            var node0 = reader.GetInt32(8);
                            var vector1 = Helper.FloatFromBuffer((byte[])reader[9]);
                            var node1 = reader.GetInt32(10);
                            var nexthash = reader.GetString(11);
                            var sim = reader.GetFloat(12);
                            var lastchanged = reader.GetDateTime(13);
                            var lastview = reader.GetDateTime(14);
                            var lastcheck = reader.GetDateTime(15);
                            var generation = reader.GetInt32(16);

                            var img = new Img(
                                name: name,
                                hash: hash,
                                width: width,
                                height: height,
                                size: size,
                                datetaken: datetaken,
                                metadata: metadata,
                                vector0: vector0,
                                node0: node0,
                                vector1: vector1,
                                node1: node1,
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

                sb.Length = 0;
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrNodeId}, "); // 0
                sb.Append($"{AppConsts.AttrPrevId}, "); // 1
                sb.Append($"{AppConsts.AttrCore}, "); // 2
                sb.Append($"{AppConsts.AttrRadius}, "); // 3
                sb.Append($"{AppConsts.AttrChildId0}, "); // 4
                sb.Append($"{AppConsts.AttrChildId1} "); // 5
                sb.Append($"FROM {AppConsts.TableNodes}");
                sqltext = sb.ToString();
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = sqltext;
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var nodeid = reader.GetInt32(0);
                            var previd = reader.GetInt32(1);
                            var core = Helper.FloatFromBuffer((byte[])reader[2]);
                            var radius = reader.GetFloat(3);
                            var childid0 = reader.GetInt32(4);
                            var childid1 = reader.GetInt32(5);

                            var node = new Node(
                                nodeid: nodeid,
                                previd: previd,
                                core: core,
                                radius: radius,
                                childid0: childid0,
                                childid1: childid1
                               );

                            AddToMemory(node);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                dtn = DateTime.Now;
                                progress.Report($"Loading nodes ({_nodeList.Count})...");
                            }
                        }
                    }
                }
            }

            lock (_imglock) {
                if (_nodeList.Count == 0) {
                    var node0 = new Node(nodeid: 1, previd: 0, core: Array.Empty<float>(), radius: 0f, childid0: 0, childid1: 0);
                    Add(node0);
                    var node1 = new Node(nodeid: 2, previd: 0, core: Array.Empty<float>(), radius: 0f, childid0: 0, childid1: 0);
                    Add(node1);
                }
            }
        }
    }
}