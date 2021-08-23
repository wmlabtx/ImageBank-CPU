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
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKi}", Helper.ArrayFrom32(img.Vector[0]));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrKiMirror}", Helper.ArrayFrom32(img.Vector[1]));
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
                            var ki = Helper.ArrayTo32((byte[])reader[7]);
                            var kimirror = Helper.ArrayTo32((byte[])reader[8]);
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
                                kimirror: kimirror,
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

                var count = SqlGetNodesCount();
                if (count == 0) {
                    var rootnode = new Node(1);
                    SqlAddNode(rootnode);
                }

                progress.Report("Database loaded");
            }
        }

        public static void SqlAddNode(Node node)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableNodes} (");
                    sb.Append($"{AppConsts.AttrNodeId},");
                    sb.Append($"{AppConsts.AttrCore},");
                    sb.Append($"{AppConsts.AttrRadius},");
                    sb.Append($"{AppConsts.AttrChildId}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrNodeId},");
                    sb.Append($"@{AppConsts.AttrCore},");
                    sb.Append($"@{AppConsts.AttrRadius},");
                    sb.Append($"@{AppConsts.AttrChildId}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", node.NodeId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCore}", Helper.ArrayFrom64(node.Core));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrRadius}", node.Radius);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChildId}", node.ChildId);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void SqlUpdateNode(Node node)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"UPDATE {AppConsts.TableNodes} SET ");
                    sb.Append($"{AppConsts.AttrCore} = @{AppConsts.AttrCore}, ");
                    sb.Append($"{AppConsts.AttrRadius} = @{AppConsts.AttrRadius}, ");
                    sb.Append($"{AppConsts.AttrChildId} = @{AppConsts.AttrChildId} ");
                    sb.Append($"WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}");
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrCore}", Helper.ArrayFrom64(node.Core));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrRadius}", node.Radius);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrChildId}", node.ChildId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", node.NodeId);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static Node SqlGetNode(int nodeid)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append("SELECT ");
                    sb.Append($"{AppConsts.AttrCore}, "); // 0
                    sb.Append($"{AppConsts.AttrRadius}, "); // 1
                    sb.Append($"{AppConsts.AttrChildId} "); // 2
                    sb.Append($"FROM {AppConsts.TableNodes} ");
                    sb.Append($"WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}");
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
                    sqlCommand.CommandText = sb.ToString();
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var result = reader.Read();
                        if (!result) {
                            return null;
                        }

                        var core = Helper.ArrayTo64((byte[])reader[0]);
                        var radius = reader.GetInt32(1);
                        var childid = reader.GetInt32(2);
                        var node = new Node(nodeid, core, radius, childid);
                        return node;
                    }
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

            var rootnode = new Node(1);
            SqlAddNode(rootnode);
        }

        public static int SqlGetNodesCount()
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"SELECT COUNT(*) FROM {AppConsts.TableNodes}";
                    var count = (int)sqlCommand.ExecuteScalar();
                    return count;
                }
            }
        }

        public static int SqlGetAvailableNodeId()
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"SELECT MAX({AppConsts.AttrNodeId}) FROM {AppConsts.TableNodes}";
                    var maxnodeid = (int)sqlCommand.ExecuteScalar();
                    return maxnodeid + 1;
                }
            }
        }

        public static int SqlFindNodeId(ulong[] vector)
        {
            /*
            var descriptorid = Helper.ComputeDescriptorId(vector);
            var existingdescriptor = SqlGetDescriptor(descriptorid);
            if (existingdescriptor == null) {
                var node = SqlFindNode(vector);
                return node.NodeId;
            }
            else {
                return existingdescriptor.NodeId;
            }
            */

            var node = SqlFindNode(vector);
            return node.NodeId;
        }

        public static Node SqlFindNode(ulong[] vector)
        {
            var nodeid = 1;
            Node node;
            do {
                node = SqlGetNode(nodeid);
                if (node.ChildId == 0) {
                    break;
                }

                var distance = Helper.GetDistance(node.Core, 0, vector, 0);
                if (distance <= node.Radius) {
                    nodeid = node.ChildId;
                }
                else {
                    nodeid = node.ChildId + 1;
                }
            }
            while (true);
            return node;
        }

        public static void SqlAddDescriptor(Descriptor descriptor)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append($"INSERT INTO {AppConsts.TableDescriptors} (");
                    sb.Append($"{AppConsts.AttrDescriptorId},");
                    sb.Append($"{AppConsts.AttrVector},");
                    sb.Append($"{AppConsts.AttrNodeId}");
                    sb.Append(") VALUES (");
                    sb.Append($"@{AppConsts.AttrDescriptorId},");
                    sb.Append($"@{AppConsts.AttrVector},");
                    sb.Append($"@{AppConsts.AttrNodeId}");
                    sb.Append(')');
                    sqlCommand.CommandText = sb.ToString();
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDescriptorId}", descriptor.DescriptorId);
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrVector}", Helper.ArrayFrom64(descriptor.Vector));
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", descriptor.NodeId);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void SqlDescriptorsUpdateProperty(long descriptorid, string key, object val)
        {
            lock (_sqllock) {
                try {
                    using (var sqlCommand = _sqlConnection.CreateCommand()) {
                        sqlCommand.Connection = _sqlConnection;
                        sqlCommand.CommandText = $"UPDATE {AppConsts.TableDescriptors} SET {key} = @{key} WHERE {AppConsts.AttrDescriptorId} = @{AppConsts.AttrDescriptorId}";
                        sqlCommand.Parameters.AddWithValue($"@{key}", val);
                        sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDescriptorId}", descriptorid);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SqlException) {
                }
            }
        }

        public static void SqlTruncateDescriptors()
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"TRUNCATE TABLE {AppConsts.TableDescriptors}";
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static List<Descriptor> SqlGetDescriptors(int nodeid)
        {
            var result = new List<Descriptor>();
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append("SELECT ");
                    sb.Append($"{AppConsts.AttrDescriptorId}, "); // 0
                    sb.Append($"{AppConsts.AttrVector} "); // 1
                    sb.Append($"FROM {AppConsts.TableDescriptors} ");
                    sb.Append($"WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}");
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
                    sqlCommand.CommandText = sb.ToString();
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            var descriptorid = reader.GetInt64(0);
                            var vector = Helper.ArrayTo64((byte[])reader[1]);
                            var descriptor = new Descriptor(descriptorid, vector, nodeid);
                            result.Add(descriptor);
                        }
                    }
                }
            }

            return result;
        }

        public static int SqlGetDescriptorsCount(int nodeid)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"SELECT COUNT(*) FROM {AppConsts.TableDescriptors} WHERE {AppConsts.AttrNodeId} = @{AppConsts.AttrNodeId}";
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrNodeId}", nodeid);
                    var count = (int)sqlCommand.ExecuteScalar();
                    return count;
                }
            }
        }

        public static int SqlGetDescriptorsCount()
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    sqlCommand.CommandText = $"SELECT COUNT(*) FROM {AppConsts.TableDescriptors}";
                    var count = (int)sqlCommand.ExecuteScalar();
                    return count;
                }
            }
        }

        public static Descriptor SqlGetDescriptor(long descriptorid)
        {
            lock (_sqllock) {
                using (var sqlCommand = _sqlConnection.CreateCommand()) {
                    sqlCommand.Connection = _sqlConnection;
                    var sb = new StringBuilder();
                    sb.Append("SELECT ");
                    sb.Append($"{AppConsts.AttrVector}, "); // 0
                    sb.Append($"{AppConsts.AttrNodeId} "); // 1
                    sb.Append($"FROM {AppConsts.TableDescriptors} ");
                    sb.Append($"WHERE {AppConsts.AttrDescriptorId} = @{AppConsts.AttrDescriptorId}");
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrDescriptorId}", descriptorid);
                    sqlCommand.CommandText = sb.ToString();
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var result = reader.Read();
                        if (!result) {
                            return null;
                        }

                        var vector = Helper.ArrayTo64((byte[])reader[0]);
                        var nodeid = reader.GetInt32(1);
                        var descriptor = new Descriptor(descriptorid, vector, nodeid);
                        return descriptor;
                    }
                }
            }
        }

        public static void PopulateDescriptor(Descriptor descriptor)
        {
            SqlAddDescriptor(descriptor);

            var node = SqlFindNode(descriptor.Vector);
            descriptor.NodeId = node.NodeId;

            var memberscount = SqlGetDescriptorsCount(node.NodeId);
            if (memberscount <= AppConsts.MaxNodeDescriptors) {
                return;
            }

            var members = SqlGetDescriptors(node.NodeId);
            var core = new ulong[4];
            var maxdistance = 0;
            for (var ox = 0; ox < members.Count - 1; ox++) {
                for (var oy = ox + 1; oy < members.Count; oy++) {
                    var distance = Helper.GetDistance(members[ox].Vector, 0, members[oy].Vector, 0);
                    if (distance > maxdistance) {
                        maxdistance = distance;
                        Array.Copy(members[ox].Vector, 0, core, 0, 4);
                    }
                }
            }

            var distances = new int[members.Count];
            for (var ox = 0; ox < members.Count; ox++) {
                var distance = Helper.GetDistance(members[ox].Vector, 0, core, 0);
                distances[ox] = distance;
            }

            var sorteddistances = distances.OrderBy(e => e).ToArray();
            var meanindex = members.Count / 2;
            var radius = sorteddistances[meanindex];
            var childid = SqlGetAvailableNodeId();
            var node0 = new Node(node.NodeId, core, radius, childid);
            var node1 = new Node(childid);
            var node2 = new Node(childid + 1);
            SqlUpdateNode(node0);
            SqlAddNode(node1);
            SqlAddNode(node2);

            for (var ox = 0; ox < members.Count; ox++) {
                var distance = distances[ox];
                members[ox].NodeId = distance <= radius ? node1.NodeId : node2.NodeId;
            }
        }

        public static void SqlPopulateDescriptors(ulong[] vectors)
        {
            var offset = 0;
            var vector = new ulong[4];
            while (offset < vectors.Length) {
                Array.Copy(vectors, offset, vector, 0, 4);
                var descriptorid = Helper.ComputeDescriptorId(vector);
                var existingdescriptor = SqlGetDescriptor(descriptorid);
                if (existingdescriptor == null) {
                    var descriptor = new Descriptor(descriptorid, vector, 0);
                    PopulateDescriptor(descriptor);
                }

                offset += 4;
            }
        }

        public static void SqlGetFeatures(ulong[] vectors, out int[] features)
        {
            var n = vectors.Length / 4;
            features = new int[n];
            var vector = new ulong[4];
            for (var offset = 0; offset < vectors.Length; offset += 4) {
                Array.Copy(vectors, offset, vector, 0, 4);
                var nodeid = SqlFindNodeId(vector);
                var index = offset / 4;
                features[index] = nodeid;
            }

            features = features.OrderBy(e => e).ToArray();
        }

        public static void SqlGetFeatures(ulong[][] descriptors, out int[][] features)
        {
            features = new int[2][];
            for (var i = 0; i < 2; i++) {
                SqlGetFeatures(descriptors[i], out int[] f);
                features[i] = f;
            }
        }
    }
}