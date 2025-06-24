using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AutoFiCore.Middleware;

public class RequestExecutionTimeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestExecutionTimeMiddleware> _logger;

    public RequestExecutionTimeMiddleware(RequestDelegate next, ILogger<RequestExecutionTimeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            
            // Log the execution time
            _logger.LogInformation(
                "API Execution Time: {ElapsedMs}ms | Path: {Path} | Method: {Method} | Status: {StatusCode}",
                elapsedMs,
                context.Request.Path,
                context.Request.Method,
                context.Response.StatusCode);
        }
    }
} 