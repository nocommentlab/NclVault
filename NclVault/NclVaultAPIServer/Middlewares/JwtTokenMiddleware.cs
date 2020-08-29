using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Middlewares
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration _configuration;

        public JwtTokenMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            _configuration = configuration;
        }
        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() => {
                var identity = context.User.Identity as ClaimsIdentity;

                //Se la richiesta era autenticata, allora creiamo un nuovo token JWT
                if (identity.IsAuthenticated)
                {
                    //Il client potrà usare questo nuovo token nella sua prossima richiesta
                    var token = CreateTokenForIdentity(identity);
                    //Usiamo l'intestazione X-Token, ma non è obbligatorio che si chiami così
                    context.Response.Headers.Add("X-Token", token);
                }
                return Task.CompletedTask;
            });
            await next.Invoke(context);
        }

        //In questo metodo creiamo il token a partire dai claim della ClaimsIdentity
        private StringValues CreateTokenForIdentity(ClaimsIdentity identity)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:SIGNING_KEY")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:ISSUER"),
              claims: identity.Claims,
              expires: DateTime.Now.AddMinutes(_configuration.GetValue<int>("NCLVaultConfiguration:JWTConfiguration:TOKEN_INACTIVITY_EXPIRATION")),
              signingCredentials: credentials
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            var serializedToken = tokenHandler.WriteToken(token);
            return serializedToken;
        }
    }
}
