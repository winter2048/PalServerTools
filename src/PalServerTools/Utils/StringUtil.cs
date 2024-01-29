using System.Security.Cryptography;
using System.Text;

namespace PalServerTools.Utils
{
    public static class StringUtil
    {
        public static string ComputeMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                
                return stringBuilder.ToString();
            }
        }
    }
}
