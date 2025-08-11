using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;

public interface IPerformanceRepository
{
    Task AddApiLogAsync(APIPerformanceLog log);
    Task AddQueryLogAsync(DBQueryLog log);
    Task AddErrorLogAsync(ErrorLog log);
    Task<List<APIPerformanceLog>> GetApiLogsInRangeAsync(DateTime start, DateTime end);
    Task<int> GetSlowQueryCountInRangeAsync(DateTime start, DateTime end, TimeSpan threshold);
    Task<double> GetAverageApiResponseTimeAsync(DateTime start, DateTime end);
    Task<double> GetErrorRatePercentageAsync(DateTime start, DateTime end);
    Task<List<APIResponseStat>> GetApiPerformanceStatsAsync(DateTime start, DateTime end);
    Task<List<ErrorStat>> GetCommonErrorStatsAsync(DateTime start, DateTime end);
    Task<List<SlowQueryEntry>> GetSlowQueriesAsync(DateTime start, DateTime end, TimeSpan threshold);
    Task<double> GetSystemUptimePercentageAsync(DateTime start, DateTime end);
    Task<PagedResult<ErrorLog>> GetErrorLogsPagedAsync(int page = 1, int pageSize = 10);
    Task<List<ResponseTimePoint>> GetResponseTimePointsAsync(DateTime start, DateTime end);
    Task<DateTime?> GetOldestApiLogTimestampAsync();
}

public class DbPerformanceRepository : IPerformanceRepository
{
    private readonly ApplicationDbContext _db;
    public DbPerformanceRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    public Task AddApiLogAsync(APIPerformanceLog log)
    {
        _db.ApiPerformanceLogs.Add(log);
        return _db.SaveChangesAsync();
    }
    public Task AddQueryLogAsync(DBQueryLog log)
    {
        _db.DbQueryLogs.Add(log);
        return _db.SaveChangesAsync();
    }
    public Task AddErrorLogAsync(ErrorLog log)
    {
        _db.ErrorLogs.Add(log);
        return _db.SaveChangesAsync();
    }
    public Task<List<APIPerformanceLog>> GetApiLogsInRangeAsync(DateTime start, DateTime end)
    {
        return _db.ApiPerformanceLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end)
            .ToListAsync();
    }
    public Task<int> GetSlowQueryCountInRangeAsync(DateTime start, DateTime end, TimeSpan threshold)
    {
        return _db.DbQueryLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end && log.Duration > threshold)
            .CountAsync();
    }
    public async Task<double> GetAverageApiResponseTimeAsync(DateTime start, DateTime end)
    {
        var logs = await _db.ApiPerformanceLogs
            .Where(x => x.Timestamp >= start && x.Timestamp < end)
            .ToListAsync();

        return logs.Any() ? logs.Average(l => l.ResponseTime.TotalMilliseconds) : 0;
    }
    public async Task<List<APIResponseStat>> GetApiPerformanceStatsAsync(DateTime start, DateTime end)
    {
        return await _db.ApiPerformanceLogs
            .Where(x => x.Timestamp >= start && x.Timestamp < end)
            .GroupBy(x => x.Endpoint)
            .Select(g => new APIResponseStat
            {
                Endpoint = g.Key,
                AverageResponseTimeMs = g.Average(x => x.ResponseTime.TotalMilliseconds),
                RequestCount = g.Count()
            })
            .ToListAsync();
    }
    public async Task<List<ErrorStat>> GetCommonErrorStatsAsync(DateTime start, DateTime end)
    {
        return await _db.ErrorLogs
            .Where(e => e.Timestamp >= start && e.Timestamp < end)
            .GroupBy(e => e.ErrorCode)
            .Select(g => new ErrorStat
            {
                ErrorCode = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(e => e.Count)
            .ToListAsync();
    }
    public Task<List<SlowQueryEntry>> GetSlowQueriesAsync(DateTime start, DateTime end, TimeSpan threshold)
    {
        return _db.DbQueryLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end && log.Duration > threshold)
            .Select(log => new SlowQueryEntry
            {
                QueryType = log.QueryType,
                Duration = log.Duration,
                Timestamp = log.Timestamp
            })
            .ToListAsync();
    }
    public async Task<double> GetErrorRatePercentageAsync(DateTime start, DateTime end)
    {
        var totalRequests = await _db.ApiPerformanceLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end)
            .CountAsync();

        if (totalRequests == 0)
            return 0;

        var errorRequests = await _db.ApiPerformanceLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end && log.StatusCode >= 500)
            .CountAsync();

        return (errorRequests / (double)totalRequests) * 100;
    }
    public async Task<double> GetSystemUptimePercentageAsync(DateTime start, DateTime end)
    {
        var totalRequests = await _db.ApiPerformanceLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end)
            .CountAsync();

        if (totalRequests == 0)
            return 100; 

        var successfulRequests = await _db.ApiPerformanceLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end && log.StatusCode < 500)
            .CountAsync();

        return (successfulRequests / (double)totalRequests) * 100;
    }
    public async Task<PagedResult<ErrorLog>> GetErrorLogsPagedAsync(int page = 1, int pageSize = 10)
    {
        var query = _db.ErrorLogs
            .OrderByDescending(e => e.Timestamp);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ErrorLog>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }
    public async Task<List<ResponseTimePoint>> GetResponseTimePointsAsync(DateTime start, DateTime end)
    {
        const int bucketCount = 10;
        var totalRange = end - start;
        var bucketSize = TimeSpan.FromTicks(totalRange.Ticks / bucketCount);

        var logs = await _db.ApiPerformanceLogs
            .Where(log => log.Timestamp >= start && log.Timestamp <= end)
            .Select(log => new
            {
                log.Timestamp,
                ResponseTimeMs = log.ResponseTime.TotalMilliseconds
            })
            .ToListAsync();

        var grouped = logs
            .GroupBy(x => (int)((x.Timestamp - start).Ticks / bucketSize.Ticks))
            .Select(g => new
            {
                BucketIndex = g.Key,
                AvgResponseTimeMs = g.Average(x => x.ResponseTimeMs)
            })
            .ToList();

        var result = Enumerable.Range(0, bucketCount)
            .Select(i =>
            {
                var bucketStart = start.AddTicks(bucketSize.Ticks * i);
                var match = grouped.FirstOrDefault(g => g.BucketIndex == i);

                return new ResponseTimePoint
                {
                    TimeLabel = bucketStart,
                    AvgResponseTimeMs = match?.AvgResponseTimeMs ?? 0
                };
            })
            .ToList();

        return result;
    }
    public async Task<DateTime?> GetOldestApiLogTimestampAsync()
    {
        return await _db.ApiPerformanceLogs
            .OrderBy(log => log.Timestamp)
            .Select(log => (DateTime?)log.Timestamp)
            .FirstOrDefaultAsync();
    }
}
