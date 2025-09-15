using AutoFiCore.Data.Interfaces;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoFiCore.Middleware
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<TokenRefreshMiddleware> _logger;

        public TokenRefreshMiddleware(RequestDelegate next, IConfiguration config, ILogger<TokenRefreshMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IRefreshTokenService refreshTokenService, ITokenProvider tokenProvider, IUserService userService)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            var accessToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var principal = JwtHelper.ValidateToken(accessToken, _config);

            if (principal == null)
            {
                var refreshToken = context.Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Access token expired and no refresh token provided.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Access token expired and no refresh token provided.");
                    return;
                }

                var stored = await refreshTokenService.GetAsync(refreshToken);
                if (stored == null || stored.Expires < DateTime.UtcNow || stored.IsRevoked)
                {
                    _logger.LogWarning("Refresh token expired or revoked.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Refresh token expired or revoked.");
                    return;
                }

                var user = await userService.GetUserByIdAsync(stored.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("User not found.");
                    return;
                }

                var newAccessToken = tokenProvider.CreateAccessToken(user);
                context.Response.Headers["X-New-Access-Token"] = newAccessToken;

                _logger.LogInformation("Access token refreshed for UserId={UserId}", user.Id);
            }

            await _next(context);
        }
    }
}