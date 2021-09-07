using System.Text;
using System.Data.SqlClient;
using System;

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
                    sb.Append($"{AppConsts.AttrDateTaken}, ");
                    sb.Append($"{AppConsts.AttrFamily}, ");
                    sb.Append($"{AppConsts.AttrBestNames}, ");
                    sb.Append($"{AppConsts.AttrLastChanged}, ");
                    sb.Append($"{AppConsts.AttrLastView}, ");
                    sb.Append($"{AppConsts.AttrLastCheck}, ");
                    sb.Append($"{AppConsts.AttrGeneration} ");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrName}, ");
                    sb.Append($"@{AppConsts.AttrHash}, ");
                    sb.Append($"@{AppConsts.AttrDateTaken}, ");
                    sb.Append($"@{AppConsts.AttrFamily}, ");
                    sb.Append($"@{AppConsts.AttrBestNames}, ");
                    sb.Append($"@{AppConsts.AttrLastChanged}, ");
                    sb.Append($"@{AppConsts.AttrLastView}, ");
                    sb.Append($"@{AppConsts.AttrLastCheck}, ");
                    sb.Append($"@{AppConsts.AttrGeneration}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", img.Name);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrHash}", img.Hash);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDateTaken}", img.DateTaken ?? new DateTime(1980, 1, 1));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrFamily}", img.Family);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrBestNames}", img.BestNames);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastChanged}", img.LastChanged);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastView}", img.LastView);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastCheck}", img.LastCheck);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrGeneration}", img.Generation);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlAddNode(int nodeid, Node node)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableNodes} (");
                    sb.Append($"{AppConsts.AttrNodeId}, ");
                    sb.Append($"{AppConsts.AttrCore}, ");
                    sb.Append($"{AppConsts.AttrDepth}, ");
                    sb.Append($"{AppConsts.AttrChildId}, ");
                    sb.Append($"{AppConsts.AttrMembers}, ");
                    sb.Append($"{AppConsts.AttrLastAdded} ");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrNodeId}, ");
                    sb.Append($"@{AppConsts.AttrCore}, ");
                    sb.Append($"@{AppConsts.AttrDepth}, ");
                    sb.Append($"@{AppConsts.AttrChildId}, ");
                    sb.Append($"@{AppConsts.AttrMembers}, ");
                    sb.Append($"@{AppConsts.AttrLastAdded}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCore}", Helper.ArrayFromMat(node.Core));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDepth}", node.Depth);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChildId}", node.ChildId);
                    var members = node.GetMembers();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrMembers}", members);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastAdded}", node.LastAdded);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SqlUpdateNode(int nodeid, Node node)
        {
            lock (_sqllock) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        var sb = new StringBuilder();
                        sb.Append($"UPDATE {AppConsts.TableNodes} SET ");
                        sb.Append($"{AppConsts.AttrCore} = @{AppConsts.AttrCore}, ");
                        sb.Append($"{AppConsts.AttrDepth} = @{AppConsts.AttrDepth}, ");
                        sb.Append($"{AppConsts.AttrChildId} = @{AppConsts.AttrChildId}, ");
                        sb.Append($"{AppConsts.AttrMembers} = @{AppConsts.AttrMembers}, ");
                        sb.Append($"{AppConsts.AttrLastAdded} = @{AppConsts.AttrLastAdded} ");
                        sb.Append($"WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}");
                        sqlCommand.CommandText = sb.ToString();
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCore}", Helper.ArrayFromMat(node.Core));
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDepth}", node.Depth);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChildId}", node.ChildId);
                        var members = node.GetMembers();
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrMembers}", members);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrLastAdded}", node.LastAdded);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                }
            }
        }

        public static void LoadImgs(IProgress<string> progress)
        {
            lock (_imglock) {
                _imgList.Clear();

                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrName}, "); // 0
                sb.Append($"{AppConsts.AttrHash}, "); // 1
                sb.Append($"{AppConsts.AttrDateTaken}, "); // 2
                sb.Append($"{AppConsts.AttrFamily}, "); // 3
                sb.Append($"{AppConsts.AttrBestNames}, "); // 4
                sb.Append($"{AppConsts.AttrLastChanged}, "); // 5
                sb.Append($"{AppConsts.AttrLastView}, "); // 6
                sb.Append($"{AppConsts.AttrLastCheck}, "); // 7
                sb.Append($"{AppConsts.AttrGeneration} "); // 8
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
                                var dt = reader.GetDateTime(2);
                                DateTime? datetaken = null;
                                if (dt.Year > 1980) {
                                    datetaken = dt;
                                }

                                var family = reader.GetInt32(3);
                                var bestnames = reader.GetString(4);
                                var lastchanged = reader.GetDateTime(5);
                                var lastview = reader.GetDateTime(6);
                                var lastcheck = reader.GetDateTime(7);
                                var generation = reader.GetInt32(8);

                                var img = new Img(
                                    name: name,
                                    hash: hash,
                                    datetaken: datetaken,
                                    family: family,
                                    bestnames: bestnames,
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

                    if (progress != null) {
                        progress.Report("Database loaded");
                    }
                }
            }
        }

        public static void LoadNodes(IProgress<string> progress)
        {
            lock (_imglock) {
                _nodeList.Clear();

                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrNodeId}, "); // 0
                sb.Append($"{AppConsts.AttrCore}, "); // 1
                sb.Append($"{AppConsts.AttrDepth}, "); // 2
                sb.Append($"{AppConsts.AttrChildId}, "); // 3
                sb.Append($"{AppConsts.AttrMembers}, "); // 4
                sb.Append($"{AppConsts.AttrLastAdded} "); // 5
                sb.Append($"FROM {AppConsts.TableNodes}");
                var sqltext = sb.ToString();
                lock (_sqllock) {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = sqltext;
                        using (var reader = sqlCommand.ExecuteReader()) {
                            var dtn = DateTime.Now;
                            while (reader.Read()) {
                                var nodeid = reader.GetInt32(0);
                                var core = Helper.ArrayToMat((byte[])reader[1]);
                                var depth = reader.GetInt32(2);
                                var childid = reader.GetInt32(3);
                                var members = reader.GetString(4);
                                var lastadded = reader.GetDateTime(5);
                                var node = new Node(
                                    core: core,
                                    depth: depth,
                                    childid: childid,
                                    members: members,
                                    lastadded: lastadded
                                   );

                                _nodeList.Add(nodeid, node);

                                if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                    dtn = DateTime.Now;
                                    if (progress != null) {
                                        progress.Report($"Loading nodes ({_nodeList.Count})...");
                                    }
                                }
                            }
                        }
                    }

                    if (progress != null) {
                        progress.Report("Nodes loaded");
                    }
                }
            }
        }
    }
}