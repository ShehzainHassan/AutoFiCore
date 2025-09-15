using AutoFiCore.Data.Interfaces;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Provides endpoints for notifications management.
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : SecureControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class with the specified notification service.
        /// </summary>
        /// <param name="notificationService">The service used to handle notification-related operations.</param>
        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a paginated list of notifications for the authenticated user.
        /// </summary>
        /// <param name="unreadOnly">If true, filters to only unread notifications.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>List of notifications.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetNotifications called. CorrelationId={CorrelationId}, UserId={UserId}, UnreadOnly={UnreadOnly}, Page={Page}, PageSize={PageSize}",
                correlationId, userId, unreadOnly, page, pageSize);

            var result = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Marks a specific notification as read.
        /// </summary>
        [Authorize]
        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized("Invalid or missing user ID");

            var correlationId = GetCorrelationId();
            _logger.LogInformation("MarkAsRead called. CorrelationId={CorrelationId}, UserId={UserId}, NotificationId={NotificationId}", correlationId, userId, id);

            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result.IsSuccess)
                return NotFound("Notification not found or already read.");

            return Ok(new
            {
                message = "Notification marked as read",
                data = result.Value
            });
        }

        /// <summary>
        /// Retrieves a specific notification by its ID.
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized("Invalid or missing user ID");

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetNotificationById called. CorrelationId={CorrelationId}, UserId={UserId}, NotificationId={NotificationId}", correlationId, userId, id);

            var result = await _notificationService.GetNotificationByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Returns the count of unread notifications for the authenticated user.
        /// </summary>
        [Authorize]
        [DisableRateLimiting]
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized("Invalid or missing user ID");

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetUnreadNotificationCount called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count.Value);
        }
    }
}
