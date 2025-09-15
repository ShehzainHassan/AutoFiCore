using AutoFiCore.Dto;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IDashboardService
    {
        Task<Result<ExecutiveDashboard>> GetExecutiveDashboardAsync(DateTime startDate, DateTime endDate);
        Task<Result<FileResultDTO>> ExportDashboardReportAsync(DateTime startDate, DateTime endDate, string format = "csv");
        Task<Result<AuctionPerformanceReport>> GetAuctionDashboardAsync(DateTime start, DateTime end);
        Task<Result<UserActivityReport>> GetUserDashboardAsync(DateTime start, DateTime end);
    }
}
