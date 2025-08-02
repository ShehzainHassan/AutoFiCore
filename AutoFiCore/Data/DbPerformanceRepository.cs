using AutoFiCore.Data;
using Microsoft.EntityFrameworkCore;

public interface IPerformanceRepository
{
    Task AddApiLogAsync(APIPerformanceLog log);
    Task AddQueryLogAsync(DBQueryLog log);
    Task AddErrorLogAsync(ErrorLog log);
    Task<List<APIPerformanceLog>> GetApiLogsInRangeAsync(DateTime start, DateTime end);
    Task<int> GetSlowQueryCountInRangeAsync(DateTime start, DateTime end, TimeSpan threshold);
    Task<double> GetAverageApiResponseTimeAsync(DateTime start, DateTime end);
    Task<int> GetErrorCountAsync(DateTime start, DateTime end);
    Task<List<APIResponseStat>> GetApiPerformanceStatsAsync(DateTime start, DateTime end);
    Task<List<ErrorStat>> GetCommonErrorStatsAsync(DateTime start, DateTime end);
    Task<List<SlowQueryEntry>> GetSlowQueriesAsync(DateTime start, DateTime end, TimeSpan threshold);
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
            .GroupBy(e => e.ErrorType)
            .Select(g => new ErrorStat
            {
                ErrorType = g.Key,
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
    public Task<int> GetErrorCountAsync(DateTime start, DateTime end)
    {
        return _db.ErrorLogs
            .Where(e => e.Timestamp >= start && e.Timestamp < end)
            .CountAsync();
    }

}
