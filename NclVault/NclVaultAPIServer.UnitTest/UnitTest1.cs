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
    }
}
