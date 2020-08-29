using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NclVaultAPIServer.Data;
using NclVaultAPIServer.Middlewares;

namespace NclVaultAPIServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Configures automapper configuration assembly
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Adds the controllers
            services.AddControllers();

            // Configures the authentication section with JWT
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,

                    //Importante: indicare lo stesso Issuer, Audience e chiave segreta
                    //usati anche nel JwtTokenMiddleware
                    ValidIssuer = Configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:ISSUER"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(Configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:SIGNING_KEY"))
                  ),
                    //Tolleranza sulla data di scadenza del token
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddScoped<DbConnProperties, DbConnProperties>();

            services.AddDbContext<VaultDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<JwtTokenMiddleware>();
            
            app.UseAuthentication(); // this one first

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            
        }
    }
}
