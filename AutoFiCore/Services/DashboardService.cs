using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.Extensions.Logging;
using System.Text;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork unitOfWork, IPdfService pdfService, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task<Result<ExecutiveDashboard>> GetExecutiveDashboardAsync(DateTime start, DateTime end)
    {
        try
        {
            var revenueTotal = await _unitOfWork.Metrics.GetRevenueTotalAsync(start, end);
            var userCount = await _unitOfWork.Metrics.GetUserCountAsync(start, end);
            var activeAuctionCount = await _unitOfWork.Metrics.GetAuctionCountAsync(start, end);

            var dashboard = new ExecutiveDashboard
            {
                TotalRevenue = revenueTotal,
                NewUsers = userCount,
                ActiveAuctions = activeAuctionCount
            };

            return Result<ExecutiveDashboard>.Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching executive dashboard metrics.");
            return Result<ExecutiveDashboard>.Failure("Failed to fetch executive dashboard metrics.");
        }
    }

    public async Task<Result<FileResultDTO>> ExportDashboardReportAsync(DateTime startDate, DateTime endDate, string format = "csv")
    {
        try
        {
            var dashboardResult = await GetExecutiveDashboardAsync(startDate, endDate);
            if (!dashboardResult.IsSuccess)
                return Result<FileResultDTO>.Failure(dashboardResult.Error!);

            var dashboardReport = dashboardResult.Value!;
            string fileName = $"dashboard_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
            byte[] content;
            string contentType;

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

            var fileResult = new FileResultDTO
            {
                Content = content,
                ContentType = contentType,
                FileName = fileName
            };

            return Result<FileResultDTO>.Success(fileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while exporting dashboard report.");
            return Result<FileResultDTO>.Failure("Failed to export dashboard report.");
        }
    }

    public async Task<Result<AuctionPerformanceReport>> GetAuctionDashboardAsync(DateTime start, DateTime end)
    {
        try
        {
            var total = await _unitOfWork.Report.GetTotalAuctionsAsync(start, end);
            var endedAuctions = await _unitOfWork.Auctions.GetEndedAuctions();
            var successful = await _unitOfWork.Report.GetSuccessfulAuctionsAsync(start, end);
            var avgViews = await _unitOfWork.Report.GetAverageAuctionViewsAsync(start, end);
            var avgBids = await _unitOfWork.Report.GetAverageAuctionBidsAsync(start, end);
            var avgPrice = await _unitOfWork.Report.GetAverageAuctionPriceAsync(start, end);

            var report = new AuctionPerformanceReport
            {
                TotalAuctions = total,
                SuccessRate = endedAuctions.Count == 0
                    ? 0
                    : (double)successful / endedAuctions.Count * 100,
                AverageViews = avgViews,
                AverageBids = avgBids,
                AverageFinalPrice = avgPrice,
            };

            return Result<AuctionPerformanceReport>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching auction dashboard data.");
            return Result<AuctionPerformanceReport>.Failure("Failed to fetch auction dashboard.");
        }
    }


    public async Task<Result<UserActivityReport>> GetUserDashboardAsync(DateTime start, DateTime end)
    {
        try
        {
            var report = new UserActivityReport
            {
                TotalUsers = await _unitOfWork.Users.GetAllUsersCountAsync(),
                ActiveUsers = await _unitOfWork.Report.GetActiveUserCountAsync(start, end),
                NewRegistrations = await _unitOfWork.Report.GetNewUserCountAsync(start, end),
                RetentionRate = await _unitOfWork.Report.GetUserRetentionRateAsync(start, end),
                EngagementScore = await _unitOfWork.Report.GetUserEngagementScoreAsync(start, end)
            };

            return Result<UserActivityReport>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching user dashboard data.");
            return Result<UserActivityReport>.Failure("Failed to fetch user dashboard.");
        }
    }
}
