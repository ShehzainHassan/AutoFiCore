using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using System.Text;

public interface IDashboardService
{
    Task<ExecutiveDashboard> GetExecutiveDashboardAsync(DateTime startDate, DateTime endDate);
    Task<FileResultDTO> ExportDashboardReportAsync(DateTime startDate, DateTime endDate, string format = "csv");
    //Task<AuctionDashboard> GetAuctionDashboardAsync(DateTime startDate, DateTime endDate);
    //Task<UserDashboard> GetUserDashboardAsync(DateTime startDate, DateTime endDate);
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
    //public async Task<AuctionDashboard> GetAuctionDashboardAsync(DateTime start, DateTime end)
    //{
    //    var performance = await _reportService.GetAuctionPerformanceReportAsync(start, end);

    //    var averageBidCount = await _unitOfWork.Report.GetAverageBidCountAsync(start, end);

    //    var topItems = await _unitOfWork.Report.GetTopAuctionItemsAsync(start, end);

    //    return new AuctionDashboard
    //    {
    //        CompletionRate = performance.SuccessRate,
    //        AverageBidCount = Math.Round(averageBidCount, 2),
    //        TopItems = topItems
    //    };
    //}
    //public async Task<UserDashboard> GetUserDashboardAsync(DateTime start, DateTime end)
    //{
    //    var user = await _reportService.GetUserActivityReportAsync(start, end);
    //    var retentionRate = await _unitOfWork.Report.GetUserRetentionRateAsync(start, end);
    //    return new UserDashboard
    //    {
    //        Registrations = user.NewRegistrations,
    //        Engagement = user.EngagementScore,
    //        RetentionRate = retentionRate,
    //    };
    //}
}
