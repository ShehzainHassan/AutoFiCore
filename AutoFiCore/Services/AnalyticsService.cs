using AutoFiCore.Data;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoFiCore.Services
{
    public interface IAnalyticsService
    {
        Task TrackEventAsync(AnalyticsEventType type, int? userId, int? auctionId, Dictionary<string, object> data, string source);
        Task TrackAuctionViewAsync(int auctionId, int? userId, string source);
        Task TrackBidEventAsync(int auctionId, int userId, decimal bidAmount);
        Task TrackAuctionCompletionAsync(int auctionId, bool isSuccessful, decimal finalPrice);
    }
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(IUnitOfWork uow, ILogger<AnalyticsService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task TrackEventAsync(AnalyticsEventType type, int? userId, int? auctionId, Dictionary<string, object> data, string source)
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
        }
        public Task TrackAuctionViewAsync(int auctionId, int? userId, string source)
        {
            return TrackEventAsync(AnalyticsEventType.AuctionView, userId, auctionId, new(), source);
        }
        public Task TrackBidEventAsync(int auctionId, int userId, decimal bidAmount)
        {
            return TrackEventAsync(AnalyticsEventType.BidPlaced, userId, auctionId,
                new Dictionary<string, object> { { "BidAmount", bidAmount } }, "Web");
        }
        public Task TrackAuctionCompletionAsync(int auctionId, bool isSuccessful, decimal finalPrice)
        {
            return TrackEventAsync(AnalyticsEventType.AuctionCompleted, null, auctionId,
                new Dictionary<string, object>
                {
                    { "Success", isSuccessful },
                    { "FinalPrice", finalPrice }
                }, "Web");
        }
    }
}