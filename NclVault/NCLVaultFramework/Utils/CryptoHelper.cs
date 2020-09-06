using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NclVaultFramework.Utils
{
    public class CryptoHelper
    {
        /// <summary>
        /// Computes SHA256 hash of a string
        /// </summary>
        /// <param name="rawData">String to hash</param>
        /// <returns>Hash string</returns>
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// https://mikaelkoskinen.net/post/encrypt-decrypt-string-asp-net-core
        /// </summary>
        /// <param name="text"></param>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static string EncryptString(string text, string keyString)
        {
            /* Adapts the password lenght */
            keyString = (keyString.Length > 32) ? keyString.Substring(0, 32) : keyString.PadLeft(32, '*');
            
            var key = Encoding.UTF8.GetBytes(keyString);
            
           
            using (var aesAlg = Aes.Create())
            {
                /*vBYTE_GeneratedIV = new byte[aesAlg.IV.Length];

                Array.Copy(aesAlg.IV, vBYTE_GeneratedIV, aesAlg.IV.Length);*/
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        /// <summary>
        /// https://mikaelkoskinen.net/post/encrypt-decrypt-string-asp-net-core
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string keyString)
        {
            /* Adapts the password lenght */
            keyString = (keyString.Length > 32) ? keyString.Substring(0, 32) : keyString.PadLeft(32, '*');

            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }
    }
}
