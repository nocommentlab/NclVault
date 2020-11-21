using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private static PasswordEntryCreateDto _newPasswordEntry;
        private static int INT32_LastInsertedPasswordId;
        private static List<PasswordEntryCreateDto> _randomCredentials;
        #endregion


        [ClassInitialize]
        [Description("Init the database")]
        public static void InitDatabase(TestContext context)
        {
            _randomCredentials = new List<PasswordEntryCreateDto>();

            for (int INT32_Idx = 0; INT32_Idx < CRED_NUMBER_STRESS_TEST; INT32_Idx++)
            {
                _randomCredentials.Add(new PasswordEntryCreateDto
                {
                    Username = $"{Guid.NewGuid()}",
                    Password = $"{Guid.NewGuid()}",
                    Expired = DateTime.Parse("01/01/1970"),
                    Group = $"{Guid.NewGuid()}",
                    Name = $"{Guid.NewGuid()}",
                    Url = $"{Guid.NewGuid()}",
                    Notes = $"{Guid.NewGuid()}"
                });
            }

            _initCredential = new NetworkCredential
            {
                UserName = "test_username",
                Password = "P@55w0rd"
            };

            _backendInterface = BackendInterface.GetInstance(true);
        }

        [TestMethod]
        [Description("Test the init process")]
        public void T1_001_DoInit()
        {
            HTTPResponseResult httpReponseResult = _backendInterface.Init(_initCredential).GetAwaiter().GetResult();

            _STRING_InitId = ((InitResponse)httpReponseResult.OBJECT_RestResult).InitId;

            Assert.AreEqual(httpReponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpReponseResult.OBJECT_RestResult, null);
            Assert.IsTrue(((InitResponse)httpReponseResult.OBJECT_RestResult).InitId.Length > 0);


        }

        [TestMethod]
        [Description("Test the InitId protection mechanism")]
        public void T1_002_DoProtectInitId()
        {
            byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(_STRING_InitId));
            File.WriteAllText("init_id.key", $"{Convert.ToBase64String(encryptedSecret)}");

            Assert.IsTrue(File.Exists("init_id.key"));
        }

        [TestMethod]
        [Description("Test the login process")]
        public void T1_003_DoLogin()
        {
            _backendInterface = BackendInterface.GetInstance(true);
            HTTPResponseResult httpReponseResult = _backendInterface.Login(_initCredential, _STRING_InitId).GetAwaiter().GetResult();

            Assert.AreEqual(httpReponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpReponseResult.STRING_JwtToken, null);
            Assert.IsTrue(httpReponseResult.STRING_JwtToken.Length > 0);
        }

        [TestMethod]
        [Description("Test the password creation process multiple time")]
        public void T1_004_CreatePasswordsStressTest()
        {

            foreach (PasswordEntryCreateDto randomCredential in _randomCredentials)
            {

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
                Assert.AreEqual(dumpedPassword[INT32_Idx].Password, _randomCredentials[INT32_Idx].Password);
                Assert.AreEqual(dumpedPassword[INT32_Idx].Url, _randomCredentials[INT32_Idx].Url);
                Assert.AreEqual(dumpedPassword[INT32_Idx].Username, _randomCredentials[INT32_Idx].Username);


            }
        }

        [TestMethod]
        [Description("Test the InitId unprotection mechanism")]
        public void T2_001_DoUnprotectInitId()
        {
            Assert.IsTrue(File.Exists("init_id.key"));
            _STRING_InitId = ProtectDataManager.Unprotect("init_id.key");
            Assert.IsTrue(_STRING_InitId.Length > 0);
        }

        [TestMethod]
        [Description("Test the login process")]
        public void T2_002_DoLogin()
        {
            _backendInterface = BackendInterface.GetInstance(true);
            HTTPResponseResult httpReponseResult = _backendInterface.Login(_initCredential, _STRING_InitId).GetAwaiter().GetResult();

            Assert.AreEqual(httpReponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpReponseResult.STRING_JwtToken, null);
            Assert.IsTrue(httpReponseResult.STRING_JwtToken.Length > 0);
        }

        [TestMethod]
        [Description("Test the password creation process")]
        public void T2_003_CreatePassword()
        {
            _newPasswordEntry = new PasswordEntryCreateDto
            {
                Username = $"{Guid.NewGuid()}",
                Password = $"{Guid.NewGuid()}",
                Expired = DateTime.Parse("01/01/1970"),
                Group = $"{Guid.NewGuid()}",
                Name = $"{Guid.NewGuid()}",
                Url = $"{Guid.NewGuid()}",
                Notes = $"{Guid.NewGuid()}"
            };

            HTTPResponseResult httpResponseResult = _backendInterface.CreatePassword(_newPasswordEntry).GetAwaiter().GetResult();
            INT32_LastInsertedPasswordId = ((PasswordEntryReadDto)(httpResponseResult.OBJECT_RestResult)).Id;
            Assert.AreEqual(httpResponseResult.StatusCode, HttpStatusCode.Created);

        }

        [TestMethod]
        [Description("Test the password read process")]
        public void T2_004_ReadPassword()
        {


            HTTPResponseResult httpResponseResult = _backendInterface.ReadPassword(INT32_LastInsertedPasswordId).GetAwaiter().GetResult();
            Assert.AreEqual(httpResponseResult.StatusCode, HttpStatusCode.OK);

            PasswordEntryReadDto dumpedPassword = (PasswordEntryReadDto)httpResponseResult.OBJECT_RestResult;


            Assert.AreEqual(dumpedPassword.Expired, _newPasswordEntry.Expired);
            Assert.AreEqual(dumpedPassword.Group, _newPasswordEntry.Group);
            Assert.AreEqual(dumpedPassword.Name, _newPasswordEntry.Name);
            Assert.AreEqual(dumpedPassword.Notes, _newPasswordEntry.Notes);
            Assert.AreEqual(dumpedPassword.Password, _newPasswordEntry.Password);
            Assert.AreEqual(dumpedPassword.Url, _newPasswordEntry.Url);
            Assert.AreEqual(dumpedPassword.Username, _newPasswordEntry.Username);



        }
    }
}
