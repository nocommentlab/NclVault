using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Middlewares
{
    /// <summary>
    /// NOT YET IMPLEMENTED - I Think that I can use this middleware to logs access to ELK? 😎
    /// https://elanderson.net/2019/12/log-requests-and-responses-in-asp-net-core-3/
    /// </summary>
    public static class RequestsLogMiddlewareExtension
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestsLogMiddleware>();
        }
    }
}
