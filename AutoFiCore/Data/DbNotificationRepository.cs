using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Data
{
    public class DbNotificationRepository:INotificationRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DbNotificationRepository> _logger;
        public DbNotificationRepository(ApplicationDbContext dbContext, ILogger<DbNotificationRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public Task<Notification> CreateNotification(Notification notification)
        {
            _dbContext.Notifications.Add(notification);
            return Task.FromResult(notification);
        }
        public async Task<bool> NotificationExistsAsync(int userId, int auctionId, NotificationType type)
        {
            return await _dbContext.Notifications.AnyAsync(n =>
                n.UserId == userId &&
                n.AuctionId == auctionId &&
                n.NotificationType == type);
        }
        public async Task<PagedResult<NotificationDTO>> GetUserNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize)
        {
            var query = _dbContext.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDTO
                {
                    AuctionId = n.AuctionId,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<NotificationDTO>
            {
                Items = items,
                TotalItems = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}