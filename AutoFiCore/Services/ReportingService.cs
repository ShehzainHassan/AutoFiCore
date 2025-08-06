using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using System.Text;

public interface IReportingService
{
    Task<AuctionPerformanceReport> GetAuctionPerformanceReportAsync(DateTime startDate, DateTime endDate);
    Task<UserActivityReport> GetUserActivityReportAsync(DateTime startDate, DateTime endDate);
    Task<RevenueReport> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
    Task<List<CategoryPerformance>> GetPopularCategoriesReportAsync(DateTime startDate, DateTime endDate);
    Task<FileResultDTO> ExportReportAsync(string reportType, DateTime startDate, DateTime endDate, string format = "csv");
    Task<List<AuctionAnalyticsTableDTO>> GetAuctionAnalyticsAsync(DateTime start, DateTime end);
    Task<List<UserAnalyticsTableDTO>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate);
}

public class ReportingService : IReportingService
{
    private readonly IUnitOfWork _uow;
    private readonly IPdfService _pdfService;

    public ReportingService(IUnitOfWork uow, IPdfService pdfService)
    {
        _uow = uow;
        _pdfService = pdfService;
    }
    public async Task<AuctionPerformanceReport> GetAuctionPerformanceReportAsync(DateTime start, DateTime end)
    {
        var total = await _uow.Report.GetTotalAuctionsAsync(start, end);
        var endedAuctions = await _uow.Auctions.GetEndedAuctions();
        var successful = await _uow.Report.GetSuccessfulAuctionsAsync(start, end);
        var avgViews = await _uow.Report.GetAverageAuctionViewsAsync(start, end);
        var avgBids = await _uow.Report.GetAverageAuctionBidsAsync(start, end);
        var avgPrice = await _uow.Report.GetAverageAuctionPriceAsync(start, end);

        return new AuctionPerformanceReport
        {
            TotalAuctions = total,
            SuccessRate = total == 0 ? 0 : (double)successful / endedAuctions.Count * 100,
            AverageViews = avgViews,
            AverageBids = avgBids,
            AverageFinalPrice = avgPrice
        };
    }
    public async Task<UserActivityReport> GetUserActivityReportAsync(DateTime start, DateTime end)
    {
        return new UserActivityReport
        {
            TotalUsers = await _uow.Users.GetAllUsersCountAsync(),
            ActiveUsers = await _uow.Report.GetActiveUserCountAsync(start, end),
            NewRegistrations = await _uow.Report.GetNewUserCountAsync(start, end),
            RetentionRate = await _uow.Report.GetUserRetentionRateAsync(start, end),
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
        string fileName;
        byte[] content;
        string contentType;

        switch (reportType.ToLower())
        {
            case "auction":
                var auctionReport = await GetAuctionPerformanceReportAsync(startDate, endDate);
                fileName = $"auction_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                if (format == "pdf")
                {
                    content = _pdfService.GenerateAuctionPerformancePdf(startDate, endDate, auctionReport);
                    contentType = "application/pdf";
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("StartDate,EndDate,TotalAuctions,SuccessRate,AverageFinalPrice");
                    sb.AppendLine($"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd},{auctionReport.TotalAuctions},{auctionReport.SuccessRate},{auctionReport.AverageFinalPrice}");
                    content = Encoding.UTF8.GetBytes(sb.ToString());
                    contentType = "text/csv";
                }
                break;

            case "user":
                var userReport = await GetUserActivityReportAsync(startDate, endDate);
                fileName = $"user_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                if (format == "pdf")
                {
                    content = _pdfService.GenerateUserActivityPdf(startDate, endDate, userReport);
                    contentType = "application/pdf";
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("StartDate,EndDate,NewRegistrations,EngagementScore");
                    sb.AppendLine($"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd},{userReport.NewRegistrations},{userReport.EngagementScore}");
                    content = Encoding.UTF8.GetBytes(sb.ToString());
                    contentType = "text/csv";
                }
                break;

            case "revenue":
                var revenue = await _uow.Report.GetCommissionEarnedAsync(startDate, endDate);
                fileName = $"revenue_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                if (format == "pdf")
                {
                    content = _pdfService.GenerateRevenueReportPdf(startDate, endDate, revenue);
                    contentType = "application/pdf";
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("StartDate,EndDate,CommissionEarned");
                    sb.AppendLine($"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd},{revenue}");
                    content = Encoding.UTF8.GetBytes(sb.ToString());
                    contentType = "text/csv";
                }
                break;

            default:
                throw new ArgumentException("Invalid report type");
        }

        return new FileResultDTO
        {
            Content = content,
            ContentType = contentType,
            FileName = fileName
        };
    }
    public async Task<List<AuctionAnalyticsTableDTO>> GetAuctionAnalyticsAsync(DateTime start, DateTime end)
    {
        return await _uow.Report.GetAuctionAnalyticsTableAsync(start, end);
    }
    public async Task<List<UserAnalyticsTableDTO>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        return await _uow.Report.GetUserAnalyticsAsync(startDate, endDate);
    }
}
