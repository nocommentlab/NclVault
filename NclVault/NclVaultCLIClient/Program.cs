using ConsoleTables;
using NclVaultCLIClient.Attributes;
using NclVaultCLIClient.Controllers;
using NclVaultCLIClient.Models;
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
        private static string STRING_InitIdKey;

        private static BackendInterface backendInterface;

        static void Main(string[] args)
        {
            backendInterface = new BackendInterface();
            if(File.Exists("init_id.key"))
            {
                //STRING_InitIdKey = Encoding.UTF8.GetString((ProtectDataManager.Unprotect(Convert.FromBase64String(File.ReadAllText("init_id.key")))));
                //Console.Write($"qui:{STRING_InitIdKey}");
            }

            Utils.PrintBanner();

            while (!BOOL_RequestExit)
            {
                Console.Write(PROMPT);
                STRING_LastCommand = Console.ReadLine();
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
                    case "cp":
                        ManageCreatePassword();
                        break;
                    case "/help":
                    case "/h":
                        Utils.PrintHelp();
                        break;
                    default:
                        Console.WriteLine("[E] - Command not found");
                        break;
                }
            }
            Console.Read();
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
            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}login/username:"); STRING_Username = Console.ReadLine();
            Console.Write($"{PROMPT}login/password:"); STRING_Password = Console.ReadLine();

            if (STRING_Username.Length > 0 && STRING_Password.Length > 0)
            {
                httpResponseResult = backendInterface.Init(new { username = STRING_Username, password = STRING_Password }).GetAwaiter().GetResult();

                if (httpResponseResult.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"Generated Init Identifier: {((InitResponse)httpResponseResult.OBJECT_RestResult).InitId}");

                    /* Creates the init_id.key file persistence */
                    byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(((InitResponse)httpResponseResult.OBJECT_RestResult).InitId));
                    File.WriteAllText("init_id.key", $"{Convert.ToBase64String(encryptedSecret)}");
                }
                Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
            }
        }

        private  static void ManageLogin()
        {
            string STRING_Username = String.Empty;
            string STRING_Password = String.Empty;
            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}login/username:"); STRING_Username = Console.ReadLine();
            Console.Write($"{PROMPT}login/password:"); STRING_Password = Console.ReadLine();

            if (STRING_Username.Length > 0 && STRING_Password.Length > 0)
            {
                
                httpResponseResult = backendInterface.Login(new { username = STRING_Username, password = STRING_Password }, ProtectDataManager.Unprotect("init_id.key")).GetAwaiter().GetResult();
                /*if (httpResponseResult.StatusCode == HttpStatusCode.OK)
                {
                    _STRING_LastJWTToken = httpResponseResult.STRING_JwtToken;
                }*/

                Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");


            }
        }

        private async static void ManageCreatePassword()
        {
            PasswordEntryCreateDto newPassword = new PasswordEntryCreateDto();
            HTTPResponseResult httpResponseResult = null;

            Console.Write($"{PROMPT}create/password/group:"); newPassword.Group = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/name:"); newPassword.Name = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/expired:"); newPassword.Expired = DateTime.Parse(Console.ReadLine());
            Console.Write($"{PROMPT}create/password/notes:"); newPassword.Notes = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/username:"); newPassword.Username = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/password:"); newPassword.Password = Console.ReadLine();
            Console.Write($"{PROMPT}create/password/url:"); newPassword.Url = Console.ReadLine();

            httpResponseResult = await backendInterface.CreatePassword(newPassword);
            if (httpResponseResult.StatusCode == HttpStatusCode.Created)
            {
                //_STRING_LastJWTToken = httpResponseResult.STRING_JwtToken;
                PasswordEntryReadDto passwordEntryReadDto = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;

                //Utils.PrintEntryTable(passwordEntryReadDto);

            }
            Console.WriteLine($"[{(int)httpResponseResult.StatusCode}] - [{httpResponseResult.StatusDescription}]");
        }
    }
}
