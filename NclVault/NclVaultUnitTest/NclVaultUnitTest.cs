using Microsoft.VisualStudio.TestTools.UnitTesting;
using ncl.net.cryptolybrary.Encryption.AES;
using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace NclVaultUnitTest
{
    [TestClass]
    // This test the following function:
    // - Init the database(the database doesn't include any credential, otherwise will fail)
    // - Login
    // - Create a new password entry
    // - Read the password created at the before step
    public class NclVaultUnitTest
    {
        #region Costants
        private const int CRED_NUMBER_STRESS_TEST = 100;

        #endregion
        #region Members
        private static BackendInterface _backendInterface;
        private static NetworkCredential _initCredential;
        private static string _STRING_InitId;
        private static List<PasswordEntryCreateDto> _randomCredentials;
        private static List<Guid> _randomPasswords;
        private static readonly IPEndPoint _vaultEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5001);
        #endregion


        [ClassInitialize]
        [Description("Init the database")]
        public static void InitDatabase(TestContext context)
        {
            _randomCredentials = new List<PasswordEntryCreateDto>();
            _randomPasswords = new List<Guid>();

            for (int INT32_Idx = 0; INT32_Idx < CRED_NUMBER_STRESS_TEST; INT32_Idx++)
            {
                Guid randomPassword = Guid.NewGuid();

                _randomCredentials.Add(new PasswordEntryCreateDto
                {
                    Username = $"{Guid.NewGuid()}",
                    Password = $"{randomPassword}",
                    Expired = DateTime.Parse("01/01/1970"),
                    Group = $"{Guid.NewGuid()}",
                    Name = $"{Guid.NewGuid()}",
                    Url = $"{Guid.NewGuid()}",
                    Notes = $"{Guid.NewGuid()}"
                });

                _randomPasswords.Add(randomPassword);
            }

            _initCredential = new NetworkCredential
            {
                UserName = "nocommentlab",
                Password = "!//Lab2020"
            };

            _backendInterface = BackendInterface.GetInstance(_vaultEndpoint, true);
        }

        [TestMethod]
        [Description("Test the init process")]
        public void T1_001_DoSignup()
        {
            HTTPResponseResult httpReponseResult = _backendInterface.Signup(_initCredential).GetAwaiter().GetResult();

            

            Assert.AreEqual(httpReponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpReponseResult.OBJECT_RestResult, null);
            


        }

        [TestMethod]
        [Description("Test the InitId protection mechanism")]
        public void T1_002_DoProtectInitId()
        {
            Guid GUID_InitId = Guid.NewGuid();
            Console.WriteLine($"Generated Init Identifier: {GUID_InitId}");

            /* Creates the init_id.key file persistence */
            byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(GUID_InitId.ToString()), _initCredential.Password);
            File.WriteAllText($"{_initCredential.UserName}_init_id.key", $"{Convert.ToBase64String(encryptedSecret)}");

            Assert.IsTrue(File.Exists($"{_initCredential.UserName}_init_id.key"));
        }

        [TestMethod]
        [Description("Test the login process")]
        public void T1_003_DoLogin()
        {
            _backendInterface = BackendInterface.GetInstance(_vaultEndpoint, true);
            
            HTTPResponseResult httpResponseResult = _backendInterface.Login(_initCredential).GetAwaiter().GetResult();
            Assert.AreEqual(httpResponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpResponseResult.STRING_JwtToken, null);
            Assert.IsTrue(httpResponseResult.STRING_JwtToken.Length > 0);
        }

        [TestMethod]
        [Description("Test the password creation process multiple time")]
        public void T1_004_CreatePasswordsStressTest()
        {
            foreach (PasswordEntryCreateDto randomCredential in _randomCredentials)
            {
                randomCredential.Password = AesProvider.AES_CBC_Encryption_Rand_IV(randomCredential.Password,
                                                                              ProtectDataManager.Unprotect($"{ _initCredential.UserName}_init_id.key", _initCredential.Password));
                HTTPResponseResult httpResponseResult = _backendInterface.CreatePassword(randomCredential).GetAwaiter().GetResult();

                Assert.AreEqual(httpResponseResult.StatusCode, HttpStatusCode.Created);
            }
        }

        [TestMethod]
        [Description("Test the password read process")]
        public void T1_005_ReadPasswordsStressTest()
        {

            HTTPResponseResult httpResponseResult = _backendInterface.ReadPasswords().GetAwaiter().GetResult();
            Assert.AreEqual(httpResponseResult.StatusCode, HttpStatusCode.OK);

            List<PasswordEntryReadDto> dumpedPassword = (List<PasswordEntryReadDto>)httpResponseResult.OBJECT_RestResult;

            for (int INT32_Idx = 0; INT32_Idx < dumpedPassword.Count; INT32_Idx++)
            {
                Assert.AreEqual(dumpedPassword[INT32_Idx].Expired, _randomCredentials[INT32_Idx].Expired);
                Assert.AreEqual(dumpedPassword[INT32_Idx].Group, _randomCredentials[INT32_Idx].Group);
                Assert.AreEqual(dumpedPassword[INT32_Idx].Name, _randomCredentials[INT32_Idx].Name);
                Assert.AreEqual(dumpedPassword[INT32_Idx].Notes, _randomCredentials[INT32_Idx].Notes);
                Assert.AreEqual(_randomPasswords[INT32_Idx].ToString(), AesProvider.AES_CBC_Decryption_Rand_IV(_randomCredentials[INT32_Idx].Password,
                                            ProtectDataManager.Unprotect($"{ _initCredential.UserName}_init_id.key", _initCredential.Password)));
                Assert.AreEqual(dumpedPassword[INT32_Idx].Url, _randomCredentials[INT32_Idx].Url);
                Assert.AreEqual(dumpedPassword[INT32_Idx].Username, _randomCredentials[INT32_Idx].Username);
            }
        }

        [TestMethod]
        [Description("Test the InitId unprotection mechanism")]
        public void T2_001_DoUnprotectInitId()
        {
            Assert.IsTrue(File.Exists($"{ _initCredential.UserName}_init_id.key"));
            _STRING_InitId = ProtectDataManager.Unprotect($"{ _initCredential.UserName}_init_id.key", _initCredential.Password);
            Assert.IsTrue(_STRING_InitId.Length > 0);
        }

        [TestMethod]
        [Description("Test the login process")]
        public void T2_002_DoLogin()
        {
            _backendInterface = BackendInterface.GetInstance(_vaultEndpoint, true);
            HTTPResponseResult httpReponseResult = _backendInterface.Login(_initCredential).GetAwaiter().GetResult();

            Assert.AreEqual(httpReponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpReponseResult.STRING_JwtToken, null);
            Assert.IsTrue(httpReponseResult.STRING_JwtToken.Length > 0);
        }

        
    }
}
