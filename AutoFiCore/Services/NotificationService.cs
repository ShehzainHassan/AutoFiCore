using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoFiCore.Services
{
    public interface INotificationService
    {
        Task<Result<Notification>> CreateNotificationAsync(int userId, NotificationType type, string title, string message, int? auctionId = null);
        Task<PagedResult<NotificationDTO>> GetUserNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize);
        Task<Result<Notification>> MarkAsReadAsync(int notificationId);
        Task<Result<NotificationDTO>> GetNotificationByIdAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public NotificationService(ILogger<NotificationService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<Notification>> CreateNotificationAsync(int userId, NotificationType type, string title, string message, int? auctionId = null)
        {
           var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null)
                return Result<Notification>.Failure("User not found");
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    NotificationType = type,
                    Title = title,
                    Message = message,
                    AuctionId = auctionId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Priority = NotificationPriority.Normal 
                };

                var createdNotification = await _unitOfWork.Notification.CreateNotification(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Notification created for UserId: {UserId}, Type: {Type}", userId, type);
                return Result<Notification>.Success(createdNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification for UserId: {UserId}", userId);
                throw;
            }
        }
        public async Task<Result<Notification>> MarkAsReadAsync(int notificationId)
        {
            return await _unitOfWork.Notification.MarkAsReadAsync(notificationId);
        }
        public Task<PagedResult<NotificationDTO>> GetUserNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize)
        {
            return _unitOfWork.Notification.GetUserNotificationsAsync(userId, unreadOnly, page, pageSize);
        }
        public async Task<Result<NotificationDTO>> GetNotificationByIdAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notification.GetNotificationByIdAsync(notificationId);
            if (notification == null)
                return Result<NotificationDTO>.Failure("Notification not found");

            return Result<NotificationDTO>.Success(notification);
        }
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork.Notification.GetUnreadNotificationCountAsync(userId);
        }
    }
}
