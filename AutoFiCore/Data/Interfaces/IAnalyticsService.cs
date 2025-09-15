using AutoFiCore.Enums;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAnalyticsService
    {
        Task<Result<bool>> TrackEventAsync(AnalyticsEventType type, int? userId, int? auctionId, Dictionary<string, object> data, string source);
        Task<Result<bool>> TrackAuctionViewAsync(int auctionId, int? userId, string source);
        Task<Result<bool>> TrackBidEventAsync(int auctionId, int userId, decimal bidAmount);
        Task<Result<bool>> TrackAuctionCompletionAsync(int auctionId, bool isSuccessful, decimal finalPrice);
        Task<Result<bool>> TrackPaymentCompletedAsync(int auctionId, int? userId, decimal finalPayment);
        Task<Result<bool>> IsAuctionPaymentCompletedAsync(int auctionId);
    }
}
