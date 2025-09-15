using AutoFiCore.Dto;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface ISystemHealthService
    {
        Task<Result<SystemHealthDashboard>> GetSystemHealthDashboardAsync(DateTime start, DateTime end);
        Task<Result<PerformanceReport>> GetPerformanceReportAsync(DateTime startDate, DateTime endDate);
        Task<Result<ErrorReport>> GetErrorReportAsync(DateTime startDate, DateTime endDate);
        Task<Result<List<SlowQueryEntry>>> IdentifySlowQueriesAsync(DateTime startDate, DateTime endDate);
        Task<Result<PagedResult<ErrorLog>>> GetErrorLogsPagedAsync(int page = 1, int pageSize = 10);
        Task<Result<List<ResponseTimePoint>>> GetResponseTimePointsAsync(DateTime start, DateTime end);
        Task<Result<DateTime?>> GetOldestApiLogTimestampAsync();
    }
}
