using ConsoleTables;
using NclVaultCLIClient.Attributes;
using NclVaultCLIClient.Controllers;
using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NclVaultCLIClient
{
    class Program
    {
        private const string PROMPT = "ncl-vault> ";
        private static bool BOOL_RequestExit = false;
        private static string STRING_LastCommand = String.Empty;
        private static BackendInterface backendInterface;
        private static IPEndPoint _connectionProperties;

        static void Main(string[] args)
        {
            

            _connectionProperties = Utils.ValidateConnectionProperties(args[0], args[1]);
            if (null != _connectionProperties)
            {
                ReadLine.AutoCompletionHandler = new CommandAutoCompletionHandler();
                backendInterface = BackendInterface.GetInstance(_connectionProperties, true);

                Utils.PrintBanner();

                while (!BOOL_RequestExit)
                {
                    STRING_LastCommand = ReadLine.Read(PROMPT);
                    switch (STRING_LastCommand)
                    {
                        case "/exit":
                        case "/quit":
                            BOOL_RequestExit = true;
                            break;
                        case "/init":
                        case "/i":
                            ManageInit();
                            break;
                        case "/login":
                        case "/l":
                            ManageLogin();
                            break;
                        case "/readpassword":
                        case "/rp":
                            ManageReadPassword();
                            break;
                        case "/readpasswords":
                        case "/rps":
                            ManagerReadPasswords();
                            break;
                        case "/createpassword":
                        case "/cp":
                            ManageCreatePassword();
                            break;
                        case "/help":
                        case "/h":
                            Utils.PrintHelp();
                            break;
                        case "/restore":
                        case "/r":
                            ManageRestore();
                            break;
                        default:
                            Console.WriteLine("[E] - Command not found");
                            break;
                    }
                }
            }
            else
            {
                Console.Write("[E] - Error during connection properties values");
            }
        }

        private static void ManagerReadPasswords()
        {
            HTTPResponseResult httpResponseResult = null;
            httpResponseResult = backendInterface.ReadPasswords().GetAwaiter().GetResult();

            if (httpResponseResult.StatusCode == HttpStatusCode.OK)
            {
                //_STRING_LastJWTToken = httpResponseResult.STRING_JwtToken;
                List<PasswordEntryReadDto> passwordsEntryReadDto = (List<PasswordEntryReadDto>)httpResponseResult.OBJECT_RestResult;

                Utils.PrintEntryTable<PasswordEntryReadDto>(passwordsEntryReadDto);

            }

            Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
        }

        private static void ManageReadPassword()
        {
            string STRING_PasswordId = String.Empty;
            int INT32_PasswordId = -1;

            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}read/password/id:"); STRING_PasswordId = Console.ReadLine();
            if (int.TryParse(STRING_PasswordId, out INT32_PasswordId))
            {
                httpResponseResult = backendInterface.ReadPassword(INT32_PasswordId).GetAwaiter().GetResult();

                if (httpResponseResult.StatusCode == HttpStatusCode.OK)
                {
                    //_STRING_LastJWTToken = httpResponseResult.STRING_JwtToken;
                    PasswordEntryReadDto passwordEntryReadDto = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;

                    Utils.PrintEntryTable(passwordEntryReadDto);

                }
                Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");

            }

        }

        private static void ManageInit()
        {
            string STRING_Username = String.Empty;
            string STRING_Password = String.Empty;
            string STRING_PasswordRetype = String.Empty;
            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}login/username:"); STRING_Username = Console.ReadLine();
            Console.Write($"{PROMPT}login/password:"); STRING_Password = ReadLine.ReadPassword();
            Console.Write($"{PROMPT}login/password[retype]:"); STRING_PasswordRetype = ReadLine.ReadPassword();

            if (STRING_Username.Length > 0 && STRING_Password.Length > 0 && STRING_PasswordRetype.Length > 0)
            {
                if (STRING_Password.Equals(STRING_PasswordRetype))
                {
                    try
                    {
                        httpResponseResult = backendInterface.Init(new NetworkCredential { UserName = STRING_Username, Password = STRING_Password }).GetAwaiter().GetResult();
                        if (httpResponseResult.StatusCode == HttpStatusCode.OK)
                        {
                            Console.WriteLine($"Generated Init Identifier: {((InitResponse)httpResponseResult.OBJECT_RestResult).InitId}");

                            /* Creates the init_id.key file persistence */
                            byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(((InitResponse)httpResponseResult.OBJECT_RestResult).InitId));
                            File.WriteAllText("init_id.key", $"{Convert.ToBase64String(encryptedSecret)}");
                        }
                        Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
                    }
                    catch (System.Net.Http.HttpRequestException httpRequestException)
                    {
                        Console.WriteLine(String.Format("[E] - {0}", httpRequestException.Message));
                    }
                }
                else
                {
                    Console.WriteLine("[E] - Password mismatch");

                }
            }
            else
            {
                Console.WriteLine("[E] - Invalid credentials");
            }
        }

        private static void ManageLogin()
        {
            string STRING_Username = String.Empty;
            string STRING_Password = String.Empty;
            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}login/username:"); STRING_Username = Console.ReadLine();
            Console.Write($"{PROMPT}login/password:"); STRING_Password = ReadLine.ReadPassword();

            if (STRING_Username.Length > 0 && STRING_Password.Length > 0)
            {
                try
                {
                    
                    httpResponseResult = backendInterface.Login(new NetworkCredential { UserName = STRING_Username, Password = STRING_Password }, ProtectDataManager.Unprotect("init_id.key")).GetAwaiter().GetResult();
                    Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
                }
                catch (FileNotFoundException fileNotFoundException)
                {
                    Console.WriteLine(String.Format("[E] - {0}", fileNotFoundException.Message));
                    Console.WriteLine(String.Format("[I] - Please /init the database first!"));
                }

            }
        }

        private static void ManageCreatePassword()
        {
            PasswordEntryCreateDto newPassword = new PasswordEntryCreateDto();
            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}create/password/group:"); newPassword.Group = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/name:"); newPassword.Name = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/expired:"); newPassword.Expired = DateTime.Parse(Console.ReadLine());
            Console.Write($"{PROMPT}create/password/notes:"); newPassword.Notes = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/username:"); newPassword.Username = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/password:"); newPassword.Password = ReadLine.ReadPassword();
            Console.Write($"{PROMPT}create/password/url:"); newPassword.Url = Console.ReadLine();

            try
            {
                httpResponseResult = backendInterface.CreatePassword(newPassword).GetAwaiter().GetResult();
                if (httpResponseResult.StatusCode == HttpStatusCode.Created)
                {
                    PasswordEntryReadDto passwordEntryReadDto = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;

                }
                Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Console.WriteLine(String.Format("[E] - {0}", invalidOperationException.Message));
                Console.WriteLine(String.Format("[I] - Please /login before create a new password!"));
            }
        }

        private static void ManageRestore()
        {
            string STRING_InitId = String.Empty;

            Console.Write($"{PROMPT}restore/initId:"); STRING_InitId = Console.ReadLine();

            if (STRING_InitId.Length > 0)
            {

                /* Creates the init_id.key file persistence */
                byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(STRING_InitId));
                File.WriteAllText("init_id.key", $"{Convert.ToBase64String(encryptedSecret)}");
                Console.WriteLine($"Generated Init Identifier File: {STRING_InitId}");
            }
            else
            {
                Console.WriteLine("[E] - Invalid credentials");
            }
        }
    }
}
