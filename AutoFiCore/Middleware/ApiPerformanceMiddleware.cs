using AutoFiCore.Data.Interfaces;

public class ApiPerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiPerformanceMiddleware> _logger;

    public ApiPerformanceMiddleware(RequestDelegate next, ILogger<ApiPerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var trackingService = context.RequestServices.GetRequiredService<IPerformanceTrackingService>();

        string endpoint = $"{context.Request.Method} {context.Request.Path}";
        int statusCode = context.Response.StatusCode;

        await trackingService.TrackAPIRequestAsync(endpoint, stopwatch.Elapsed, statusCode);
    }
}
