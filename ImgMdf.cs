using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
                var sb = new StringBuilder();
                var count = _imgList.Count(e => e.Value.Generation == 0);
                if (count > 0) {
                    sb.Append($"g{count}/");
                }
                
                count = _imgList.Count(e => e.Value.LastView < e.Value.LastChange);
                if (count > 0) {
                    sb.Append($"c{count}/");
                }

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

                int id;
                var scope = _imgList
                        .Values
                        .Where(e => e.LastId < _id)
                        .ToArray();

                if (scope.Length == 0) {
                    scope = _imgList
                            .Values
                            .ToArray();

                    id = scope.Aggregate((m, e) => e.LastFind < m.LastFind ? e : m).Id;
                    return id;
                }

                id = scope.Aggregate((m, e) => e.LastId < m.LastId ? e : m).Id;
                return id;
            }
        }

        private int AllocateId()
        {
            _id++;
            SqlUpdateVar(AppConsts.AttrId, _id);
            return _id;
        }

        private string GetSuggestedName(string prefixname, string checksum)
        {
            string suggestedname;
            string suggestedfilename;
            var namelenght = 2;
            lock (_imglock) {
                do {
                    namelenght++;
                    suggestedname = string.Concat(prefixname, checksum.Substring(0, namelenght));
                    suggestedfilename = Helper.GetFileName(suggestedname);
                } while (File.Exists(suggestedfilename));
            }

            return suggestedname;
        }
    }
}