using System.Security.Cryptography;
using System.Text;

namespace ImageBank
{
    public static class MD5HashHelper
    {
        public static string Compute(byte[] array)
        {
            if (array == null) {
                return null;
            }

            using (var md5 = MD5.Create()) {
                var hashmd5 = md5.ComputeHash(array);
                var sb = new StringBuilder();
                for (var i = 0; i < 16; i++) {
                    sb.Append(hashmd5[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
