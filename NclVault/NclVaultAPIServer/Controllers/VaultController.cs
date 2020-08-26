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

        
        //GET /initvault/{STRING_Username}
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

        [HttpPost]
        [Route("initvault")]
        public IActionResult InitVault([FromBody] CredentialCreateDto credentialCreateDto)
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
            Credential credential = new Credential();
            // Sets the passed Username
            credential.Username = credentialCreateDto.Username;
            // Sets the passed Password - Sha256(<passed_password>+salt)
            credential.Password = CryptoHelper.ComputeSha256Hash(credentialCreateDto.Password.PadLeft(32, '*') + _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "PASSWORD_SALT"));

            // Adds the element to Credential table and save
            _vaultDbContext.Credentials.Add(credential);
            _vaultDbContext.SaveChanges();

            // Returns the stored Credential
            return Ok();
        }
        
        
        [HttpPost]
        [Route("create/password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreatePassword([FromBody] PasswordEntryCreateDto passwordEntryCreateDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            passwordEntryCreateDto.Password = Utils.CryptoHelper.EncryptString(passwordEntryCreateDto.Password, _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "ENTRY_ENC_KEY").ToString());
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

            passwordEntry.Password = Utils.CryptoHelper.DecryptString(passwordEntry.Password, _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "ENTRY_ENC_KEY").ToString());

            return Ok(_mapper.Map<PasswordEntryReadDto>(passwordEntry));
        }

        [HttpGet]
        [Route("test")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult Test()
        {
            
            return Ok();
        }
    }
}
