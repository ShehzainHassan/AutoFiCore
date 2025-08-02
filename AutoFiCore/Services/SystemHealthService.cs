using AutoFiCore.Data;

public interface ISystemHealthService
{
    Task<SystemHealthDashboard> GetSystemHealthDashboardAsync();
    Task<PerformanceReport> GetPerformanceReportAsync(DateTime startDate, DateTime endDate);
    Task<ErrorReport> GetErrorReportAsync(DateTime startDate, DateTime endDate);
    Task<List<SlowQueryEntry>> IdentifySlowQueriesAsync(DateTime startDate, DateTime endDate);
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

    public async Task<SystemHealthDashboard> GetSystemHealthDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var start = now.AddHours(-1);

        var avgApiTime = await _repo.GetAverageApiResponseTimeAsync(start, now);
        var totalErrors = await _repo.GetErrorCountAsync(start, now);
        var activeUsers = await _reportRepo.GetActiveUserCountAsync(start, now);

        return new SystemHealthDashboard
        {
            AverageApiResponseTime = avgApiTime,
            TotalErrors = totalErrors,
            ActiveUserCount = activeUsers
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
}
