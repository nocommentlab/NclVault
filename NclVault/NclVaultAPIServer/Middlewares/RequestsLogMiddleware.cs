using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Middlewares
{
    /// <summary>
    /// NOT YET IMPLEMENTED - I Think that I can use this middleware to logs access to ELK? 😎
    /// https://elanderson.net/2019/12/log-requests-and-responses-in-asp-net-core-3/
    /// </summary>
    public class RequestsLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestsLogMiddleware> _logger;

        public RequestsLogMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<RequestsLogMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {
                var request = context.Request;
                var stream = new StreamReader(request.Body);
                var body = stream.ReadToEndAsync();
                stream.Close();
                _logger.LogInformation(
                    "Request {method} {url} => {statusCode} {body}",
                    context.Request?.Method,
                    context.Request?.Path.Value + context.Request?.QueryString.Value,
                    context.Response?.StatusCode,
                    body);
            }

            /*context.Response.OnStarting(() => {
                Console.WriteLine(context.Request.Query.Count.ToString()) ; 
                return Task.CompletedTask; 
            });

            await _next.Invoke(context);*/

        }
    }
}
