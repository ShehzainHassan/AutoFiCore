using AutoFiCore.Dto;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
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
}
