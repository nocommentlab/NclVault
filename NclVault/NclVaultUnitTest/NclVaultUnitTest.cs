using Microsoft.VisualStudio.TestTools.UnitTesting;
using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using System;
using System.Net;

namespace NclVaultUnitTest
{
    [TestClass]
    public class NclVaultUnitTest
    {
        #region Members
        private static BackendInterface _backendInterface;
        private static NetworkCredential _initCredential;
        private static string _STRING_InitId;
        private static PasswordEntryCreateDto _newPasswordEntry;
        #endregion


        [TestMethod]
        [Description("TestInit")]

        public void Test1()
        {
            _initCredential = new NetworkCredential
            {
                UserName = "test_username",
                Password = "P@55w0rd"
            };
            _backendInterface = BackendInterface.GetInstance();

            HTTPResponseResult httpReponseResult = _backendInterface.Init(_initCredential).GetAwaiter().GetResult();

            _STRING_InitId = ((InitResponse)httpReponseResult.OBJECT_RestResult).InitId;

            Assert.AreEqual(httpReponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpReponseResult.OBJECT_RestResult, null);
            Assert.IsTrue(((InitResponse)httpReponseResult.OBJECT_RestResult).InitId.Length > 0);
        }

        [TestMethod]
        [Description("TestLogin")]
        public void Test2()
        {
            _backendInterface = BackendInterface.GetInstance();
            HTTPResponseResult httpReponseResult = _backendInterface.Login(_initCredential, _STRING_InitId).GetAwaiter().GetResult();
             
            Assert.AreEqual(httpReponseResult.StatusCode, HttpStatusCode.OK);
            Assert.AreNotEqual(httpReponseResult.STRING_JwtToken, null);
            Assert.IsTrue(httpReponseResult.STRING_JwtToken.Length > 0);
        }

        [TestMethod]
        [Description("TestCreatePassword")]

        public void Test3()
        {
            _backendInterface = BackendInterface.GetInstance();

            _newPasswordEntry = new PasswordEntryCreateDto
            {
                Username = "darth.vader",
                Password = "D3ath5t@r",
                Expired = DateTime.Parse("01/01/1970"),
                Group = "star_wars",
                Name = "death star access",
                Url = "https://www.startwars/login",
                Notes = "my personal notes"
            };

            HTTPResponseResult httpResponseResult = _backendInterface.CreatePassword(_newPasswordEntry).GetAwaiter().GetResult();
            
            Assert.AreEqual(httpResponseResult.StatusCode, HttpStatusCode.OK);
        }
    }
}
