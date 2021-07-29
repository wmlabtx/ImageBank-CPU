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
        private static SqlConnection _sqlConnection;
        private static readonly object _imglock = new object();
        private static readonly SortedDictionary<string, Img> _imgList = new SortedDictionary<string, Img>(StringComparer.OrdinalIgnoreCase);
        private static readonly SortedDictionary<string, Img> _hashList = new SortedDictionary<string, Img>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _rwlock = new object();
        private static List<FileInfo> _rwList = new List<FileInfo>();
        private static readonly CryptoRandom _random = new CryptoRandom();

        public static readonly SortedDictionary<int, string> Family = new SortedDictionary<int, string>();

        public ImgMdf()
        {
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=60";
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();

            Family.Add(0, "Reset");
            Family.Add(1, "Edna");
            Family.Add(2, "Anette");
            Family.Add(3, "Dennis+Elley");
            Family.Add(4, "Janet");
            Family.Add(5, "Monique");
            Family.Add(6, "Patty+Pam");
            Family.Add(7, "Sophia+Luigi");
            Family.Add(8, "Yvonne+Peter");
            Family.Add(9, "Alexandra");
            Family.Add(10, "Chertenok");
            Family.Add(11, "Emely");
            Family.Add(12, "Sandra");
            Family.Add(13, "Gerda");
            Family.Add(14, "Mandy");
            Family.Add(15, "Alina Latypova");
            Family.Add(16, "Nami");
            Family.Add(17, "Janne");
            Family.Add(18, "Fatima+Rashit");
            Family.Add(19, "Judy");
            Family.Add(20, "Agness");
            Family.Add(21, "Masha Allen");
            Family.Add(22, "Carl's party");
            Family.Add(23, "Maisy");
            Family.Add(24, "Tara");
            Family.Add(25, "Scar girl");
            Family.Add(26, "Tilly");
            Family.Add(27, "Ellen");
            Family.Add(28, "Kate");
            Family.Add(29, "Herda+Brother");
            Family.Add(30, "Alex");
            Family.Add(31, "Alica");
            Family.Add(32, "Helen");
            Family.Add(33, "Cecile");
            Family.Add(34, "Inga");
            Family.Add(35, "Marietta");
            Family.Add(36, "Rika Nishimura");
            Family.Add(37, "Nozomi Kurahashi");
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