using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface INotificationService
    {
        Task<Result<Notification>> CreateNotificationAsync(int userId, NotificationType type, string title, string message, int? auctionId = null);
        Task<PagedResult<NotificationDTO>> GetUserNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize);
        Task<Result<Notification>> MarkAsReadAsync(int notificationId);
        Task<Result<NotificationDTO>> GetNotificationByIdAsync(int notificationId);
        Task<Result<int>> GetUnreadCountAsync(int userId);
    }
}
