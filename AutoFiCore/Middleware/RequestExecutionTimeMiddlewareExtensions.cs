using Microsoft.AspNetCore.Builder;

namespace AutoFiCore.Middleware;

public static class RequestExecutionTimeMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestExecutionTimeLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestExecutionTimeMiddleware>();
    }
} 