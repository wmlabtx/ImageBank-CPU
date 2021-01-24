using System.Collections.Generic;
using System.Data.SqlClient;

namespace ImageBank
{
    public partial class ImgMdf
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static object _sqllock = new object();
        private static SqlConnection _sqlConnection;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private object _imglock = new object();
        private readonly SortedDictionary<string, Img> _imgList = new SortedDictionary<string, Img>();
        private readonly SortedDictionary<string, Img> _hashList = new SortedDictionary<string, Img>();

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=60";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }
    }
}