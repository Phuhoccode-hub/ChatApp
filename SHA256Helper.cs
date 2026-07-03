using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SecureChatTCP_Client.Security
{
    public class SHA256Helper
    {
        public static string Hash(string text)
        {
            SHA256 sha = SHA256.Create();

            byte[] bytes =Encoding.UTF8.GetBytes(text);

            byte[] hash =sha.ComputeHash(bytes);

            StringBuilder sb =new StringBuilder();

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}