using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            System.ComponentModel.DataAnnotations.ValidationException => new { message = exception.Message, statusCode = 400 },
            UnauthorizedAccessException => new { message = "Unauthorized", statusCode = 401 },
            ArgumentNullException => new { message = "Resource not found", statusCode = 404 },
            _ => new { message = "Internal Server Error", statusCode = 500 }
        };

        response.StatusCode = errorResponse.statusCode;

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
