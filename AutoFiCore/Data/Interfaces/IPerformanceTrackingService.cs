using AutoFiCore.Dto;

namespace AutoFiCore.Data.Interfaces
{
    public interface IPerformanceTrackingService
    {
        Task TrackAPIRequestAsync(string endpoint, TimeSpan responseTime, int statusCode);
        Task TrackDatabaseQueryAsync(string queryType, TimeSpan duration);
        Task TrackErrorEventAsync(int errorCode, string message);
        Task<HourlyPerformanceMetrics> CalculateHourlyPerformanceMetricsAsync(DateTime hour);
    }
}
