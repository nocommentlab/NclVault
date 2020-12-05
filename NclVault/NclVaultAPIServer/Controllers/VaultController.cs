using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NclVaultAPIServer.Data;
using NclVaultAPIServer.DTOs.CredentialDTO;
using NclVaultAPIServer.DTOs.PasswordEntryDTO;
using NclVaultAPIServer.Models;
using NclVaultAPIServer.Utils;
using NETCore.Encrypt;

namespace NclVaultAPIServer.Controllers
{
    /// <summary>
    /// Manages the vault entries
    /// </summary>
    [ApiController]
    [Route("[controller]")]

    public class VaultController : ControllerBase
    {
        #region Members
        private readonly VaultDbContext _vaultDbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vaultDbContext">The DB context</param>
        /// <param name="mapper">The Mapper service</param>
        /// <param name="configuration">The application configuration</param>
        public VaultController(VaultDbContext vaultDbContext, IMapper mapper, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _vaultDbContext = vaultDbContext;
            _mapper = mapper;
            _configuration = configuration;
            _memoryCache = memoryCache;

        }

        //POST /singup
        [HttpPost]
        [Route("singup")]
        public ActionResult<InitResponse> DoSignup([FromBody] CredentialCreateDto credentialCreateDto)
        {
            /* Checks if the request body respects the Template Decorators of the CredentialCreateDto Objects */
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            /* Checks if there's present a user with the same username */
            if (_vaultDbContext.Credentials.Any(credential => credential.Username.Equals(credentialCreateDto.Username)))
            {
                return Unauthorized(); //401
            }

            // Creates a Credential object
            Credential credential = new Credential
            {
                // Sets the passed Username
                Username = credentialCreateDto.Username,
                // Sets the passed Password - Sha256(<passed_password>+salt)
                Password = CryptoHelper.ComputeSha256Hash(credentialCreateDto.Password.PadLeft(32, '*') + _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "PASSWORD_SALT"))
            };

            // Adds the element to Credential table and save
            _vaultDbContext.Credentials.Add(credential);
            _vaultDbContext.SaveChanges();

            // Returns the stored Credential
            return Ok(new InitResponse { Username = credential.Username });
        }

