using AutoFiCore.Data;
using AutoFiCore.Data.Interfaces;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

    public async Task<Result<bool>> CalculateDailyMetricsAsync(DateTime date)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();
            try
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
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to calculate daily metrics for Date={Date}", date);
                return Result<bool>.Failure("Failed to calculate daily metrics.");
            }
        });
    }

    public async Task<Result<bool>> UpdateAuctionAnalyticsAsync(int auctionId)
    {
        try
        {
            var views = await _repo.GetAuctionViewsAsync(auctionId);
            var uniqueBidders = await _unitOfWork.Bids.GetUniqueBiddersCountAsync(auctionId);
            var totalBids = await _unitOfWork.Bids.GetTotalBidsAsync(auctionId);
            var auction = await _unitOfWork.Auctions.GetAuctionByIdAsync(auctionId);

            if (auction == null)
            {
                _logger.LogWarning("Auction {AuctionId} not found while updating analytics.", auctionId);
                return Result<bool>.Failure("Auction not found.");
            }

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

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update auction analytics for AuctionId={AuctionId}", auctionId);
            return Result<bool>.Failure("Failed to update auction analytics.");
        }
    }

    public async Task<Result<decimal>> CalculateUserEngagementAsync(int userId, DateTime start, DateTime end)
    {
        try
        {
            var engagement = await _repo.CalculateUserEngagementAsync(userId, start, end);
            return Result<decimal>.Success(engagement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate user engagement for UserId={UserId}", userId);
            return Result<decimal>.Failure("Failed to calculate user engagement.");
        }
    }

    public async Task<Result<decimal>> GenerateRevenueMetricsAsync(DateTime start, DateTime end)
    {
        try
        {
            var revenue = await _repo.GetRevenueTotalAsync(start, end);
            return Result<decimal>.Success(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate revenue metrics between {Start} and {End}", start, end);
            return Result<decimal>.Failure("Failed to generate revenue metrics.");
        }
    }
}
