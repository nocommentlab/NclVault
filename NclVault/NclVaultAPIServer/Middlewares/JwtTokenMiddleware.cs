using Microsoft.AspNetCore.Http;
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
        public JwtTokenMiddleware(RequestDelegate next)
        {
            this.next = next;
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MiaChiaveSegreta"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: "NCLVault",
              claims: identity.Claims,
              expires: DateTime.Now.AddMinutes(20),
              signingCredentials: credentials
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            var serializedToken = tokenHandler.WriteToken(token);
            return serializedToken;
        }
    }
}
