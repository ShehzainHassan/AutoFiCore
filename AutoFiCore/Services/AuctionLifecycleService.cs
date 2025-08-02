using AutoFiCore.Data;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace AutoFiCore.Services
{
    public interface IAuctionLifecycleService
    {
        Task HandleAuctionWonAsync(Auction auction, int userId);
        Task HandleOutbid(Auction auction, int? previousBidderId);
    }
    public class AuctionLifecycleService:IAuctionLifecycleService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionLifecycleService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuctionNotifier _notifier;


        public AuctionLifecycleService(INotificationService notificationService, ILogger<AuctionLifecycleService> logger, IUnitOfWork unitOfWork, IAuctionNotifier notifier)
        {
            _notificationService = notificationService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _notifier = notifier;
        }

        public async Task HandleOutbid(Auction auction, int? previousBidderId)
        {
            if (previousBidderId == null)
                return;
            string message = $"You got outbid on {auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model} auction!.";
            await _notificationService.CreateNotificationAsync(previousBidderId.Value, NotificationType.Outbid, "Outbid", message, auction.AuctionId);
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
