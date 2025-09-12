using AutoFiCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoFiCore.Controllers
{
    public abstract class SecureControllerBase : ControllerBase
    {
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        protected string? GetCorrelationId()
        {
            return HttpContext.Items["X-Correlation-ID"]?.ToString();
        }

        protected virtual bool IsUserContextValid(out int userId)
        {
            userId = GetUserId();
            return userId != 0;
        }
    }
}
