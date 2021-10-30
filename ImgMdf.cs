using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static readonly object _sqllock = new object();
        private static readonly SqlConnection _sqlConnection;
        private static readonly object _imglock = new object();
        private static readonly SortedDictionary<int, Img> _imgList = new SortedDictionary<int, Img>();
        private static readonly SortedDictionary<string, Img> _nameList = new SortedDictionary<string, Img>();
        private static readonly SortedDictionary<string, Img> _hashList = new SortedDictionary<string, Img>();

        private static readonly object _rwlock = new object();
        private static List<FileInfo> _rwList = new List<FileInfo>();

        private static readonly SortedDictionary<int, SiftNode> _nodesList = new SortedDictionary<int, SiftNode>();
        private static object _nodesLock = new object();

        private static int _id;
        private static int _importLimit;

        static ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=300";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        private static int AllocateId()
        {
            _id++;
            SqlUpdateVar(AppConsts.AttrId, _id);
            return _id;
        }

        private static int DecreaseImportLimit()
        {
            _importLimit -= 10;
            SqlUpdateVar(AppConsts.AttrImportLimit, _importLimit);
            return _id;
        }

        public static DateTime GetMinLastView()
        {
            lock (_imglock)
            {
                if (_imgList.Count == 0) {
                    return DateTime.Now;
                }

                var scope = _imgList.ToArray();
                if (scope.Length == 0) {
                    return DateTime.Now;
                }

                return scope 
                    .Min(e => e.Value.LastView)
                    .AddSeconds(-1);
            }
        }

        public static DateTime GetMinLastCheck()
        {
            lock (_imglock) {
                if (_imgList.Count == 0) {
                    return DateTime.Now;
                }

                var scope = _imgList.ToArray();
                if (scope.Length == 0) {
                    return DateTime.Now;
                }

                return scope
                    .Min(e => e.Value.LastCheck)
                    .AddSeconds(-1);
            }
        }
    }
}