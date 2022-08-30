using System.Collections.Generic;
using System.Data.SqlClient;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        private const int SIMMAX = 27;

        private static int _id;
        private static int _importLimit;

        private static readonly object _sqllock = new object();
        private static readonly SqlConnection _sqlConnection;
        private static readonly SortedList<int, Img> _imgList = new SortedList<int, Img>();
        private static readonly SortedList<string, Img> _nameList = new SortedList<string, Img>();
        private static readonly SortedList<string, Img> _hashList = new SortedList<string, Img>();
        private static readonly List<float[]> _lastviewed = new List<float[]>();
        private static readonly CryptoRandom _random = new CryptoRandom();

        public static readonly SortedList<int, string> BinsList = new SortedList<int, string>();

        private static int _sv = 0;

        static ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=300";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public static float[] GetPalette()
        {
            return _palette;
        }

        private static int AllocateId()
        {
            _id++;
            SqlVarsUpdateProperty(AppConsts.AttributeId, _id);
            return _id;
        }

        private static void DecreaseImportLimit()
        {
            _importLimit -= 10;
            SqlVarsUpdateProperty(AppConsts.AttributeImportLimit, _importLimit);
        }
    }
}