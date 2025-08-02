using AutoFiCore.Data;
using AutoFiCore.Dto;
using System.Text;

public interface IReportingService
{
    Task<AuctionPerformanceReport> GetAuctionPerformanceReportAsync(DateTime startDate, DateTime endDate);
    Task<UserActivityReport> GetUserActivityReportAsync(DateTime startDate, DateTime endDate);
    Task<RevenueReport> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
    Task<List<CategoryPerformance>> GetPopularCategoriesReportAsync(DateTime startDate, DateTime endDate);
    Task<FileResultDTO> ExportReportAsync(string reportType, DateTime startDate, DateTime endDate, string format = "csv");
}

public class ReportingService : IReportingService
{
    private readonly IUnitOfWork _uow;
    public ReportingService(IUnitOfWork uow) => _uow = uow;

    public async Task<AuctionPerformanceReport> GetAuctionPerformanceReportAsync(DateTime start, DateTime end)
    {
        var total = await _uow.Report.GetTotalAuctionsAsync(start, end);
        var successful = await _uow.Report.GetSuccessfulAuctionsAsync(start, end);
        var avgPrice = await _uow.Report.GetAverageAuctionPriceAsync(start, end);

        return new AuctionPerformanceReport
        {
            TotalAuctions = total,
            SuccessRate = total == 0 ? 0 : (double)successful / total * 100,
            AverageFinalPrice = avgPrice
        };
    }
    public async Task<UserActivityReport> GetUserActivityReportAsync(DateTime start, DateTime end)
    {
        return new UserActivityReport
        {
            ActiveUsers = await _uow.Report.GetActiveUserCountAsync(start, end),
            NewRegistrations = await _uow.Report.GetNewUserCountAsync(start, end),
            EngagementScore = await _uow.Report.GetUserEngagementScoreAsync(start, end)
        };
    }
    public async Task<RevenueReport> GetRevenueReportAsync(DateTime start, DateTime end)
    {
        return new RevenueReport
        {
            TotalRevenue = await _uow.Metrics.GetRevenueTotalAsync(start, end),
            CommissionEarned = await _uow.Report.GetCommissionEarnedAsync(start, end),
            SuccessfulPayments = await _uow.Report.GetSuccessfulPaymentsCountAsync(start, end)
        };
    }
    public Task<List<CategoryPerformance>> GetPopularCategoriesReportAsync(DateTime start, DateTime end)
    {
        return _uow.Report.GetPopularCategoriesReportAsync(start, end);
    }
    public async Task<FileResultDTO> ExportReportAsync(string reportType, DateTime startDate, DateTime endDate, string format = "csv")
    {
        var sb = new StringBuilder();
        string fileName;

        switch (reportType.ToLower())
        {
            case "auction":
                var auctionReport = await GetAuctionPerformanceReportAsync(startDate, endDate);
                fileName = $"auction_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                sb.AppendLine("StartDate,EndDate,TotalAuctions,SuccessRate,AverageFinalPrice");
                sb.AppendLine($"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd},{auctionReport.TotalAuctions},{auctionReport.SuccessRate},{auctionReport.AverageFinalPrice}");
                break;

            case "user":
                var userReport = await GetUserActivityReportAsync(startDate, endDate);
                fileName = $"user_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                sb.AppendLine("StartDate,EndDate,NewRegistrations,EngagementScore");
                sb.AppendLine($"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd},{userReport.NewRegistrations},{userReport.EngagementScore}");
                break;

            case "revenue":
                var revenue = await _uow.Report.GetCommissionEarnedAsync(startDate, endDate);
                fileName = $"revenue_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                sb.AppendLine("StartDate,EndDate,CommissionEarned");
                sb.AppendLine($"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd},{revenue}");
                break;

            default:
                throw new ArgumentException("Invalid report type");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        return new FileResultDTO
        {
            Content = bytes,
            ContentType = "text/csv",
            FileName = fileName
        };
    }
}
