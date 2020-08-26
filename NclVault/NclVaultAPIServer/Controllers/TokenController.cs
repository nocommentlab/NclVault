using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NclVaultAPIServer.Data;
using NclVaultAPIServer.DTOs.CredentialDTO;
using NclVaultAPIServer.Utils;

namespace NclVaultAPIServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly VaultDbContext _vaultDbContext;
        private readonly IConfiguration _configuration;

        public TokenController(VaultDbContext vaultDbContext, IConfiguration configuration)
        {
            _vaultDbContext = vaultDbContext;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult DoLogin([FromBody] CredentialReadDto credentialReadDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }


            string STRING_CalculatedSHA256Password = CryptoHelper.ComputeSha256Hash(credentialReadDto.Password.PadLeft(32, '*') + _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "PASSWORD_SALT"));
            /* Checks the credential */
            if (!_vaultDbContext.Credentials.Any(cred => cred.Username == credentialReadDto.Username &&
                                                         cred.Password == STRING_CalculatedSHA256Password))
            {
                return Unauthorized();
            }

            //L'utente ha fornito credenziali valide
            //creiamo per lui una ClaimsIdentity
            var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            //Aggiungiamo uno o più claim relativi all'utente loggato
            identity.AddClaim(new Claim("username", credentialReadDto.Username));
            //Incapsuliamo l'identità in una ClaimsPrincipal e associamola alla richiesta corrente
            HttpContext.User = new ClaimsPrincipal(identity);

            return Ok();
        }
    }
}
