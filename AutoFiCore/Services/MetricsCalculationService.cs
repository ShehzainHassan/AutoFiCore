using AutoFiCore.Data;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public interface IMetricsCalculationService
{
    Task CalculateDailyMetricsAsync(DateTime date);
    Task UpdateAuctionAnalyticsAsync(int auctionId);
    Task<decimal> CalculateUserEngagementAsync(int userId, DateTime startDate, DateTime endDate);
    Task<decimal> GenerateRevenueMetricsAsync(DateTime startDate, DateTime endDate);
}

public class MetricsCalculationService : IMetricsCalculationService
{
    private readonly IMetricsRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MetricsCalculationService> _logger;

    public MetricsCalculationService(IMetricsRepository repo, ILogger<MetricsCalculationService> logger, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task CalculateDailyMetricsAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var auctionCount = await _unitOfWork.Metrics.GetAuctionCountAsync(start, end);
        var bidCount = await _unitOfWork.Bids.GetBidCountAsync(start, end);
        var userCount = await _unitOfWork.Metrics.GetUserCountAsync(start, end);
        var revenueTotal = await _repo.GetRevenueTotalAsync(start, end);

        var metrics = new List<DailyMetric>
        {
            new() { Date = DateOnly.FromDateTime(start), MetricType = MetricType.AuctionCount, Count = auctionCount },
            new() { Date = DateOnly.FromDateTime(start), MetricType = MetricType.BidCount, Count = bidCount },
            new() { Date = DateOnly.FromDateTime(start), MetricType = MetricType.UserCount, Count = userCount },
            new() { Date = DateOnly.FromDateTime(start), MetricType = MetricType.RevenueTotal, Value = revenueTotal }
        };

        await _repo.SaveDailyMetricsAsync(metrics);
    }
    public async Task UpdateAuctionAnalyticsAsync(int auctionId)
    {
        var views = await _repo.GetAuctionViewsAsync(auctionId);
        var uniqueBidders = await _unitOfWork.Bids.GetUniqueBiddersCountAsync(auctionId);
        var totalBids = await _unitOfWork.Bids.GetTotalBidsAsync(auctionId);
        var auction = await _unitOfWork.Auctions.GetAuctionByIdAsync(auctionId);

        if (auction == null) return;

        var viewToBidRatio = views == 0 ? 0 : Math.Round((decimal)totalBids / views, 2);

        var analytics = await _repo.GetAuctionAnalyticsAsync(auctionId) ?? new AuctionAnalytics
        {
            AuctionId = auctionId
        };

        analytics.TotalViews = views;
        analytics.UniqueBidders = uniqueBidders;
        analytics.TotalBids = totalBids;
        analytics.ViewToBidRatio = viewToBidRatio;
        analytics.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveAuctionAnalyticsAsync(analytics);
    }
    public Task<decimal> CalculateUserEngagementAsync(int userId, DateTime start, DateTime end)
    {
        return _repo.CalculateUserEngagementAsync(userId, start, end);
    }
    public Task<decimal> GenerateRevenueMetricsAsync(DateTime start, DateTime end)
    {
        return _repo.GetRevenueTotalAsync(start, end);
    }
}
