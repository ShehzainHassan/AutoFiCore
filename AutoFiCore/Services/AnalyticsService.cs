using AutoFiCore.Data.Interfaces;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoFiCore.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(IUnitOfWork uow, ILogger<AnalyticsService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Tracks a generic analytics event.
        /// </summary>
        public async Task<Result<bool>> TrackEventAsync(AnalyticsEventType type, int? userId, int? auctionId, Dictionary<string, object> data, string source)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        if (!Enum.TryParse<AnalyticsSource>(source, out var parsedSource))
                            parsedSource = AnalyticsSource.Web;

                        var analyticsEvent = new AnalyticsEvent
                        {
                            EventType = type,
                            UserId = userId,
                            AuctionId = auctionId,
                            EventData = JsonSerializer.Serialize(data),
                            Source = parsedSource,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _uow.Analytics.AddEventAsync(analyticsEvent);
                        await _uow.SaveChangesAsync();

                        await _uow.CommitTransactionAsync();

                        return Result<bool>.Success(true);
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _logger.LogError(ex,"Failed to track analytics event. Type={Type}, UserId={UserId}, AuctionId={AuctionId}", type, userId, auctionId);

                        return Result<bool>.Failure("Failed to track analytics event.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Execution strategy failed while tracking analytics event. Type={Type}, UserId={UserId}, AuctionId={AuctionId}",
                    type, userId, auctionId);

                return Result<bool>.Failure("Failed to track analytics event.");
            }
        }


        public async Task<Result<bool>> TrackAuctionViewAsync(int auctionId, int? userId, string source)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            var vehicle = auction!.Vehicle;
            var message = $"Viewed Auction for {vehicle.Year} {vehicle.Make} {vehicle.Model}";


            return await TrackEventAsync(
                AnalyticsEventType.AuctionView,
                userId,
                auctionId,
                new Dictionary<string, object>
                {
                    { "Viewed Auction", message }
                },
                source
            );
        }

        public async Task<Result<bool>> TrackBidEventAsync(int auctionId, int userId, decimal bidAmount)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            var vehicle = auction!.Vehicle;

            var message = $"Placed Bid on {vehicle.Year} {vehicle.Make} {vehicle.Model} Auction, Bid Amount: {bidAmount}";

            return await TrackEventAsync(
                AnalyticsEventType.BidPlaced,
                userId,
                auctionId,
                new Dictionary<string, object> { { "Placed Bid", message } },
                "Web"
            );
        }

        public async Task<Result<bool>> TrackAuctionCompletionAsync(int auctionId, bool isSuccessful, decimal finalPrice)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            var vehicle = auction!.Vehicle;

            string message = isSuccessful
                ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model} Auction completed, Final Price: {finalPrice}"
                : $"{vehicle.Year} {vehicle.Make} {vehicle.Model} Auction did not sell, Final Price: {finalPrice}";

            return await TrackEventAsync(
                AnalyticsEventType.AuctionCompleted,
                null,
                auctionId,
                new Dictionary<string, object> { { "Auction Result", message } },
                "Web"
            );
        }

        public async Task<Result<bool>> TrackPaymentCompletedAsync(int auctionId, int? userId, decimal finalPayment)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            var vehicle = auction!.Vehicle;

            var message = $"Amount Paid for {vehicle.Year} {vehicle.Make} {vehicle.Model} Auction: {finalPayment}";

            return await TrackEventAsync(
                AnalyticsEventType.PaymentCompleted,
                userId,
                auctionId,
                new Dictionary<string, object> { { "Payment", message } },
                "Web"
            );
        }


        /// <summary>
        /// Checks if payment has been completed for an auction.
        /// </summary>
        public async Task<Result<bool>> IsAuctionPaymentCompletedAsync(int auctionId)
        {
            try
            {
                var completed = await _uow.Analytics.IsPaymentCompletedAsync(auctionId);
                return Result<bool>.Success(completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check payment status for auction {AuctionId}", auctionId);
                return Result<bool>.Failure("Failed to check auction payment status.");
            }
        }
    }
}
