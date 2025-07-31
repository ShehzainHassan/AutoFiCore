using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotification(Notification notification);
        Task<bool> NotificationExistsAsync(int userId, int auctionId, NotificationType type);
        Task<PagedResult<NotificationDTO>> GetUserNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize);
    }
}
