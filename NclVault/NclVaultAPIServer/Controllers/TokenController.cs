﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using it.ncl.netcore.cryptolibrary.Encryption.PBKDF2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using NclVaultAPIServer.Data;
using NclVaultAPIServer.DTOs.CredentialDTO;
using NclVaultAPIServer.Utils;

namespace NclVaultAPIServer.Controllers
{
    /// <summary>
    /// Manage the JWT generation after the login process
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly VaultDbContext _vaultDbContext;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vaultDbContext">The DB context</param>
        /// <param name="configuration">The application configuration</param>
        public TokenController(VaultDbContext vaultDbContext, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _vaultDbContext = vaultDbContext;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }


        /// <summary>
        /// Configure the DoLogin Service
        /// </summary>
        /// <param name="credentialCreateDto">JSON CredentialReadDto Object</param>
        /// <returns>Ok, BadRequest, Unauthorized</returns>
        [HttpPost]
        [Route("login")]
        public IActionResult DoLogin([FromBody] CredentialCreateDto credentialCreateDto)
        {
            /* Checks if the received object is well formed */
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            /* Calculate the SHA256(<password>+SALT) - OBSOLETE and INSECURE! */
            //string STRING_CalculatedSHA256Password = CryptoHelper.ComputeSha256Hash(credentialCreateDto.Password.PadLeft(32, '*') + _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "PASSWORD_SALT"));

            var credential = _vaultDbContext.Credentials.Where(cred => cred.Username == credentialCreateDto.Username).FirstOrDefault();

            /* Checks the credential */
            if (credential !=null && 
                !PBKDF2Provider.IsValid(credentialCreateDto.Password.PadLeft(32, '*'), credential.Password))
                                                         //cred.Password == STRING_CalculatedSHA256Password))
            {
                return Unauthorized();
            }

            // The login process is terminated correctly, generates the JWT token

            // Generates the ClaimIndentity object
            var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);

            // Adds to the JWT token the Username Claim that contains the username that has logged in */
            identity.AddClaim(new Claim("username", credentialCreateDto.Username));

            /* Saves the generated ClaimsPrincipal inside the HTTP Context */
            HttpContext.User = new ClaimsPrincipal(identity);

            return Ok();
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult DoLogout()
        {
            /* Checks if the received object is well formed */
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            _memoryCache.Set(Request.Headers[HeaderNames.Authorization], true, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(_configuration.GetValue<int>("NCLVaultConfiguration:JWTConfiguration:CACHE_TOKEN_REPUDIATION_ABSOLUTE_MINUTES")),
                SlidingExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("NCLVaultConfiguration:JWTConfiguration:TOKEN_INACTIVITY_EXPIRATION_MINUTES"))
            });

            return Ok();
        }
    }
}
