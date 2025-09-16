using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using AutoFiCore.Data.Interfaces;

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
        var trackingService = context.RequestServices.GetRequiredService<IPerformanceTrackingService>();

        try
        {
            await _next(context);

            if (context.Response.StatusCode >= 400)
            {
                string reason = ReasonPhrases.GetReasonPhrase(context.Response.StatusCode);
                await trackingService.TrackErrorEventAsync(
                    context.Response.StatusCode,
                    reason
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            var statusCode = ex switch
            {
                System.ComponentModel.DataAnnotations.ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentNullException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            string reason = ReasonPhrases.GetReasonPhrase(statusCode);

            await trackingService.TrackErrorEventAsync(statusCode, reason);

            await HandleExceptionAsync(context, ex, statusCode);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = statusCode;

        var errorResponse = new
        {
            message = exception switch
            {
                System.ComponentModel.DataAnnotations.ValidationException => exception.Message,
                UnauthorizedAccessException => "Unauthorized",
                ArgumentNullException => "Resource not found",
                _ => "Internal Server Error"
            },
            statusCode
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
