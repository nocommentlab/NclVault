using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NclVaultAPIServer.Data;
using NclVaultAPIServer.DTOs.CredentialDTO;
using NclVaultAPIServer.DTOs.PasswordEntryDTO;
using NclVaultAPIServer.Models;
using NclVaultAPIServer.Utils;

namespace NclVaultAPIServer.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    
    public class VaultController : ControllerBase
    {
        #region Members
        private readonly VaultDbContext _vaultDbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        #endregion

        public VaultController(VaultDbContext vaultDbContext, IMapper mapper, IConfiguration configuration)
        {
            _vaultDbContext = vaultDbContext;
            _mapper = mapper;
            _configuration = configuration;

            
        }

        
        //GET /initvault/{STRING_Username} - 
        [HttpGet]
        [Route("initvault/{STRING_Username}")]
        public ActionResult<CredentialReadDto> InitVault(string STRING_Username)
        {
            // Checks if the vault has just an user
            if(_vaultDbContext.Credentials.Count() > 0)
            {
                // Returns 401
                return Unauthorized();
            }

            // Creates a Credential object
            Credential credential = new Credential();
            // Sets the Username passed
            credential.Username = STRING_Username;
            // Sets a random Password
            credential.Password = Guid.NewGuid().ToString();

            // Adds the element to Credential table and save
            _vaultDbContext.Credentials.Add(credential);
            _vaultDbContext.SaveChanges();

            // Return the stored Credential
            return Ok(_mapper.Map<CredentialReadDto>(credential));

        }

        //POST /initvault
        [HttpPost]
        [Route("initvault")]
        public ActionResult<InitResponse> InitVault([FromBody] CredentialCreateDto credentialCreateDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (_vaultDbContext.Credentials.Count() > 0)
            {
                return Unauthorized();
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
            return Ok(new InitResponse { Username = credential.Username, InitId = Guid.NewGuid().ToString()});
        }
        
        
        //POST /create/password
        [HttpPost]
        [Route("create/password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreatePassword([FromBody] PasswordEntryCreateDto passwordEntryCreateDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if (null == selectedCredential)
            {
                return Unauthorized();
            }

            /* Sets the encrypted password */
            passwordEntryCreateDto.Password = CryptoHelper.EncryptString(passwordEntryCreateDto.Password, Request.Headers["InitId"]);
            PasswordEntry passwordEntry = _mapper.Map<PasswordEntry>(passwordEntryCreateDto);
            
            _vaultDbContext.Passwords.Add(passwordEntry);
            _vaultDbContext.SaveChanges();
            
            return CreatedAtAction(nameof(ReadPasswordById),new { ID = passwordEntry.Id }, passwordEntry);
        }


        [ActionName("ReadPasswordById")]
        public ActionResult<PasswordEntryReadDto> ReadPasswordById(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            PasswordEntry passwordEntry = _vaultDbContext.Passwords.SingleOrDefault(password => password.Id == id);
            if(null == passwordEntry)
            {
                return NotFound();
            }



            return _mapper.Map<PasswordEntryReadDto>(passwordEntry); 
        }

        //GET read/password/{id}
        [HttpGet]
        [Route("read/password/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<PasswordEntryReadDto> DecryptedReadPasswordById(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            PasswordEntry passwordEntry = _vaultDbContext.Passwords.SingleOrDefault(password => password.Id == id);
            
            if (null == passwordEntry)
            {
                return NotFound();
            }
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if(null == selectedCredential)
            {
                return Unauthorized();
            }

            passwordEntry.Password = Utils.CryptoHelper.DecryptString(passwordEntry.Password, Request.Headers["InitId"]);

            return Ok(_mapper.Map<PasswordEntryReadDto>(passwordEntry));
        }

        //GET read/password
        [HttpGet]
        [Route("read/password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<List<PasswordEntryReadDto>> DecryptedReadPassword()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (_vaultDbContext.Passwords.Count() == 0)
            {
                return NotFound();
            }

            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if (null == selectedCredential)
            {
                return Unauthorized();
            }

            foreach (PasswordEntry passwordEntry in _vaultDbContext.Passwords)
            {
                passwordEntry.Password = Utils.CryptoHelper.DecryptString(passwordEntry.Password, Request.Headers["InitId"]);
            }
            

            return Ok(_mapper.Map<List<PasswordEntryReadDto>>(_vaultDbContext.Passwords));
        }

        
    }
}
