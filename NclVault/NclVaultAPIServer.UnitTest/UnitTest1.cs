using it.ncl.netcore.cryptolibrary.Encryption.PBKDF2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NclVaultAPIServer.Controllers;
using NclVaultAPIServer.Data;
using NclVaultAPIServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;

namespace NclVaultAPIServer.UnitTest
{
    public class NclVaultAPIServerUnitTest
    {

        private IConfiguration LoadConfiguration(string STRING_ConfigFile)
        {
            return new ConfigurationBuilder()
                .AddJsonFile(STRING_ConfigFile)
                .Build();
        }

        private IMemoryCache GenerateMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetService<IMemoryCache>();
        }

        private ControllerContext GenerateHTTPContext()
        {
            var httpCtxStub = new Mock<HttpContext>();

            var controllerCtx = new ControllerContext();
            controllerCtx.HttpContext = httpCtxStub.Object;

            return controllerCtx;
        }
        /// <summary>
        /// Test the new user registration that is not present in the Database
        /// </summary>
        [Fact]
        public void DoSingUp_UserNotExists_Ok()
        {
            // Arrange
            IConfiguration configuration = LoadConfiguration("appsettings.json");
            IMemoryCache memoryCache = GenerateMemoryCache();

            /* Adds some fake username */
            var data = new List<Credential>
            {
                new Credential { Username = "user1" },
                new Credential { Username = "user2" },
                new Credential { Username = "user3" },
            }.AsQueryable();

            /* Mocks the DbSet<Credential> */
            var mockSet = new Mock<DbSet<Credential>>();
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            /* Mocks the VaultDbContext */
            var mockContext = new Mock<VaultDbContext>(configuration);
            mockContext.Setup(c => c.Credentials).Returns(mockSet.Object);

            VaultController vaultController = new VaultController(mockContext.Object, null, configuration, memoryCache);

            // Act
            var initResponse = vaultController.DoSignup(new DTOs.CredentialDTO.CredentialCreateDto { Username = "antonio", Password = "!//Lab2020" }).Result;

            // Assert
            Assert.IsType<OkObjectResult>(initResponse);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)initResponse).StatusCode);

        }

        /// <summary>
        /// Test the new user registration that is present in the Database
        /// </summary>
        [Fact]
        public void DoSignUp_UserExists_Unauthorized()
        {
            // Arrange
            IConfiguration configuration = LoadConfiguration("appsettings.json");
            IMemoryCache memoryCache = GenerateMemoryCache();

            /* Adds some fake username */
            var data = new List<Credential>
            {
                new Credential { Username = "antonio" },
                new Credential { Username = "user2" },
                new Credential { Username = "user3" },
            }.AsQueryable();

            /* Mocks the DbSet<Credential> */
            var mockSet = new Mock<DbSet<Credential>>();
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            /* Mocks the VaultDbContext */
            var mockContext = new Mock<VaultDbContext>(configuration);
            mockContext.Setup(c => c.Credentials).Returns(mockSet.Object);

            VaultController vaultController = new VaultController(mockContext.Object, null, configuration, memoryCache);

            // Act
            var initResponse = vaultController.DoSignup(new DTOs.CredentialDTO.CredentialCreateDto { Username = "antonio", Password = "!//Lab2020" }).Result;

            // Assert
            Assert.IsType<UnauthorizedResult>(initResponse);
            Assert.Equal((int)HttpStatusCode.Unauthorized, ((StatusCodeResult)initResponse).StatusCode);

        }

        [Fact]
        public void DoLogin_CredsExists_Ok()
        {
            // Arrange
            IConfiguration configuration = LoadConfiguration("appsettings.json");
            IMemoryCache memoryCache = GenerateMemoryCache();

            /* Adds some fake username */
            var data = new List<Credential>
            {
                new Credential { Username = "antonio", Password = PBKDF2Provider.Generate("!//Lab2020".PadLeft(32, '*')) },
                new Credential { Username = "user2" , Password = PBKDF2Provider.Generate("!//Lab2021".PadLeft(32, '*')) },
                new Credential { Username = "user3" , Password = PBKDF2Provider.Generate("!//Lab2022".PadLeft(32, '*')) },
            }.AsQueryable();

            /* Mocks the DbSet<Credential> */
            var mockSet = new Mock<DbSet<Credential>>();
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            /* Mocks the VaultDbContext */
            var mockContext = new Mock<VaultDbContext>(configuration);
            mockContext.Setup(c => c.Credentials).Returns(mockSet.Object);

            TokenController tokenController = new TokenController(mockContext.Object, configuration, memoryCache);
            /* Assigns the fake HTTPContext */
            tokenController.ControllerContext = GenerateHTTPContext();

            // Act
            var initResponse = tokenController.DoLogin(new DTOs.CredentialDTO.CredentialCreateDto { Username = "antonio", Password = "!//Lab2020" });

            // Assert
            Assert.IsType<OkResult>(initResponse);
            //Assert.Equal((int)HttpStatusCode.Unauthorized, ((StatusCodeResult)initResponse).StatusCode);
        }

        [Fact]
        public void DoLogin_CredsNotExists_Ok()
        {
            // Arrange
            IConfiguration configuration = LoadConfiguration("appsettings.json");
            IMemoryCache memoryCache = GenerateMemoryCache();

            /* Adds some fake username */
            var data = new List<Credential>
            {
                new Credential { Username = "nocommentlab", Password = PBKDF2Provider.Generate("!//Lab2023")  },
                new Credential { Username = "user2" , Password = PBKDF2Provider.Generate("!//Lab2021")},
                new Credential { Username = "user3" , Password = PBKDF2Provider.Generate("!//Lab2022")},
            }.AsQueryable();

            /* Mocks the DbSet<Credential> */
            var mockSet = new Mock<DbSet<Credential>>();
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Credential>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            /* Mocks the VaultDbContext */
            var mockContext = new Mock<VaultDbContext>(configuration);
            mockContext.Setup(c => c.Credentials).Returns(mockSet.Object);

            TokenController tokenController = new TokenController(mockContext.Object, configuration, memoryCache);
            /* Assigns the fake HTTPContext */
            tokenController.ControllerContext = GenerateHTTPContext();

            // Act
            var initResponse = tokenController.DoLogin(new DTOs.CredentialDTO.CredentialCreateDto { Username = "antonio", Password = "!//Lab2020" });

            // Assert
            Assert.IsType<OkResult>(initResponse);
            //Assert.Equal((int)HttpStatusCode.Unauthorized, ((StatusCodeResult)initResponse).StatusCode);
        }
    }
}
