using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NForza.Cyrus.WebApi;

public static class JsonExceptionHandlerExtensions
{
    public class JsonExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JsonExceptionHandlingMiddleware> _logger;

        public JsonExceptionHandlingMiddleware(RequestDelegate next, ILogger<JsonExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (ex.InnerException is JsonException jsonException)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body.");

                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                var error = new
                {
                    error = "Invalid JSON in request body.",
                    path = jsonException.Path,
                    message = jsonException.Message
                };

                await context.Response.WriteAsJsonAsync(error);
            }
        }
    }


    public static IApplicationBuilder UseJsonExceptionHandler(this WebApplication app)
    {
        return app.UseMiddleware<JsonExceptionHandlingMiddleware>();
    }
}