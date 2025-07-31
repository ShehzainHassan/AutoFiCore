using AutoFiCore.Data;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace AutoFiCore.Services
{
    public interface IAuctionLifecycleService
    {
        Task HandleAuctionWonAsync(Auction auction, int userId);
    }
    public class AuctionLifecycleService:IAuctionLifecycleService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionLifecycleService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AuctionLifecycleService(INotificationService notificationService, ILogger<AuctionLifecycleService> logger, IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
       

        public async Task HandleAuctionWonAsync(Auction auction, int userId)
        {
            if (await _unitOfWork.Notification.NotificationExistsAsync(userId, auction.AuctionId, NotificationType.AuctionWon))
                return;

            string vehicleInfo = auction.Vehicle != null ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}" : "the vehicle";
            string title = $"Results are in for {vehicleInfo}";

            string message = $"Congratulations! You've won the auction for {vehicleInfo} with your highest bid of {auction.CurrentPrice}. Please complete the payment within 48 hours.";

            await _notificationService.CreateNotificationAsync(
                userId,
                NotificationType.AuctionWon,
                title,
                message,
                auction.AuctionId
            );
        }

    }
}
