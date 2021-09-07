﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static readonly object _sqllock = new object();
        private static SqlConnection _sqlConnection;
        private static readonly object _imglock = new object();
        private static readonly SortedDictionary<string, Img> _imgList = new SortedDictionary<string, Img>(StringComparer.OrdinalIgnoreCase);
        private static readonly SortedDictionary<string, Img> _hashList = new SortedDictionary<string, Img>(StringComparer.OrdinalIgnoreCase);
        private static readonly SortedDictionary<int, Node> _nodeList = new SortedDictionary<int, Node>();

        private static readonly object _rwlock = new object();
        private static List<FileInfo> _rwList = new List<FileInfo>();

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=300";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public static int GetLiveNodesCount()
        {
            lock (_imglock) {
                return _nodeList.Count(e => e.Value.Core != null);
            }
        }

        public static DateTime GetMinLastView()
        {
            lock (_imglock)
            {
                if (_imgList.Count == 0)
                {
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
                return _imgList.Count == 0 ? DateTime.Now : _imgList
                    .Min(e => e.Value.LastCheck)
                    .AddSeconds(-1);
            }
        }

        public static int GetGenerationSize(int generation)
        {
            lock (_imglock) {
                return _imgList.Count == 0 ? 0 : _imgList.Count(e => e.Value.Generation == generation);
            }
        }
    }
}