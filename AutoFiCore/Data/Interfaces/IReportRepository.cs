using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IReportRepository
    {
        Task<int> GetTotalAuctionsAsync(DateTime start, DateTime end);
        Task<int> GetSuccessfulAuctionsAsync(DateTime start, DateTime end);
        Task<decimal> GetAverageAuctionPriceAsync(DateTime start, DateTime end);
        Task<int> GetActiveUserCountAsync(DateTime start, DateTime end);
        Task<int> GetNewUserCountAsync(DateTime start, DateTime end);
        Task<decimal> GetUserEngagementScoreAsync(DateTime start, DateTime end);
        Task<decimal> GetSuccessfulPaymentPercentageAsync(DateTime start, DateTime end);
        Task<decimal> GetCommissionEarnedAsync(DateTime start, DateTime end);
        Task<List<CategoryPerformance>> GetPopularCategoriesReportAsync(DateTime start, DateTime end);
        Task<List<string>> GetTopAuctionItemsAsync(DateTime start, DateTime end);
        Task<double> GetUserRetentionRateAsync(DateTime start, DateTime end);
        Task<decimal> GetAverageAuctionViewsAsync(DateTime start, DateTime end);
        Task<decimal> GetAverageAuctionBidsAsync(DateTime start, DateTime end);
        Task<int> GetAuctionViewCountAsync(int auctionId, DateTime start, DateTime end);
        Task<int> GetUniqueBiddersCountAsync(int auctionId, DateTime start, DateTime end);
        Task<int> GetTotalBidsCountAsync(int auctionId, DateTime start, DateTime end);
        Task<List<Auction>> GetAuctionsInDateRangeAsync(DateTime start, DateTime end);
        Task<List<AuctionAnalyticsTableDTO>> GetAuctionAnalyticsTableAsync(DateTime start, DateTime end, string? category);
        Task<List<UserAnalyticsTableDTO>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueTableAnalyticsDTO>> GetRevenueTableAnalyticsAsync(DateTime start, DateTime end);
        Task<Dictionary<string, decimal>> GetSummaryDataAsync(DateTime startDate, DateTime endDate, string dataType);
        Task AddRecentDownload(RecentDownloads recentDownloads);
        Task<PagedResult<RecentDownloads>> GetRecentDownloadsPagedAsync(int page = 1, int pageSize = 10);
    }
}
