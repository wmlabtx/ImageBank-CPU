using System.Security.Cryptography;
using System.Text;

namespace ImageBank
{
    public static class HashHelper
    {
        public static string Compute(byte[] array)
        {
            if (array == null) {
                return null;
            }

            using (var sha256 = SHA256.Create()) {
                var hash = sha256.ComputeHash(array);
                var sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++) {
                    sb.Append(hash[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
