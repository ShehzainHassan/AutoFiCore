using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Utilities;

public interface ISystemHealthService
{
    Task<SystemHealthDashboard> GetSystemHealthDashboardAsync(DateTime start, DateTime end);
    Task<PerformanceReport> GetPerformanceReportAsync(DateTime startDate, DateTime endDate);
    Task<ErrorReport> GetErrorReportAsync(DateTime startDate, DateTime endDate);
    Task<List<SlowQueryEntry>> IdentifySlowQueriesAsync(DateTime startDate, DateTime endDate);
    Task<PagedResult<ErrorLog>> GetErrorLogsPagedAsync(int page = 1, int pageSize = 10);
    Task<List<ResponseTimePoint>> GetResponseTimePointsAsync(DateTime start, DateTime end);
    Task<DateTime?> GetOldestApiLogTimestampAsync();
}

public class SystemHealthService : ISystemHealthService
{
    private readonly IPerformanceRepository _repo;
    private readonly IReportRepository _reportRepo;

    public SystemHealthService(IPerformanceRepository repo, IReportRepository reportRepo)
    {
        _repo = repo;
        _reportRepo = reportRepo;
    }

    public async Task<SystemHealthDashboard> GetSystemHealthDashboardAsync(DateTime start, DateTime end)
    {
        var avgApiTime = await _repo.GetAverageApiResponseTimeAsync(start, end);
        var errorRate = await _repo.GetErrorRatePercentageAsync(start, end);
        var activeUsers = await _reportRepo.GetActiveUserCountAsync(start, end);
        var systemUptime = await _repo.GetSystemUptimePercentageAsync(start, end);
        return new SystemHealthDashboard
        {
            AverageApiResponseTime = avgApiTime,
            ErrorRate = errorRate,
            ActiveSessions = activeUsers,
            SystemUptime = systemUptime,
        };
    }

    public async Task<PerformanceReport> GetPerformanceReportAsync(DateTime start, DateTime end)
    {
        var apiStats = await _repo.GetApiPerformanceStatsAsync(start, end);
        var slowQueryCount = await _repo.GetSlowQueryCountInRangeAsync(start, end, TimeSpan.FromMilliseconds(500));

        return new PerformanceReport
        {
            ApiStats = apiStats,
            TotalSlowQueries = slowQueryCount
        };
    }

    public async Task<ErrorReport> GetErrorReportAsync(DateTime start, DateTime end)
    {
        var errors = await _repo.GetCommonErrorStatsAsync(start, end);

        return new ErrorReport
        {
            CommonErrors = errors
        };
    }

    public Task<List<SlowQueryEntry>> IdentifySlowQueriesAsync(DateTime start, DateTime end)
    {
        return _repo.GetSlowQueriesAsync(start, end, TimeSpan.FromMilliseconds(500));
    }
    public async Task<PagedResult<ErrorLog>> GetErrorLogsPagedAsync(int page = 1, int pageSize = 10)
    {
       return await _repo.GetErrorLogsPagedAsync(page, pageSize);
    }
    public async Task<List<ResponseTimePoint>> GetResponseTimePointsAsync(DateTime start, DateTime end)
    {
        return await _repo.GetResponseTimePointsAsync(start, end);
    }
    public async Task<DateTime?> GetOldestApiLogTimestampAsync()
    {
        return await _repo.GetOldestApiLogTimestampAsync();
    }
}
