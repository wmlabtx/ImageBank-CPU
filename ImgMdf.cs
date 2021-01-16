using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static object _sqllock = new object();
        private static SqlConnection _sqlConnection;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private object _imglock = new object();
        private readonly SortedList<string, Img> _imgList = new SortedList<string, Img>();
        private readonly SortedList<string, string> _hashList = new SortedList<string, string>();

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=60";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        private string GetPrompt()
        {
            lock (_imglock) {
                var sb = new StringBuilder();
                var maxfolder = _imgList.Count == 0 ? 0 : _imgList.Max(e => e.Value.Folder);
                var maxfoldercount = _imgList.Count(e => e.Value.Folder == maxfolder);
                var recent = _imgList.Count(e => 
                    DateTime.Now.Subtract(e.Value.LastAdded).TotalDays < 1 &&
                    DateTime.Now.Subtract(e.Value.LastView).TotalDays > 3000
                    );
                sb.Append($"{maxfolder}:{maxfoldercount}/({recent})/{_imgList.Count}: ");
                return sb.ToString();
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

        public int GetMaxFolder()
        {
            lock (_imglock) {
                var max = (_imgList.Count == 0 ?
                    0 :
                    _imgList.Max(e => e.Value.Folder) - 1);
                max = Math.Max(0, max);
                return max;
            }
        }
    }
}