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

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=30";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public void UpdateLastView(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    img.LastView = DateTime.Now;
                }
            }
        }

        public void UpdateCounter(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    img.Counter += 1;
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

        private DateTime GetMinLastCheck()
        {
            lock (_imglock) {
                var min = (_imgList.Count == 0 ?
                    DateTime.Now :
                    _imgList.Min(e => e.Value.LastCheck))
                    .AddSeconds(-1);

                return min;
            }
        }

        private string GetPrompt()
        {
            lock (_imglock) {
                var sb = new StringBuilder();
                var mc = _imgList.Min(e => e.Value.Counter);
                var cc = _imgList.Count(e => e.Value.Counter == mc);
                var md = _imgList.Where(e => e.Value.Counter == mc).Min(e => (int)e.Value.Distance);
                var cd = _imgList.Where(e => e.Value.Counter == mc).Count(e => (int)e.Value.Distance == md);
                sb.Append($"{cc}:{mc}/");
                sb.Append($"{cd}:{md}/");
                sb.Append($"{_imgList.Count}: ");
                return sb.ToString();
            }
        }

        private int GetNextToCheck()
        {
            lock (_imgList) {
                var idX = _imgList
                    .OrderBy(e => e.Value.LastCheck)
                    .FirstOrDefault()
                    .Value
                    .Id;

                return idX;
            }
        }

        private int AllocateId()
        {
            _id++;
            SqlUpdateVar(AppConsts.AttrId, _id);
            return _id;
        }
    }
}