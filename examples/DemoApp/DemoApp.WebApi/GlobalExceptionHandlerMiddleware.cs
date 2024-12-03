using Microsoft.AspNetCore.Mvc;

using System.Net;

/// <summary>
/// Middleware to handle all unhandled exceptions globally.
/// </summary>
public class GlobalExceptionHandlerMiddleware(RequestDelegate next, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = context.Response.StatusCode,
            Detail = environment.IsDevelopment() ? exception.Message : "An internal server error occurred.",
            Instance = context.Request.Path
        };

        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}