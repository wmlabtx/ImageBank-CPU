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

        private string GetPrompt()
        {
            lock (_imglock) {
                var sb = new StringBuilder();
                var mincounter = _imgList.Min(e => e.Value.Counter);
                var count = _imgList.Count(e => e.Value.Counter == mincounter);
                sb.Append($"{mincounter}:{count}/");
                count = _imgList.Count;
                sb.Append($"{count}: ");
                return sb.ToString();
            }
        }

        private int GetNextToCheck()
        {
            lock (_imgList) {
                var idX = 0;
                var xcounter = int.MaxValue;
                var lc = DateTime.MaxValue;
                foreach (var e in _imgList) {
                    if (e.Value.Counter < xcounter) {
                        idX = e.Value.Id;
                        xcounter = e.Value.Counter;
                        lc = e.Value.LastCheck;
                    }
                    else {
                        if (e.Value.Counter == xcounter) {
                            if (e.Value.LastCheck < lc) {
                                idX = e.Value.Id;
                                lc = e.Value.LastCheck;
                            }
                        }
                    }
                }

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