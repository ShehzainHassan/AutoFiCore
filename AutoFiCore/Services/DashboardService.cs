using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using System.Text;

public interface IDashboardService
{
    Task<ExecutiveDashboard> GetExecutiveDashboardAsync(DateTime startDate, DateTime endDate);
    Task<FileResultDTO> ExportDashboardReportAsync(DateTime startDate, DateTime endDate, string format = "csv");
    Task<AuctionPerformanceReport> GetAuctionDashboardAsync(DateTime start, DateTime end);
    Task<UserActivityReport> GetUserDashboardAsync(DateTime start, DateTime end);
}

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;
    public DashboardService(IUnitOfWork unitOfWork, IPdfService pdfService)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
    }
    public async Task<ExecutiveDashboard> GetExecutiveDashboardAsync(DateTime start, DateTime end)
    {  
        var revenueTotal = await _unitOfWork.Metrics.GetRevenueTotalAsync(start, end);
        var userCount = await _unitOfWork.Metrics.GetUserCountAsync(start, end);
        var activeAuctionCount = await _unitOfWork.Metrics.GetAuctionCountAsync(start, end);

        return new ExecutiveDashboard
        {
            TotalRevenue = revenueTotal,
            NewUsers = userCount,
            ActiveAuctions = activeAuctionCount
        };
    }
    public async Task<FileResultDTO> ExportDashboardReportAsync(DateTime startDate, DateTime endDate, string format = "csv")
    {
        var dashboardReport = await GetExecutiveDashboardAsync(startDate, endDate);

        string fileName;
        byte[] content;
        string contentType;

        fileName = $"dashboard_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

        if (format.Equals("PDF", StringComparison.OrdinalIgnoreCase))
        {
            content = _pdfService.GenerateDashboardSummaryPdf(startDate, endDate, dashboardReport);
            contentType = "application/pdf";
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("StartDate,EndDate,TotalRevenue,NewUsers,ActiveAuctions");
            sb.AppendLine($"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd},{dashboardReport.TotalRevenue},{dashboardReport.NewUsers},{dashboardReport.ActiveAuctions}");
            content = Encoding.UTF8.GetBytes(sb.ToString());
            contentType = "text/csv";
        }

        await _unitOfWork.Report.AddRecentDownload(new RecentDownloads
        {
            ReportType = ReportType.DashboardSummary,
            DateRange = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            Format = format
        });
        await _unitOfWork.SaveChangesAsync();

        return new FileResultDTO
        {
            Content = content,
            ContentType = contentType,
            FileName = fileName
        };
    }
    public async Task<AuctionPerformanceReport> GetAuctionDashboardAsync(DateTime start, DateTime end)
    {
        var total = await _unitOfWork.Report.GetTotalAuctionsAsync(start, end);
        var endedAuctions = await _unitOfWork.Auctions.GetEndedAuctions();
        var successful = await _unitOfWork.Report.GetSuccessfulAuctionsAsync(start, end);
        var avgViews = await _unitOfWork.Report.GetAverageAuctionViewsAsync(start, end);
        var avgBids = await _unitOfWork.Report.GetAverageAuctionBidsAsync(start, end);
        var avgPrice = await _unitOfWork.Report.GetAverageAuctionPriceAsync(start, end);
        var topItems = await _unitOfWork.Report.GetTopAuctionItemsAsync(start, end);

        return new AuctionPerformanceReport
        {
            TotalAuctions = total,
            SuccessRate = total == 0 ? 0 : (double)successful / endedAuctions.Count * 100,
            AverageViews = avgViews,
            AverageBids = avgBids,
            AverageFinalPrice = avgPrice,
        };
    }
    public async Task<UserActivityReport> GetUserDashboardAsync(DateTime start, DateTime end)
    {
        return new UserActivityReport
        {
            TotalUsers = await _unitOfWork.Users.GetAllUsersCountAsync(),
            ActiveUsers = await _unitOfWork.Report.GetActiveUserCountAsync(start, end),
            NewRegistrations = await _unitOfWork.Report.GetNewUserCountAsync(start, end),
            RetentionRate = await _unitOfWork.Report.GetUserRetentionRateAsync(start, end),
            EngagementScore = await _unitOfWork.Report.GetUserEngagementScoreAsync(start, end)
        };
    }
}
