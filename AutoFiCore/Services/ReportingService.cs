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
    Task<List<AuctionAnalyticsTableDTO>> GetAuctionAnalyticsAsync(DateTime start, DateTime end, string? category);
    Task<List<UserAnalyticsTableDTO>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<List<RevenueTableAnalyticsDTO>> GetRevenueTableAnalyticsAsync(DateTime start, DateTime end);
    Task<SummaryWithChange<decimal>> GetRevenueSummaryAsync(SummaryPeriod period);
    Task<SummaryWithChange<int>> GetUserRegistrationSummaryAsync(SummaryPeriod period);
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
            AverageSalePrice = await _uow.Report.GetAverageAuctionPriceAsync(start, end),
            SuccessfulPaymentsPercentage = await _uow.Report.GetSuccessfulPaymentPercentageAsync(start, end)
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
    public async Task<List<AuctionAnalyticsTableDTO>> GetAuctionAnalyticsAsync(DateTime start, DateTime end, string? category)
    {  
        return await _uow.Report.GetAuctionAnalyticsTableAsync(start, end, category);
    }
    public async Task<List<UserAnalyticsTableDTO>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        return await _uow.Report.GetUserAnalyticsAsync(startDate, endDate);
    }
    public async Task<List<RevenueTableAnalyticsDTO>> GetRevenueTableAnalyticsAsync(DateTime start, DateTime end)
    {
        return await _uow.Report.GetRevenueTableAnalyticsAsync(start, end);
    }
    public async Task<SummaryWithChange<decimal>> GetRevenueSummaryAsync(SummaryPeriod period)
    {
        var now = DateTime.UtcNow.Date;
        var results = new Dictionary<string, decimal>();
        decimal currentTotal = 0, previousTotal = 0;

        switch (period)
        {
            case SummaryPeriod.Last7Days:
            {
                var currentFrom = now.AddDays(-7);
                var currentTo = now;
                var previousFrom = currentFrom.AddDays(-7);
                var previousTo = currentFrom;

                var currentData = await _uow.Report.GetRevenueGroupedByDayAsync(currentFrom, currentTo);
                var previousData = await _uow.Report.GetRevenueGroupedByDayAsync(previousFrom, previousTo);

                for (var date = currentFrom; date < currentTo; date = date.AddDays(1))
                {
                    var key = date.ToString("yyyy-MM-dd");
                    decimal val = currentData.TryGetValue(date, out var v) ? v : 0m;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.Last2Weeks:
            {
                var currentFrom = now.AddDays(-14);
                var currentTo = now;
                var previousFrom = currentFrom.AddDays(-14);
                var previousTo = currentFrom;

                var currentData = await _uow.Report.GetRevenueGroupedByDayAsync(currentFrom, currentTo);
                var previousData = await _uow.Report.GetRevenueGroupedByDayAsync(previousFrom, previousTo);

                for (var date = currentFrom; date < currentTo; date = date.AddDays(1))
                {
                    var key = date.ToString("yyyy-MM-dd");
                    decimal val = currentData.TryGetValue(date, out var v) ? v : 0m;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.LastMonth:
            {
                var currentStart = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                var currentEnd = currentStart.AddMonths(1);
                var previousStart = currentStart.AddMonths(-1);
                var previousEnd = currentStart;

                var currentData = await _uow.Report.GetRevenueGroupedByDayAsync(currentStart, currentEnd);
                var previousData = await _uow.Report.GetRevenueGroupedByDayAsync(previousStart, previousEnd);

                for (var date = currentStart; date < currentEnd; date = date.AddDays(1))
                {
                    var key = date.ToString("yyyy-MM-dd");
                    decimal val = currentData.TryGetValue(date, out var v) ? v : 0m;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.LastQuarter:
            {
                var start = new DateTime(now.Year, now.Month, 1).AddMonths(-3);
                var end = new DateTime(now.Year, now.Month, 1);
                var dbData = await _uow.Report.GetRevenueGroupedByMonthAsync(start, end);

                for (var m = 0; m < 3; m++)
                {
                    var date = start.AddMonths(m);
                    var key = date.ToString("MMMM");
                    results[key] = dbData.TryGetValue(date, out var val) ? val : 0;
                }

                break;
            }
            case SummaryPeriod.Last12Months:
            {
                int months = period == SummaryPeriod.LastQuarter ? 3 : 12;
                var currentStart = new DateTime(now.Year, now.Month, 1).AddMonths(-months);
                var currentEnd = new DateTime(now.Year, now.Month, 1);
                var previousStart = currentStart.AddMonths(-months);
                var previousEnd = currentStart;

                var currentData = await _uow.Report.GetRevenueGroupedByMonthAsync(currentStart, currentEnd);
                var previousData = await _uow.Report.GetRevenueGroupedByMonthAsync(previousStart, previousEnd);

                for (int i = 0; i < months; i++)
                {
                    var date = currentStart.AddMonths(i);
                    var key = date.ToString("MMMM");
                    decimal val = currentData.TryGetValue(date, out var v) ? v : 0m;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.AllTime:
            {
                var allData = await _uow.Report.GetAllTimeRevenueAsync();

                foreach (var revenue in allData.OrderBy(x => x.Key))
                {
                    var key = revenue.Key.ToString("MMMM");
                    results[key] = revenue.Value;
                }

                currentTotal = results.Sum(x => x.Value);
                break;
            }

            default:
                throw new ArgumentException("Invalid period", nameof(period));
        }

        double percentChange = (previousTotal > 0)
            ? ((double)(currentTotal - previousTotal) / (double)previousTotal) * 100
            : (currentTotal > 0 ? 100 : 0);

        return new SummaryWithChange<decimal>
        {
            Data = results,
            PercentageChange = Math.Round(percentChange, 2)
        };
    }
    public async Task<SummaryWithChange<int>> GetUserRegistrationSummaryAsync(SummaryPeriod period)
    {
        var today = DateTime.UtcNow.Date;
        var results = new Dictionary<string, int>();
        int currentTotal = 0, previousTotal = 0;

        switch (period)
        {
            case SummaryPeriod.Last7Days:
            {
                var currentFrom = today.AddDays(-7);
                var currentTo = today;
                var previousFrom = currentFrom.AddDays(-7);
                var previousTo = currentFrom;

                var currentData = await _uow.Report.GetUserRegistrationsGroupedByDayAsync(currentFrom, currentTo);
                var previousData = await _uow.Report.GetUserRegistrationsGroupedByDayAsync(previousFrom, previousTo);

                for (var date = currentFrom; date < currentTo; date = date.AddDays(1))
                {
                    var key = date.ToString("yyyy-MM-dd");
                    int val = currentData.TryGetValue(date, out var v) ? v : 0;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.Last2Weeks:
            {
                var currentFrom = today.AddDays(-14);
                var currentTo = today;
                var previousFrom = currentFrom.AddDays(-14);
                var previousTo = currentFrom;

                var currentData = await _uow.Report.GetUserRegistrationsGroupedByDayAsync(currentFrom, currentTo);
                var previousData = await _uow.Report.GetUserRegistrationsGroupedByDayAsync(previousFrom, previousTo);

                for (var date = currentFrom; date < currentTo; date = date.AddDays(1))
                {
                    var key = date.ToString("yyyy-MM-dd");
                    int val = currentData.TryGetValue(date, out var v) ? v : 0;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.LastMonth:
            {
                var start = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                var end = start.AddMonths(1);
                var prevStart = start.AddMonths(-1);
                var prevEnd = start;

                var currentData = await _uow.Report.GetUserRegistrationsGroupedByDayAsync(start, end);
                var previousData = await _uow.Report.GetUserRegistrationsGroupedByDayAsync(prevStart, prevEnd);

                for (var date = start; date < end; date = date.AddDays(1))
                {
                    var key = date.ToString("yyyy-MM-dd");
                    int val = currentData.TryGetValue(date, out var v) ? v : 0;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.LastQuarter:
            {
                var start = new DateTime(today.Year, today.Month, 1).AddMonths(-3);
                var end = new DateTime(today.Year, today.Month, 1);
                var dbData = await _uow.Report.GetUserRegistrationsGroupedByMonthAsync(start, end);

                for (var m = 0; m < 3; m++)
                {
                    var date = start.AddMonths(m);
                    var key = date.ToString("MMMM");
                    results[key] = dbData.TryGetValue(date, out var val) ? val : 0;
                }

                break;
            }
            case SummaryPeriod.Last12Months:
            {
                int months = period == SummaryPeriod.LastQuarter ? 3 : 12;
                var start = new DateTime(today.Year, today.Month, 1).AddMonths(-months);
                var end = new DateTime(today.Year, today.Month, 1);
                var prevStart = start.AddMonths(-months);
                var prevEnd = start;

                var currentData = await _uow.Report.GetUserRegistrationsGroupedByMonthAsync(start, end);
                var previousData = await _uow.Report.GetUserRegistrationsGroupedByMonthAsync(prevStart, prevEnd);

                for (int i = 0; i < months; i++)
                {
                    var date = start.AddMonths(i);
                    var key = date.ToString("MMMM");
                    int val = currentData.TryGetValue(date, out var v) ? v : 0;
                    results[key] = val;
                    currentTotal += val;
                }

                previousTotal = previousData.Sum(x => x.Value);
                break;
            }

            case SummaryPeriod.AllTime:
            {
                var allData = await _uow.Report.GetAllTimeUserRegistrationsAsync();

                foreach (var user in allData.OrderBy(x => x.Key))
                {
                    var key = user.Key.ToString("MMMM");
                    results[key] = user.Value;
                }

                currentTotal = results.Sum(x => x.Value);
                break;
            }

            default:
                throw new ArgumentException("Invalid period", nameof(period));
        }

        double percentChange = (previousTotal > 0)
            ? ((double)(currentTotal - previousTotal) / previousTotal) * 100
            : (currentTotal > 0 ? 100 : 0);

        return new SummaryWithChange<int>
        {
            Data = results,
            PercentageChange = Math.Round(percentChange, 2)
        };
    }
}
