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
    /// <summary>
    /// The JWTMiddleware class
    /// </summary>
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
            context.Response.OnStarting(() =>
            {
                var identity = context.User.Identity as ClaimsIdentity;

                // Checks if the request is authenticated
                if (identity.IsAuthenticated)
                {

                    // Generates the new JWT token that will be used from the client
                    var token = CreateTokenForIdentity(identity);

                    // Adds the new Token inside the X-Token server response header
                    context.Response.Headers.Add("X-Token", token);
                }
                return Task.CompletedTask;
            });
            await next.Invoke(context);
        }

        /// <summary>
        /// Generates the token with the given ClaimsIdentity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private StringValues CreateTokenForIdentity(ClaimsIdentity identity)
        {
            // Generates the SymmetricSecurityKey used to sign the token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:SIGNING_KEY")));
            // Uses the HMAC SHA256 signing alghoritm
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // Configures the JWT Token
            var token = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:ISSUER"),
              claims: identity.Claims,
              expires: DateTime.Now.AddMinutes(_configuration.GetValue<int>("NCLVaultConfiguration:JWTConfiguration:TOKEN_INACTIVITY_EXPIRATION_MINUTES")),
              signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var serializedToken = tokenHandler.WriteToken(token);
            return serializedToken;
        }
    }
}
