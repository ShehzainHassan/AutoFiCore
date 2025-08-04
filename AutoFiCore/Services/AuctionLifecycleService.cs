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
        Task HandleNewBid(int auctionId);
        Task HandleReserveMet(Auction auction, int? newBidUserId = null);
        Task HandleAuctionExtended(Auction auction);
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
        public async Task HandleNewBid(int auctionId)
        {
            await _notifier.NotifyNewBid(auctionId);
        }
        public async Task HandleOutbid(Auction auction, int? previousBidderId)
        {
            if (previousBidderId == null)
                return;
            string message = $"You got outbid on {auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model} auction!.";
            await _notificationService.CreateNotificationAsync(previousBidderId.Value, NotificationType.Outbid, "Outbid", message, auction.AuctionId);
            await _notifier.NotifyOutbid(previousBidderId.Value, auction.AuctionId);
        }
        public async Task HandleAuctionWonAsync(Auction auction, int userId)
        {
            if (await _unitOfWork.Notification.HasAuctionWonNotificationBeenSentAsync(userId, auction.AuctionId))
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
        public async Task HandleReserveMet(Auction auction, int? newBidUserId = null)
        {
            if (auction == null || auction.ReservePrice <= 0)
                return;

            string vehicleInfo = auction.Vehicle != null
                ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}"
                : "the vehicle";

            string title = "Reserve Price Met for Auction";
            string message = $"The reserve price has been met for {vehicleInfo}.";

            var uniqueBidderIds = await _unitOfWork.Bids.GetUniqueBidderIdsAsync(auction.AuctionId);

            if (uniqueBidderIds == null || uniqueBidderIds.Count == 0)
                return;

            foreach (var userId in uniqueBidderIds)
            {
                if (newBidUserId.HasValue && userId != newBidUserId.Value)
                    continue;

                bool alreadyNotified = await _unitOfWork.Notification
                    .HasReservePriceMetNotificationBeenSentAsync(userId, auction.AuctionId);

                if (!alreadyNotified)
                {
                    await _notificationService.CreateNotificationAsync(
                        userId,
                        NotificationType.ReservePriceMet,
                        title,
                        message,
                        auction.AuctionId
                    );
                }
            }

            if (!newBidUserId.HasValue)
            {
                foreach (var userId in uniqueBidderIds)
                {
                    bool alreadyNotified = await _unitOfWork.Notification
                        .HasReservePriceMetNotificationBeenSentAsync(userId, auction.AuctionId);

                    if (!alreadyNotified)
                    {
                        await _notificationService.CreateNotificationAsync(
                            userId,
                            NotificationType.ReservePriceMet,
                            title,
                            message,
                            auction.AuctionId
                        );
                    }
                }
            }
            await _notifier.NotifyReserveMet(auction.AuctionId); 
        }
        public async Task HandleAuctionExtended(Auction auction)
        {
            if (auction == null)
                return;

            string vehicleInfo = auction.Vehicle != null
                ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}"
                : "the vehicle";

            string title = "Auction Extended";
            string message = $"The auction for {vehicleInfo} has been extended.";

            var uniqueBidderIds = await _unitOfWork.Bids.GetUniqueBidderIdsAsync(auction.AuctionId);
            if (uniqueBidderIds == null || uniqueBidderIds.Count == 0)
                return;

            foreach (var userId in uniqueBidderIds)
            {
                bool alreadyNotified = await _unitOfWork.Notification
                    .HasAuctionExtendedNotificationBeenSentAsync(userId, auction.AuctionId);

                if (!alreadyNotified)
                {
                    await _notificationService.CreateNotificationAsync(
                        userId,
                        NotificationType.AuctionExtended,
                        title,
                        message,
                        auction.AuctionId
                    );
                }
            }

            await _notifier.NotifyAuctionExtended(auction.AuctionId, auction.EndUtc);
        }
    }
}
