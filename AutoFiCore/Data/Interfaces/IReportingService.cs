using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IReportingService
    {
        Task<Result<RevenueReport>> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<Result<List<CategoryPerformance>>> GetPopularCategoriesReportAsync(DateTime startDate, DateTime endDate);
        Task<Result<FileResultDTO>> ExportReportAsync(ReportType reportType, DateTime startDate, DateTime endDate, string format = "csv");
        Task<Result<AuctionAnalyticsResponseDTO>> GetAuctionAnalyticsAsync(DateTime start, DateTime end, string? category);
        Task<Result<List<UserAnalyticsTableDTO>>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<Result<List<RevenueTableAnalyticsDTO>>> GetRevenueTableAnalyticsAsync(DateTime start, DateTime end);
        Task<Result<SummaryWithChange<decimal>>> GetSummaryAsync(string dataType, DateTime startDate, DateTime endDate);
        Task<Result<PagedResult<RecentDownloads>>> GetRecentDownloadsAsync(int page, int pageSize);
    }
}
