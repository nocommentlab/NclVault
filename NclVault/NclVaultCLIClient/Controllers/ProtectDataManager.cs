using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NclVaultCLIClient.Controllers
{
    /// <summary>
    /// https://docs.microsoft.com/it-it/dotnet/api/system.security.cryptography.protecteddata?view=dotnet-plat-ext-3.1
    /// </summary>
    class ProtectDataManager
    {
        // Create byte array for additional entropy when using Protect method.
        static byte[] s_additionalEntropy = { 0xC0, 0xFF,0xEE, 0xC0, 0xFF, 0xEE };

        public static byte[] Protect(byte[] data)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
                return ProtectedData.Protect(data, s_additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static string Unprotect(string STRING_ProtectedKey)
        {
            try
            {
                byte[] protectedKey = Convert.FromBase64String(File.ReadAllText(STRING_ProtectedKey));
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(protectedKey, s_additionalEntropy, DataProtectionScope.CurrentUser));
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
