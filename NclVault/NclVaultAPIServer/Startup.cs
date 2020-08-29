using System;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,

                    // Declares he JWTProperties. Use the same inside the Middleware
                    ValidIssuer = Configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:ISSUER"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(Configuration.GetValue<string>("NCLVaultConfiguration:JWTConfiguration:SIGNING_KEY"))
                  ),

                    // Disables all time tollerance
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Adds the database connection properties to services scoped pool
            services.AddScoped<DbConnProperties, DbConnProperties>();
            // Adds the database contect
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

            // Configures the JWTMiddleware
            app.UseMiddleware<JwtTokenMiddleware>();

            // Configures the authentication campability.
            // Declare this befor the Authorization
            app.UseAuthentication();

            // Configures the authorization campability
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
