using ncl.net.cryptolybrary.Encryption.AES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NclVaultFramework.Controllers
{
    /// <summary>
    /// https://docs.microsoft.com/it-it/dotnet/api/system.security.cryptography.protecteddata?view=dotnet-plat-ext-3.1
    /// </summary>
    public class ProtectDataManager
    {
        // Create byte array for additional entropy when using Protect method.
        static readonly byte[] s_additionalEntropy = { 0xC0, 0xFF, 0xEE, 0xC0, 0xFF, 0xEE };

        public static byte[] Protect(byte[] data, string STRING_Key)
        {
            try
            {
                byte[] vByteEncryptedData = AesProvider.AES_CBC_Encryption_Rand_IV(data, STRING_Key);
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
                return ProtectedData.Protect(vByteEncryptedData, s_additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static string Unprotect(string STRING_ProtectedKey, string STRING_Key)
        {
            try
            {
                byte[] protectedKey = Convert.FromBase64String(File.ReadAllText(STRING_ProtectedKey));
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return Encoding.UTF8.GetString(AesProvider.AES_CBC_Decryption_Rand_IV(ProtectedData.Unprotect(protectedKey,
                                                                                                              s_additionalEntropy,
                                                                                                              DataProtectionScope.CurrentUser),
                                                                                                              STRING_Key));
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not decrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }
    }
}
