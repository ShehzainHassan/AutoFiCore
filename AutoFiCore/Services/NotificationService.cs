using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoFiCore.Services
{
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
            try
            {
                var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for notification creation: {UserId}", userId);
                    return Result<Notification>.Failure("User not found.");
                }

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
                return Result<Notification>.Failure("Unexpected error occurred while creating notification.");
            }
        }
        public async Task<Result<Notification>> MarkAsReadAsync(int notificationId)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        var result = await _unitOfWork.Notification.MarkAsReadAsync(notificationId);

                        if (!result.IsSuccess)
                        {
                            _logger.LogWarning("Failed to mark notification {NotificationId} as read: {Error}", notificationId, result.Error);
                            return result;
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                        return Result<Notification>.Failure("Unexpected error occurred while marking notification as read.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution strategy failed while marking notification {NotificationId} as read", notificationId);
                return Result<Notification>.Failure("Execution strategy failed.");
            }
        }
        public Task<PagedResult<NotificationDTO>> GetUserNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize)
        {
            return _unitOfWork.Notification.GetUserNotificationsAsync(userId, unreadOnly, page, pageSize);
        }
        public async Task<Result<NotificationDTO>> GetNotificationByIdAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notification.GetNotificationByIdAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification not found: {NotificationId}", notificationId);
                return Result<NotificationDTO>.Failure("Notification not found.");
            }

            return Result<NotificationDTO>.Success(notification);
        }
        public async Task<Result<int>> GetUnreadCountAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found while fetching unread notification count: {UserId}", userId);
                    return Result<int>.Failure("User not found.");
                }

                int count = await _unitOfWork.Notification.GetUnreadNotificationCountAsync(userId);
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching unread notification count for user {UserId}", userId);
                return Result<int>.Failure("Unexpected error occurred while retrieving unread count.");
            }
        }
    }
}