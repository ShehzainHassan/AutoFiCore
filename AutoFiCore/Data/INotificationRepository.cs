using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Data
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotification(Notification notification);
        Task<bool> HasAuctionStatusChangeNotificationBeenSentAsync(int userId, int auctionId, NotificationType type);
        Task<bool> HasAuctionWonNotificationBeenSentAsync(int userId, int auctionId);
        Task<bool> HasAuctionLostNotificationBeenSentAsync(int userId, int auctionId);
        Task<bool> HasReservePriceMetNotificationBeenSentAsync(int userId, int auctionId);
        Task<bool> HasAuctionExtendedNotificationBeenSentAsync(int userId, int auctionId);
        Task<bool> HasBidderCountNotificationBeenSentAsync(int userId, int auctionId);
        Task<bool> HasAutoBidNotificationBeenSentAsync(int userId, int auctionId, string message);
        Task<bool> HasAuctionEndNotificationBeenSentAsync(int userId, int auctionId);
        Task<PagedResult<NotificationDTO>> GetUserNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize);
        Task<Result<Notification>> MarkAsReadAsync(int notificationId);
        Task<NotificationDTO?> GetNotificationByIdAsync(int notificationId);
        Task<int> GetUnreadNotificationCountAsync(int userId);

    }
}
