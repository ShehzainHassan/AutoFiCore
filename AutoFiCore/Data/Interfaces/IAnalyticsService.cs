using AutoFiCore.Enums;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAnalyticsService
    {
        Task TrackEventAsync(AnalyticsEventType type, int? userId, int? auctionId, Dictionary<string, object> data, string source);
        Task TrackAuctionViewAsync(int auctionId, int? userId, string source);
        Task TrackBidEventAsync(int auctionId, int userId, decimal bidAmount);
        Task TrackAuctionCompletionAsync(int auctionId, bool isSuccessful, decimal finalPrice);
        Task TrackPaymentCompleted(int auctionId, int? userId, decimal finalPayment);
        Task<bool> IsAuctionPaymentCompleted(int auctionId);
    }
}
