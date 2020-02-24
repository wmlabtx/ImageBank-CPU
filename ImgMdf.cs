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
        private readonly SortedDictionary<string, Img> _nameList = new SortedDictionary<string, Img>(StringComparer.OrdinalIgnoreCase);
        private readonly SortedDictionary<string, Img> _checksumList = new SortedDictionary<string, Img>(StringComparer.OrdinalIgnoreCase);

        private int _id;

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=30";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        private bool GetPairToCompare(out int idX, out int idY)
        {
            idX = -1;
            idY = -1;
            lock (_imglock) {
                var scopetoview = _imgList
                    .Values
                    .Where(e => e.LastId >= 0)
                    .ToArray();

                if (scopetoview.Length == 0) {
                    return false;
                }

                var mingeneration = scopetoview.Min(e => e.Generation);
                scopetoview = scopetoview
                    .Where(e => e.Generation == mingeneration)
                    .ToArray();

                long min = long.MaxValue;
                foreach (var img in scopetoview) {
                    if (_imgList.TryGetValue(img.NextId, out var imgY)) {
                        var mint = img.LastView.Ticks + imgY.LastView.Ticks;
                        if (mint < min) {
                            min = mint;
                            idX = img.Id;
                            idY = imgY.Id;
                        }
                    }
                }
            }

            if (idX < 0 || idY < 0) {
                return false;
            }

            return true;
        }

        public void UpdateGeneration(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    img.Generation++;
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
                var counters = new SortedDictionary<int, int>();
                var scope = _imgList
                    .Values
                    .ToArray();

                foreach (var img in scope) {
                    if (counters.ContainsKey(img.Generation)) {
                        counters[img.Generation]++;
                    }
                    else {
                        counters.Add(img.Generation, 1);
                    }
                }

                var sb = new StringBuilder();
                var generations = counters.Keys.ToArray();
                for (var i = generations.Length - 1; i >= 0; i--) {
                    if (sb.Length > 0) {
                        sb.Append('/');
                    }

                    sb.Append($"g{generations[i]}:{counters[generations[i]]}");
                }

                sb.Append($"/{_imgList.Count}");
                sb.Append(": ");
                return sb.ToString();
            }
        }

        private int GetNextToCheck()
        {
            lock (_imgList) {
                if (_imgList.Count == 0) {
                    return -1;
                }

                var scopetocheck = _imgList
                    .Values
                    .Where(e => !_imgList.ContainsKey(e.NextId))
                    .ToArray();

                if (scopetocheck.Length == 0) {
                    scopetocheck = _imgList
                        .Values
                        .Where(e => e.LastId < _id)
                        .ToArray();
                }

                if (scopetocheck.Length == 0) {
                    return -1;
                }

                var id = scopetocheck.Aggregate((m, e) => e.LastId < m.LastId ? e : m).Id;
                return id;
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