        //POST /password
        [HttpPost]
        [Route("password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreatePassword([FromBody] PasswordEntryCreateDto passwordEntryCreateDto)
        {
            bool BOOL_IsJWtTokenRepudied;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            // Checks if the JWT token is repudied
            _memoryCache.TryGetValue(Request.Headers[HeaderNames.Authorization], out BOOL_IsJWtTokenRepudied);
            if (null == selectedCredential || BOOL_IsJWtTokenRepudied)
            {
                return Unauthorized();
            }

            /* Sets the encrypted password using the InitId request header parameter as key*/
            PasswordEntry passwordEntry = _mapper.Map<PasswordEntry>(passwordEntryCreateDto);
            /* Assign the password entry foreign key with the logged credential primary key */
            passwordEntry.CredentialFK = selectedCredential.Id;

            // Adds the entry password to EF and writes to the databae
            _vaultDbContext.Passwords.Add(passwordEntry);
            _vaultDbContext.SaveChanges();

            // After, redirect the browser to the ReadPasswordById Action(see the below function)
            return CreatedAtAction(nameof(ReadPasswordById), new { ID = passwordEntry.Id }, passwordEntry);
        }

        //GET /password/{id}
        [HttpGet]
        [Route("password/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<PasswordEntryReadDto> DecryptedReadPasswordById(int id)
        {
            bool BOOL_IsJWtTokenRepudied;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            // Checks if the JWT token is repudied
            _memoryCache.TryGetValue(Request.Headers[HeaderNames.Authorization], out BOOL_IsJWtTokenRepudied);
            if (null == selectedCredential || BOOL_IsJWtTokenRepudied)
            {
                return Unauthorized();
            }

            // Extracts the PasswordEntry that has the received ID
            PasswordEntry passwordEntry = _vaultDbContext.Passwords.SingleOrDefault(password => password.Id == id && password.CredentialFK == selectedCredential.Id);

            if (null == passwordEntry)
            {
                return NotFound();
            }

            // Returns the PasswordEntry object
            return Ok(_mapper.Map<PasswordEntryReadDto>(passwordEntry));
        }

        //GET /password
        [HttpGet]
        [Route("password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<List<PasswordEntryReadDto>> DecryptedReadPassword()
        {
            bool BOOL_IsJWtTokenRepudied;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            // Checks if the JWT token is repudied
            _memoryCache.TryGetValue(Request.Headers[HeaderNames.Authorization], out BOOL_IsJWtTokenRepudied);
            if (null == selectedCredential || BOOL_IsJWtTokenRepudied)
            {
                return Unauthorized();
            }

            IEnumerable<PasswordEntry> passwordEntries = _vaultDbContext.Passwords.Where(cred => cred.CredentialFK == selectedCredential.Id);

            if (passwordEntries.Count() == 0)
            {
                return NotFound();
            }

            // Returns the Mapped List<PasswordEntry> objects
            return Ok(_mapper.Map<List<PasswordEntryReadDto>>(passwordEntries));
        }

        //PUT /password/{id}
        [HttpPut]
        [Route("password/{id}")]
        public IActionResult UpdatePassword(int id, [FromBody] PasswordEntryCreateDto passwordEntryCreateDto)
        {
            bool BOOL_IsJWtTokenRepudied;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            // Checks if the JWT token is repudied
            _memoryCache.TryGetValue(Request.Headers[HeaderNames.Authorization], out BOOL_IsJWtTokenRepudied);
            if (null == selectedCredential || BOOL_IsJWtTokenRepudied)
            {
                return Unauthorized();
            }

            // Extract the password entry with the requested ID
            PasswordEntry passwordEntry = _vaultDbContext.Passwords.FirstOrDefault(element => element.Id == id && element.CredentialFK == selectedCredential.Id);
            if (null == passwordEntry)
            {
                return NotFound();
            }

            /* Sets the encrypted password using the InitId request header parameter as key*/
            //passwordEntryCreateDto.Password = CryptoHelper.EncryptString(passwordEntryCreateDto.Password, Request.Headers["InitId"]);
            //passwordEntryCreateDto.Password = EncryptProvider.AESEncrypt(passwordEntryCreateDto.Password, Request.Headers["InitId"]);

            _mapper.Map(passwordEntryCreateDto, passwordEntry);
            _vaultDbContext.SaveChanges();


            return CreatedAtAction(nameof(ReadPasswordById), new { ID = passwordEntry.Id }, passwordEntry);
        }

        [HttpDelete]
        [Route("password/{id}")]
        public IActionResult DeletePassword(int id)
        {
            bool BOOL_IsJWtTokenRepudied;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();
            // Checks if the JWT token is repudied
            _memoryCache.TryGetValue(Request.Headers[HeaderNames.Authorization], out BOOL_IsJWtTokenRepudied);
            
            if (null == selectedCredential || BOOL_IsJWtTokenRepudied)
            {
                return Unauthorized();
            }

            // Extracts the PasswordEntry that has the received ID
            PasswordEntry passwordEntry = _vaultDbContext.Passwords.SingleOrDefault(password => password.Id == id && password.CredentialFK == selectedCredential.Id);
            if (null == passwordEntry)
            {
                return NotFound();
            }

            _vaultDbContext.Passwords.Remove(passwordEntry);
            _vaultDbContext.SaveChanges();

            return Ok();
        }

        [ActionName("ReadPasswordById")]
        public ActionResult<PasswordEntryReadDto> ReadPasswordById(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            // Extracts the PasswordEntry that has the received ID
            PasswordEntry passwordEntry = _vaultDbContext.Passwords.SingleOrDefault(password => password.Id == id);
            if (null == passwordEntry)
            {
                return NotFound();
            }


            // Returns the mapped PasswordEntry
            return _mapper.Map<PasswordEntryReadDto>(passwordEntry);
        }


        //https://stackoverflow.com/questions/16015548/tool-for-sending-multipart-form-data-request
        //https://www.c-sharpcorner.com/article/upload-download-files-in-asp-net-core-2-0/
    }
}
