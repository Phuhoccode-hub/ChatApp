using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace SecureChatTCP_Client.Security
{
    class AESHelper
    {
        // 32 byte = AES-256
        private static readonly string Key = "12345678901234567890123456789012";
        // 16 byte
        private static readonly string IV = "1234567890123456";

        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor =aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs =new CryptoStream(ms,encryptor,CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw =new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        public static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform decryptor =aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] cipher =Convert.FromBase64String(cipherText);
                using (MemoryStream ms =new MemoryStream(cipher))
                {
                    using (CryptoStream cs =new CryptoStream(ms,decryptor,CryptoStreamMode.Read))
                    {
                        using (StreamReader sr =new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static byte[] Encrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms,aes.CreateEncryptor(),CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }
        public static byte[] Decrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream input = new MemoryStream(data))
                using (CryptoStream cs = new CryptoStream(input,aes.CreateDecryptor(),CryptoStreamMode.Read))
                using (MemoryStream output = new MemoryStream())
                {
                    cs.CopyTo(output);
                    return output.ToArray();
                }
            }
        }
    }


}
