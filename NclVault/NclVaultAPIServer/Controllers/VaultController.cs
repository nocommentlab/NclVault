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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vaultDbContext">The DB context</param>
        /// <param name="mapper">The Mapper service</param>
        /// <param name="configuration">The application configuration</param>
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
            if (_vaultDbContext.Credentials.Count() > 0)
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

            // Checks if there is just another registered user
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
            return Ok(new InitResponse { Username = credential.Username, InitId = Guid.NewGuid().ToString() });
        }


        //POST /create/password
        [HttpPost]
        [Route("password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreatePassword([FromBody] PasswordEntryCreateDto passwordEntryCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if (null == selectedCredential)
            {
                return Unauthorized();
            }

            /* Sets the encrypted password using the InitId request header parameter as key*/
            //passwordEntryCreateDto.Password = CryptoHelper.EncryptString(passwordEntryCreateDto.Password, Request.Headers["InitId"]);
            passwordEntryCreateDto.Password = EncryptProvider.AESEncrypt(passwordEntryCreateDto.Password, Request.Headers["InitId"]);
            PasswordEntry passwordEntry = _mapper.Map<PasswordEntry>(passwordEntryCreateDto);

            // Adds the entry password to EF and writes to the databae
            _vaultDbContext.Passwords.Add(passwordEntry);
            _vaultDbContext.SaveChanges();

            // After, redirect the browser to the ReadPasswordById Action(see the below function)
            return CreatedAtAction(nameof(ReadPasswordById), new { ID = passwordEntry.Id }, passwordEntry);
        }

        //PUT /update/password/{id}
        [HttpPut]
        [Route("password/{id}")]
        public IActionResult UpdatePassword(int id, [FromBody] PasswordEntryCreateDto passwordEntryCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if (null == selectedCredential)
            {
                return Unauthorized();
            }

            // Extract the password entry with the requested ID
            PasswordEntry passwordEntry = _vaultDbContext.Passwords.FirstOrDefault(element => element.Id == id);
            if (null == passwordEntry)
            {
                return NotFound();
            }

            /* Sets the encrypted password using the InitId request header parameter as key*/
            //passwordEntryCreateDto.Password = CryptoHelper.EncryptString(passwordEntryCreateDto.Password, Request.Headers["InitId"]);
            passwordEntryCreateDto.Password = EncryptProvider.AESEncrypt(passwordEntryCreateDto.Password, Request.Headers["InitId"]);

            _mapper.Map(passwordEntryCreateDto, passwordEntry);
            _vaultDbContext.SaveChanges();


            return CreatedAtAction(nameof(ReadPasswordById), new { ID = passwordEntry.Id }, passwordEntry);
        }

        [HttpDelete]
        [Route("password/{id}")]
        public IActionResult DeletePassword(int id)
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

        //GET read/password/{id}
        [HttpGet]
        [Route("password/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<PasswordEntryReadDto> DecryptedReadPasswordById(int id)
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

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if (null == selectedCredential)
            {
                return Unauthorized();
            }

            // Decrypts the password using the InitId request header parameter as key
            passwordEntry.Password = EncryptProvider.AESDecrypt(passwordEntry.Password, Request.Headers["InitId"]);

            // Returns the PasswordEntry object
            return Ok(_mapper.Map<PasswordEntryReadDto>(passwordEntry));
        }

        //GET read/password
        [HttpGet]
        [Route("password")]
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

            // Extract the Credential element that has the same username received
            Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if (null == selectedCredential)
            {
                return Unauthorized();
            }

            // Iterates over all PasswordEntry elements
            foreach (PasswordEntry passwordEntry in _vaultDbContext.Passwords)
            {
                // Decrypts the password using the InitId request header parameter as key
                passwordEntry.Password = EncryptProvider.AESDecrypt(passwordEntry.Password, Request.Headers["InitId"]);
            }

            // Returns the Mapped List<PasswordEntry> objects
            return Ok(_mapper.Map<List<PasswordEntryReadDto>>(_vaultDbContext.Passwords));
        }

        //https://stackoverflow.com/questions/16015548/tool-for-sending-multipart-form-data-request
        //https://www.c-sharpcorner.com/article/upload-download-files-in-asp-net-core-2-0/
        [HttpPost]
        [Route("files/")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            byte[] vBYTE_EncryptedPayload;

            // Checks if the received file is null or empty
            if (file == null || file.Length == 0)
                return BadRequest();

            // Extract the Credential element that has the same username received
            /*Credential selectedCredential = _vaultDbContext.Credentials.Where(credential => credential.Username.Equals(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("username").Value)).FirstOrDefault();

            if (null == selectedCredential)
            {
                return Unauthorized();
            }*/

            /* Encrypts the content */
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                vBYTE_EncryptedPayload = EncryptProvider.AESEncrypt(memoryStream.GetBuffer(), Request.Headers["InitId"]);

            }

            /* Writes the encrypted buffer to file */
            using (var stream = new FileStream(file.FileName + ".enc", FileMode.Create))
            {
                await stream.WriteAsync(vBYTE_EncryptedPayload);
            }


            return Ok();
        }

        [HttpGet]
        [Route("files/{filename}")]
        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null || Request.Headers["InitId"].Count == 0)
                return BadRequest();

            var encryptedPayload = new MemoryStream();
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                await stream.CopyToAsync(encryptedPayload);
            }

            byte[] decryptedPayload = EncryptProvider.AESDecrypt(encryptedPayload.ToArray(), Request.Headers["InitId"]);

            var memory = new MemoryStream();
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(decryptedPayload, "image/jpeg", "decrypted.jpg");
        }


        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".zip", "application/zip"}
            };
        }
    }
}
