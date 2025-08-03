using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Enums;

public interface IDashboardService
{
    Task<ExecutiveDashboard> GetExecutiveDashboardAsync(DateTime startDate, DateTime endDate);
    Task<AuctionDashboard> GetAuctionDashboardAsync(DateTime startDate, DateTime endDate);
    Task<UserDashboard> GetUserDashboardAsync(DateTime startDate, DateTime endDate);
}

public class DashboardService : IDashboardService
{
    private readonly IReportingService _reportService;
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IReportingService reportService, IUnitOfWork unitOfWork)
    {
        _reportService = reportService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ExecutiveDashboard> GetExecutiveDashboardAsync(DateTime start, DateTime end)
    {
        var revenue = await _reportService.GetRevenueReportAsync(start, end);
        var user = await _reportService.GetUserActivityReportAsync(start, end);

        return new ExecutiveDashboard
        {
            TotalRevenue = revenue.TotalRevenue,
            NewUsers = user.NewRegistrations,
            ActiveAuctions = await _reportService.GetAuctionPerformanceReportAsync(start, end)
                .ContinueWith(r => r.Result.TotalAuctions)
        };
    }
    public async Task<AuctionDashboard> GetAuctionDashboardAsync(DateTime start, DateTime end)
    {
        var performance = await _reportService.GetAuctionPerformanceReportAsync(start, end);

        var averageBidCount = await _unitOfWork.Report.GetAverageBidCountAsync(start, end);

        var topItems = await _unitOfWork.Report.GetTopAuctionItemsAsync(start, end);

        return new AuctionDashboard
        {
            CompletionRate = performance.SuccessRate,
            AverageBidCount = Math.Round(averageBidCount, 2),
            TopItems = topItems
        };
    }
    public async Task<UserDashboard> GetUserDashboardAsync(DateTime start, DateTime end)
    {
        var user = await _reportService.GetUserActivityReportAsync(start, end);
        var retentionRate = await _unitOfWork.Report.GetUserRetentionRateAsync(start, end);
        return new UserDashboard
        {
            Registrations = user.NewRegistrations,
            Engagement = user.EngagementScore,
            RetentionRate = retentionRate,
        };
    }
}
