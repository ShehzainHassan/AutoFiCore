using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Provides endpoints for notifications management.
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class with the specified notification service.
        /// </summary>
        /// <param name="notificationService">The service used to handle notification-related operations.</param>
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Retrieves a paginated list of notifications for the authenticated user.
        /// </summary>
        /// <param name="unreadOnly">If true, filters to only unread notifications.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>List of notifications.</returns>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                  User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user ID");
            }

            var result = await _notificationService.MarkAsReadAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound("Notification not found or already read.");
            }

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
            var result = await _notificationService.GetNotificationByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Returns the count of unread notifications for the authenticated user.
        /// </summary>
        [Authorize]
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadNotificationCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user ID");
            }
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }
    }
}
