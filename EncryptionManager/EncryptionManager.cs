using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ClientSideChatApp
{
    internal static class EncryptionManager
    {
        private static readonly byte[] _secretKey = Encoding.UTF8.GetBytes("ISolemnlySwearThatIAmUpToNoGood.");

        private static readonly byte[] _iv = new byte[16];

        public static string EncryptMessage(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _secretKey;

                aes.IV = _iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))

                    using (StreamWriter sw = new StreamWriter(cs))
                    {

                        sw.Write(plainText);

                    }
                    return Convert.ToBase64String(ms.ToArray());

                }
            }
        }

        public static string DecryptMessage(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _secretKey;

                aes.IV = _iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))

                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))

                using (StreamReader sr = new StreamReader(cs))
                {

                    return sr.ReadToEnd();

                }
            }
        }
    }
}