using ncl.net.cryptolybrary.Encryption.AES;
using NclVaultCLIClient.Controllers;
using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace NclVaultCLIClient
{
    class Program
    {
        private const string PROMPT = "ncl-vault> ";
        private static bool BOOL_RequestExit = false;
        private static string STRING_LastCommand = String.Empty;
        private static BackendInterface backendInterface;
        private static IPEndPoint _connectionProperties;
        private static NetworkCredential _loggedUser;

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
                        case "/signup":
                        case "/s":
                            ManageSignup();
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
                        case "/updatepassword":
                        case "/up":
                            ManageUpdatePassword();
                            break;
                        case "/deletepassword":
                        case "/del":
                            ManageDeletePassword();
                            break;
                        case "/help":
                        case "/h":
                            Utils.PrintHelp();
                            break;
                        case "/restore":
                        case "/r":
                            ManageRestore();
                            break;
                        case "/logout":
                            ManageLogout();
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

        private static void ManageLogout()
        {
            HTTPResponseResult httpResponseResult =  backendInterface.Logout().GetAwaiter().GetResult();

            Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
        }

        private static void ManageSignup()
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
                        httpResponseResult = backendInterface.Signup(new NetworkCredential { UserName = STRING_Username, Password = STRING_Password }).GetAwaiter().GetResult();
                        if (httpResponseResult.StatusCode == HttpStatusCode.OK)
                        {
                            Guid GUID_InitId = Guid.NewGuid();
                            Console.WriteLine($"Generated Init Identifier: {GUID_InitId}");

                            /* Creates the init_id.key file persistence */
                            byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(GUID_InitId.ToString()), STRING_Password);
                            File.WriteAllText($"{STRING_Username}_init_id.key", $"{Convert.ToBase64String(encryptedSecret)}");


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
        private static void ManagerReadPasswords()
        {
            HTTPResponseResult httpResponseResult = null;
            httpResponseResult = backendInterface.ReadPasswords().GetAwaiter().GetResult();

            if (httpResponseResult.StatusCode == HttpStatusCode.OK)
            {
                List<PasswordEntryReadDto> passwordsEntryReadDto = (List<PasswordEntryReadDto>)httpResponseResult.OBJECT_RestResult;
                for (int INT32_Id = 0; INT32_Id < passwordsEntryReadDto.Count; INT32_Id++)
                {
                    Console.Write("\r[I] - Decrypting credentials[{0}/{1}]", INT32_Id + 1, passwordsEntryReadDto.Count);
                    passwordsEntryReadDto[INT32_Id].Password = AesProvider.AES_CBC_Decryption_Rand_IV(passwordsEntryReadDto[INT32_Id].Password,
                                                                              ProtectDataManager.Unprotect($"{ _loggedUser.UserName}_init_id.key", _loggedUser.Password));
                    Thread.Sleep(1);
                }
                Console.WriteLine();
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
                    PasswordEntryReadDto passwordEntryReadDto = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;
                    passwordEntryReadDto.Password = AesProvider.AES_CBC_Decryption_Rand_IV(passwordEntryReadDto.Password,
                                                                              ProtectDataManager.Unprotect($"{ _loggedUser.UserName}_init_id.key", _loggedUser.Password));
                    Utils.PrintEntryTable(passwordEntryReadDto);

                }
                Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");

            }

        }
        private static void ManageUpdatePassword()
        {
            string STRING_PasswordId = String.Empty;
            int INT32_PasswordId = -1;

            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}update/password/id:"); STRING_PasswordId = Console.ReadLine();
            if (int.TryParse(STRING_PasswordId, out INT32_PasswordId))
            {
                httpResponseResult = backendInterface.ReadPassword(INT32_PasswordId).GetAwaiter().GetResult();
                if (httpResponseResult.StatusCode == HttpStatusCode.OK)
                {
                    PasswordEntryCreateDto updatedPassword = new PasswordEntryCreateDto();

                    PasswordEntryReadDto passwordEntryReadDto = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;
                    passwordEntryReadDto.Password = AesProvider.AES_CBC_Decryption_Rand_IV(passwordEntryReadDto.Password,
                                                                              ProtectDataManager.Unprotect($"{ _loggedUser.UserName}_init_id.key", _loggedUser.Password));

                    Console.Write($"{PROMPT}update/password/group[{passwordEntryReadDto.Group}]:"); updatedPassword.Group = Console.ReadLine();
                    Console.Write($"{PROMPT}update/password/name[{passwordEntryReadDto.Name}]:"); updatedPassword.Name = Console.ReadLine();
                    Console.Write($"{PROMPT}update/password/expired[{passwordEntryReadDto.Expired}]:"); updatedPassword.Expired = DateTime.Parse(Console.ReadLine());
                    Console.Write($"{PROMPT}update/password/notes[{passwordEntryReadDto.Notes}]:"); updatedPassword.Notes = Console.ReadLine();
                    Console.Write($"{PROMPT}update/password/username[{passwordEntryReadDto.Username}]:"); updatedPassword.Username = Console.ReadLine();
                    Console.Write($"{PROMPT}update/password/password[{passwordEntryReadDto.Password}]:"); updatedPassword.Password = ReadLine.ReadPassword();
                    Console.Write($"{PROMPT}update/password/url[{passwordEntryReadDto.Url}]:"); updatedPassword.Url = Console.ReadLine();

                    updatedPassword.Password = AesProvider.AES_CBC_Encryption_Rand_IV(updatedPassword.Password,
                                                                              ProtectDataManager.Unprotect($"{ _loggedUser.UserName}_init_id.key", _loggedUser.Password));
                    try
                    {
                        httpResponseResult = backendInterface.UpdatePassword(INT32_PasswordId, updatedPassword).GetAwaiter().GetResult();
                        if (httpResponseResult.StatusCode == HttpStatusCode.Created)
                        {
                            passwordEntryReadDto = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;
                            passwordEntryReadDto.Password = AesProvider.AES_CBC_Decryption_Rand_IV(passwordEntryReadDto.Password,
                                                                              ProtectDataManager.Unprotect($"{ _loggedUser.UserName}_init_id.key", _loggedUser.Password));
                            Utils.PrintEntryTable(passwordEntryReadDto);
                        }
                    }
                    catch (InvalidOperationException invalidOperationException)
                    {
                        Console.WriteLine(String.Format("[E] - {0}", invalidOperationException.Message));
                        Console.WriteLine(String.Format("[I] - Please /login before create a new password!"));
                    }

                    Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
                }
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

                    httpResponseResult = backendInterface.Login(new NetworkCredential { UserName = STRING_Username, Password = STRING_Password }/*, ProtectDataManager.Unprotect("init_id.key")*/).GetAwaiter().GetResult();
                    Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");

                    _loggedUser = new NetworkCredential { UserName = STRING_Username, Password = STRING_Password };
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

            if (null != _loggedUser)
            {
                Console.Write($"{PROMPT}create/password/group:"); newPassword.Group = Console.ReadLine();
                Console.Write($"{PROMPT}create/password/name:"); newPassword.Name = Console.ReadLine();
                Console.Write($"{PROMPT}create/password/expired:"); newPassword.Expired = DateTime.Parse(Console.ReadLine());
                Console.Write($"{PROMPT}create/password/notes:"); newPassword.Notes = Console.ReadLine();
                Console.Write($"{PROMPT}create/password/username:"); newPassword.Username = Console.ReadLine();
                Console.Write($"{PROMPT}create/password/password:"); newPassword.Password = ReadLine.ReadPassword();
                Console.Write($"{PROMPT}create/password/url:"); newPassword.Url = Console.ReadLine();

                newPassword.Password = AesProvider.AES_CBC_Encryption_Rand_IV(newPassword.Password,
                                                                              ProtectDataManager.Unprotect($"{ _loggedUser.UserName}_init_id.key", _loggedUser.Password));

                try
                {
                    httpResponseResult = backendInterface.CreatePassword(newPassword).GetAwaiter().GetResult();
                    if (httpResponseResult.StatusCode == HttpStatusCode.Created)
                    {
                        PasswordEntryReadDto passwordEntryReadDto = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;
                        Utils.PrintEntryTable(passwordEntryReadDto);
                    }
                    Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    Console.WriteLine(String.Format("[E] - {0}", invalidOperationException.Message));
                    Console.WriteLine(String.Format("[I] - Please /login before create a new password!"));
                }
            }
            else
            {
                Console.WriteLine(String.Format("[I] - Please /login before create a new password!"));
            }
        }
        private static void ManageDeletePassword()
        {
            string STRING_PasswordId = String.Empty;
            int INT32_PasswordId = -1;

            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}read/password/id:"); STRING_PasswordId = Console.ReadLine();
            if (int.TryParse(STRING_PasswordId, out INT32_PasswordId))
            {
                httpResponseResult = backendInterface.DeletePassword(INT32_PasswordId).GetAwaiter().GetResult();

                Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
            }
        }
        private static void ManageRestore()
        {
            string STRING_InitId = String.Empty;

            Console.Write($"{PROMPT}restore/initId:"); STRING_InitId = Console.ReadLine();

            if (STRING_InitId.Length > 0)
            {

                /* Creates the init_id.key file persistence */
                byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(STRING_InitId), _loggedUser.Password);
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
