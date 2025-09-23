using AutoFiCore.Data;
using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Utilities;
public class SystemHealthService : ISystemHealthService
{
    private readonly IPerformanceRepository _repo;
    private readonly IReportRepository _reportRepo;

    public SystemHealthService(IPerformanceRepository repo, IReportRepository reportRepo)
    {
        _repo = repo;
        _reportRepo = reportRepo;
    }

    public async Task<Result<SystemHealthDashboard>> GetSystemHealthDashboardAsync(DateTime start, DateTime end)
    {
        try
        {
            var avgApiTime = await _repo.GetAverageApiResponseTimeAsync(start, end);
            var errorRate = await _repo.GetErrorRatePercentageAsync(start, end);
            var activeUsers = await _reportRepo.GetActiveUserCountAsync(start, end);
            var systemUptime = await _repo.GetSystemUptimePercentageAsync(start, end);

            var dashboard = new SystemHealthDashboard
            {
                AverageApiResponseTime = avgApiTime,
                ErrorRate = errorRate,
                ActiveSessions = activeUsers,
                SystemUptime = systemUptime,
            };

            return Result<SystemHealthDashboard>.Success(dashboard);
        }
        catch (Exception ex)
        {
            return Result<SystemHealthDashboard>.Failure($"Failed to get system health dashboard: {ex.Message}");
        }
    }

    public async Task<Result<PerformanceReport>> GetPerformanceReportAsync(DateTime start, DateTime end)
    {
        try
        {
            var apiStats = await _repo.GetApiPerformanceStatsAsync(start, end);
            var slowQueryCount = await _repo.GetSlowQueryCountInRangeAsync(start, end, TimeSpan.FromMilliseconds(500));

            var report = new PerformanceReport
            {
                ApiStats = apiStats,
                TotalSlowQueries = slowQueryCount
            };

            return Result<PerformanceReport>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<PerformanceReport>.Failure($"Failed to get performance report: {ex.Message}");
        }
    }

    public async Task<Result<ErrorReport>> GetErrorReportAsync(DateTime start, DateTime end)
    {
        try
        {
            var errors = await _repo.GetCommonErrorStatsAsync(start, end);
            var report = new ErrorReport { CommonErrors = errors };
            return Result<ErrorReport>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<ErrorReport>.Failure($"Failed to get error report: {ex.Message}");
        }
    }

    public async Task<Result<List<SlowQueryEntry>>> IdentifySlowQueriesAsync(DateTime start, DateTime end)
    {
        try
        {
            var queries = await _repo.GetSlowQueriesAsync(start, end, TimeSpan.FromMilliseconds(500));
            return Result<List<SlowQueryEntry>>.Success(queries);
        }
        catch (Exception ex)
        {
            return Result<List<SlowQueryEntry>>.Failure($"Failed to identify slow queries: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<ErrorLog>>> GetErrorLogsPagedAsync(int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var logs = await _repo.GetErrorLogsPagedAsync(page, pageSize, startDate, endDate);
            return Result<PagedResult<ErrorLog>>.Success(logs);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ErrorLog>>.Failure($"Failed to get error logs: {ex.Message}");
        }
    }


    public async Task<Result<List<ResponseTimePoint>>> GetResponseTimePointsAsync(DateTime start, DateTime end)
    {
        try
        {
            var points = await _repo.GetResponseTimePointsAsync(start, end);
            return Result<List<ResponseTimePoint>>.Success(points);
        }
        catch (Exception ex)
        {
            return Result<List<ResponseTimePoint>>.Failure($"Failed to get response time points: {ex.Message}");
        }
    }

    public async Task<Result<DateTime?>> GetOldestApiLogTimestampAsync()
    {
        try
        {
            var timestamp = await _repo.GetOldestApiLogTimestampAsync();
            return Result<DateTime?>.Success(timestamp);
        }
        catch (Exception ex)
        {
            return Result<DateTime?>.Failure($"Failed to get oldest API log timestamp: {ex.Message}");
        }
    }
}
