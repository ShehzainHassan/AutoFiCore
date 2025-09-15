using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.Extensions.Logging;
using System.Text;
public class ReportingService : IReportingService
{
    private readonly IUnitOfWork _uow;
    private readonly IPdfService _pdfService;
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<ReportingService> _logger;

    public ReportingService(IUnitOfWork uow, IPdfService pdfService, IDashboardService dashboardService, ILogger<ReportingService> logger)
    {
        _uow = uow;
        _pdfService = pdfService;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<Result<RevenueReport>> GetRevenueReportAsync(DateTime start, DateTime end)
    {
        try
        {
            var report = new RevenueReport
            {
                TotalRevenue = await _uow.Metrics.GetRevenueTotalAsync(start, end),
                CommissionEarned = await _uow.Report.GetCommissionEarnedAsync(start, end),
                AverageSalePrice = await _uow.Report.GetAverageAuctionPriceAsync(start, end),
                SuccessfulPaymentsPercentage = await _uow.Report.GetSuccessfulPaymentPercentageAsync(start, end)
            };

            return Result<RevenueReport>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while generating revenue report.");
            return Result<RevenueReport>.Failure("Failed to generate revenue report.");
        }
    }

    public async Task<Result<List<CategoryPerformance>>> GetPopularCategoriesReportAsync(DateTime start, DateTime end)
    {
        try
        {
            var categories = await _uow.Report.GetPopularCategoriesReportAsync(start, end);
            return Result<List<CategoryPerformance>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching popular categories report.");
            return Result<List<CategoryPerformance>>.Failure("Failed to fetch popular categories report.");
        }
    }

    public async Task<Result<FileResultDTO>> ExportReportAsync(ReportType reportType, DateTime startDate, DateTime endDate, string format = "csv")
    {
        try
        {
            string fileName;
            byte[] content;
            string contentType;

            switch (reportType)
            {
                case ReportType.DashboardSummary:
                    return await _dashboardService.ExportDashboardReportAsync(startDate, endDate, format);

                case ReportType.AuctionReport:
                    var auctionResult = await _dashboardService.GetAuctionDashboardAsync(startDate, endDate);
                    if (!auctionResult.IsSuccess)
                        return Result<FileResultDTO>.Failure(auctionResult.Error!);

                    var auctionReport = auctionResult.Value!;
                    fileName = $"auction_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                    if (format.Equals("PDF", StringComparison.OrdinalIgnoreCase))
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
                    await SaveReportDownload(reportType, startDate, endDate, format);
                    return Result<FileResultDTO>.Success(new FileResultDTO { Content = content, ContentType = contentType, FileName = fileName });

                case ReportType.UserReport:
                    var userResult = await _dashboardService.GetUserDashboardAsync(startDate, endDate);
                    if (!userResult.IsSuccess)
                        return Result<FileResultDTO>.Failure(userResult.Error!);

                    var userReport = userResult.Value!;
                    fileName = $"user_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                    if (format.Equals("PDF", StringComparison.OrdinalIgnoreCase))
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
                    await SaveReportDownload(reportType, startDate, endDate, format);
                    return Result<FileResultDTO>.Success(new FileResultDTO { Content = content, ContentType = contentType, FileName = fileName });

                case ReportType.RevenueReport:
                    var revenue = await _uow.Report.GetCommissionEarnedAsync(startDate, endDate);
                    fileName = $"revenue_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";

                    if (format.Equals("PDF", StringComparison.OrdinalIgnoreCase))
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
                    await SaveReportDownload(reportType, startDate, endDate, format);
                    return Result<FileResultDTO>.Success(new FileResultDTO { Content = content, ContentType = contentType, FileName = fileName });

                default:
                    return Result<FileResultDTO>.Failure("Invalid report type.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while exporting report {ReportType}.", reportType);
            return Result<FileResultDTO>.Failure("Failed to export report.");
        }
    }

    public async Task<Result<List<AuctionAnalyticsTableDTO>>> GetAuctionAnalyticsAsync(DateTime start, DateTime end, string? category)
    {
        try
        {
            var data = await _uow.Report.GetAuctionAnalyticsTableAsync(start, end, category);
            return Result<List<AuctionAnalyticsTableDTO>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching auction analytics.");
            return Result<List<AuctionAnalyticsTableDTO>>.Failure("Failed to fetch auction analytics.");
        }
    }

    public async Task<Result<List<UserAnalyticsTableDTO>>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var data = await _uow.Report.GetUserAnalyticsAsync(startDate, endDate);
            return Result<List<UserAnalyticsTableDTO>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching user analytics.");
            return Result<List<UserAnalyticsTableDTO>>.Failure("Failed to fetch user analytics.");
        }
    }

    public async Task<Result<List<RevenueTableAnalyticsDTO>>> GetRevenueTableAnalyticsAsync(DateTime start, DateTime end)
    {
        try
        {
            var data = await _uow.Report.GetRevenueTableAnalyticsAsync(start, end);
            return Result<List<RevenueTableAnalyticsDTO>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching revenue table analytics.");
            return Result<List<RevenueTableAnalyticsDTO>>.Failure("Failed to fetch revenue table analytics.");
        }
    }

    public async Task<Result<SummaryWithChange<decimal>>> GetSummaryAsync(string dataType, DateTime startDate, DateTime endDate)
    {
        try
        {
            var currentData = await _uow.Report.GetSummaryDataAsync(startDate, endDate, dataType);

            var periodLength = endDate - startDate;
            var prevStart = startDate - periodLength;
            var prevEnd = startDate;
            var previousData = await _uow.Report.GetSummaryDataAsync(prevStart, prevEnd, dataType);

            decimal currentTotal = currentData.Sum(x => x.Value);
            decimal previousTotal = previousData.Sum(x => x.Value);

            double percentChange = (previousTotal > 0)
                ? ((double)(currentTotal - previousTotal) / (double)previousTotal) * 100
                : (currentTotal > 0 ? 100 : 0);

            var result = new SummaryWithChange<decimal>
            {
                Data = currentData,
                PercentageChange = Math.Round(percentChange, 2)
            };

            return Result<SummaryWithChange<decimal>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching summary for {DataType}.", dataType);
            return Result<SummaryWithChange<decimal>>.Failure("Failed to fetch summary data.");
        }
    }

    public async Task<Result<PagedResult<RecentDownloads>>> GetRecentDownloadsAsync(int page, int pageSize)
    {
        try
        {
            var data = await _uow.Report.GetRecentDownloadsPagedAsync(page, pageSize);
            return Result<PagedResult<RecentDownloads>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching recent downloads.");
            return Result<PagedResult<RecentDownloads>>.Failure("Failed to fetch recent downloads.");
        }
    }

    private async Task SaveReportDownload(ReportType type, DateTime start, DateTime end, string format)
    {
        await _uow.Report.AddRecentDownload(new RecentDownloads
        {
            ReportType = type,
            DateRange = $"{start:yyyy-MM-dd} to {end:yyyy-MM-dd}",
            Format = format
        });
        await _uow.SaveChangesAsync();
    }
}
