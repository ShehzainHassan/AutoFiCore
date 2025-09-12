using AutoFiCore.Data.Interfaces;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace AutoFiCore.Services
{
   
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
            await _notifier.NotifyAuctionWon(userId, auction.AuctionId);
        }
        public async Task HandleAuctionLostAsync(Auction auction, int userId)
        {
            if (await _unitOfWork.Notification.HasAuctionLostNotificationBeenSentAsync(userId, auction.AuctionId))
                return;

            string vehicleInfo = auction.Vehicle != null
                ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}"
                : "the vehicle";

            string title = $"Auction Lost";
            string message = $"Unfortunately, you didn't win the auction for {vehicleInfo}. Better luck next time! Keep an eye out for similar listings.";

            await _notificationService.CreateNotificationAsync(
                userId,
                NotificationType.AuctionLost,
                title,
                message,
                auction.AuctionId
            );
            await _notifier.NotifyAuctionLost(userId, auction.AuctionId);   
        }
        public async Task HandleAuctionEndAsync(Auction auction)
        {
            if (auction == null || auction.Status != AuctionStatus.Ended)
                return;

            string vehicleInfo = auction.Vehicle != null
                ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}"
                : "the vehicle";

            string endTitle = "Auction Ended";
            string endMessage = $"The auction for {vehicleInfo} has ended.";

            var uniqueBidderIds = await _unitOfWork.Bids.GetUniqueBidderIdsAsync(auction.AuctionId);
            if (uniqueBidderIds == null || uniqueBidderIds.Count == 0)
                return;

            int? winningUserId = await _unitOfWork.Bids.GetHighestBidderIdAsync(auction.AuctionId);

            foreach (var userId in uniqueBidderIds)
            {
                bool alreadySentEnd = await _unitOfWork.Notification.HasAuctionEndNotificationBeenSentAsync(userId, auction.AuctionId);
                if (!alreadySentEnd)
                {
                    await _notificationService.CreateNotificationAsync(
                        userId,
                        NotificationType.AuctionEnd,
                        endTitle,
                        endMessage,
                        auction.AuctionId
                    );
                }
            }

            if (auction.IsReserveMet && winningUserId.HasValue)
            {
                foreach (var userId in uniqueBidderIds)
                {
                    if (userId == winningUserId.Value)
                    {
                        await HandleAuctionWonAsync(auction, userId);
                    }
                    else
                    {
                        await HandleAuctionLostAsync(auction, userId);
                    }
                }
            }
            await _notifier.NotifyAuctionEnd(auction);
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
        public async Task HandleBidderCountUpdate(Auction auction, List<int> previousBidders, List<int> updatedBidders)
        {
            var newBidders = updatedBidders.Except(previousBidders).ToList();
            if (!newBidders.Any())
                return;

            string vehicleInfo = auction.Vehicle != null
                ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}"
                : "an auctioned vehicle";

            foreach (var newBidderId in newBidders)
            {
                var newUser = await _unitOfWork.Users.GetUserByIdAsync(newBidderId);
                if (newUser == null) continue;

                string title = "New Bidder Joined";
                string message = $"{newUser.Name} has joined the auction for {vehicleInfo}.";

                foreach (var previousBidderId in previousBidders)
                {
                    if (previousBidderId == newBidderId) continue;

                    await _notificationService.CreateNotificationAsync(
                        previousBidderId,
                        NotificationType.BidderCountUpdate,
                        title,
                        message,
                        auction.AuctionId
                    );
                }
            }

            await _notifier.NotifyBidderCount(auction.AuctionId, updatedBidders.Count);
        }
        public async Task HandleAuctionStatusChangedAsync(Auction auction, AuctionStatus previousStatus)
        {
            if (auction == null || auction.Status == previousStatus)
                return;

            if (previousStatus == AuctionStatus.PreviewMode && auction.Status == AuctionStatus.Active)
            {
                var watchers = await _unitOfWork.Watchlist.GetAuctionWatchersAsync(auction.AuctionId);
                if (watchers == null || watchers.Count == 0)
                    return;

                var userIds = watchers.Select(w => w.UserId).Distinct().ToList();

                string vehicleInfo = auction.Vehicle != null
                    ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}"
                    : "the vehicle";

                string title = "Auction is Now Live!";
                string message = $"The auction for {vehicleInfo} is now live. Place your bids now!";
                var notificationType = NotificationType.AuctionStart;

                foreach (var userId in userIds)
                {
                    bool alreadyNotified = await _unitOfWork.Notification
                        .HasAuctionStatusChangeNotificationBeenSentAsync(userId, auction.AuctionId, notificationType);

                    if (!alreadyNotified)
                    {
                        await _notificationService.CreateNotificationAsync(
                            userId,
                            notificationType,
                            title,
                            message,
                            auction.AuctionId
                        );

                        await _notifier.NotifyAuctionStatusChanged(userId, auction.AuctionId, auction.Status.ToString());
                    }
                }
            }
        }
        public async Task HandleAutoBidAsync(int auctionId, int userId, decimal amount)
        {
            var auction = await _unitOfWork.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return;

            string vehicleInfo = auction.Vehicle != null
                ? $"{auction.Vehicle.Year} {auction.Vehicle.Make} {auction.Vehicle.Model}"
                : "a vehicle";

            string title = "AutoBid Executed";
            string message = $"Your AutoBid has been executed on {vehicleInfo} with a bid of {amount}.";

            bool alreadyNotified = await _unitOfWork.Notification
                .HasAutoBidNotificationBeenSentAsync(userId, auctionId, message);

            if (!alreadyNotified)
            {
                await _notificationService.CreateNotificationAsync(
                    userId,
                    NotificationType.AutoBidExecuted,
                    title,
                    message,
                    auctionId
                );
            }

            await _notifier.NotifyAutoBidExecuted(userId, auctionId, amount);
        }
    }
}
