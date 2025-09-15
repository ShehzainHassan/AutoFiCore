using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IMetricsCalculationService
    {
        Task<Result<bool>> CalculateDailyMetricsAsync(DateTime date);
        Task<Result<bool>> UpdateAuctionAnalyticsAsync(int auctionId);
        Task<Result<decimal>> CalculateUserEngagementAsync(int userId, DateTime startDate, DateTime endDate);
        Task<Result<decimal>> GenerateRevenueMetricsAsync(DateTime startDate, DateTime endDate);
    }
}
