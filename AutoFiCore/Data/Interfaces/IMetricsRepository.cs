using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface IMetricsRepository
    {
        Task<decimal> GetRevenueTotalAsync(DateTime start, DateTime end);
        Task SaveDailyMetricsAsync(IEnumerable<DailyMetric> metrics);
        Task<int> GetAuctionViewsAsync(int auctionId);
        Task<AuctionAnalytics?> GetAuctionAnalyticsAsync(int auctionId);
        Task SaveAuctionAnalyticsAsync(AuctionAnalytics analytics);
        Task<decimal> CalculateUserEngagementAsync(int userId, DateTime start, DateTime end);
        Task<int> GetUserCountAsync(DateTime start, DateTime end);
        Task<int> GetAuctionCountAsync(DateTime start, DateTime end);
    }
}
