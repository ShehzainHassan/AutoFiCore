using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;

public interface IPerformanceTrackingService
{
    Task TrackAPIRequestAsync(string endpoint, TimeSpan responseTime, int statusCode);
    Task TrackDatabaseQueryAsync(string queryType, TimeSpan duration);
    Task TrackErrorEventAsync(int errorCode, string message);
    Task<HourlyPerformanceMetrics> CalculateHourlyPerformanceMetricsAsync(DateTime hour);
}

public class PerformanceTrackingService : IPerformanceTrackingService
{
    private readonly IUnitOfWork _uow;

    public PerformanceTrackingService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public Task TrackAPIRequestAsync(string endpoint, TimeSpan responseTime, int statusCode)
    {
        var log = new APIPerformanceLog
        {
            Endpoint = endpoint,
            ResponseTime = responseTime,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
        return _uow.Performance.AddApiLogAsync(log);
    }

    public Task TrackDatabaseQueryAsync(string queryType, TimeSpan duration)
    {
        var log = new DBQueryLog
        {
            QueryType = queryType,
            Duration = duration,
            Timestamp = DateTime.UtcNow
        };
        return _uow.Performance.AddQueryLogAsync(log);
    }

    public Task TrackErrorEventAsync(int errorCode, string message)
    {
        var error = new ErrorLog
        {
            ErrorCode = errorCode,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
        return _uow.Performance.AddErrorLogAsync(error);
    }

    public async Task<HourlyPerformanceMetrics> CalculateHourlyPerformanceMetricsAsync(DateTime hour)
    {
        var start = hour;
        var end = hour.AddHours(1);

        var apiLogs = await _uow.Performance.GetApiLogsInRangeAsync(start, end);
        var slowQueryCount = await _uow.Performance.GetSlowQueryCountInRangeAsync(start, end, TimeSpan.FromMilliseconds(500));

        return new HourlyPerformanceMetrics
        {
            Hour = hour,
            AvgApiResponseTime = apiLogs.Any() ? apiLogs.Average(x => x.ResponseTime.TotalMilliseconds) : 0,
            TotalApiCalls = apiLogs.Count,
            SlowQueryCount = slowQueryCount
        };
    }
}
