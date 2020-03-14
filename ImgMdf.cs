using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private static object _sqllock = new object();
        private static SqlConnection _sqlConnection;

        private object _imglock = new object();
        private readonly SortedDictionary<int, Img> _imgList = new SortedDictionary<int, Img>();

        private int _id;
        private int _family;

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=30";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public void UpdateHistory(int idx, int idy)
        {
            if (idx == idy) {
                return;
            }

            lock (_imglock) {
                if (_imgList.TryGetValue(idx, out var imgX)) {
                    imgX.AddToHistory(idy);
                    imgX.LastId = 0;
                }
            }
        }

        public void UpdateLastView(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    img.LastView = DateTime.Now;
                }
            }
        }

        private DateTime GetMinLastView()
        {
            lock (_imglock) {
                var min = (_imgList.Count == 0 ?
                    DateTime.Now :
                    _imgList.Min(e => e.Value.LastView))
                    .AddSeconds(-1);

                return min;
            }
        }

        private string GetPrompt()
        {
            lock (_imglock) {
                var sb = new StringBuilder();
                var mingeneration = _imgList.Min(e => e.Value.Generation);
                var count = _imgList.Count(e => e.Value.Generation == mingeneration);
                sb.Append($"{mingeneration}:{count}/");
                count = _imgList.Count;
                sb.Append($"{count}: ");
                return sb.ToString();
            }
        }

        private int GetNextToCheck()
        {
            lock (_imgList) {
                if (_imgList.Count == 0) {
                    return 0;
                }

                var scope = _imgList
                        .Values
                        .Where(e => e.LastId < _id)
                        .ToArray();

                if (scope.Length == 0) {
                    return 0;
                }

                return scope.Aggregate((m, e) => e.LastId < m.LastId ? e : m).Id;
            }
        }

        private int GetFamilySize(int family)
        {
            if (family == 0) {
                return 0;
            }

            lock (_imglock) {
                return _imgList.Count(e => e.Value.Family == family);
            }
        }

        private int AllocateId()
        {
            _id++;
            SqlUpdateVar(AppConsts.AttrId, _id);
            return _id;
        }

        private int AllocateFamily()
        {
            _family++;
            SqlUpdateVar(AppConsts.AttrFamily, _family);
            return _family;
        }
    }
}