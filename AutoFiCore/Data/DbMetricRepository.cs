﻿using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Data
{
    public interface IMetricsRepository
    {
        Task<decimal> GetRevenueTotalAsync(DateTime start, DateTime end);
        Task SaveDailyMetricsAsync(IEnumerable<DailyMetric> metrics);
        Task<int> GetAuctionViewsAsync(int auctionId);
        Task<AuctionAnalytics?> GetAuctionAnalyticsAsync(int auctionId);
        Task SaveAuctionAnalyticsAsync(AuctionAnalytics analytics);
        Task<decimal> CalculateUserEngagementAsync(int userId, DateTime start, DateTime end);
    }


    public class DbMetricsRepository : IMetricsRepository
    {
        private readonly ApplicationDbContext _db;

        public DbMetricsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetRevenueTotalAsync(DateTime start, DateTime end)
        {
            var total = await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended &&
                            a.IsReserveMet &&
                            a.EndUtc >= start && a.EndUtc < end)
                .SumAsync(a => (decimal?)a.CurrentPrice);

            return total ?? 0m;
        }
        public Task SaveDailyMetricsAsync(IEnumerable<DailyMetric> metrics)
        {
            _db.DailyMetrics.AddRange(metrics);
            return _db.SaveChangesAsync();
        }
        public Task<AuctionAnalytics?> GetAuctionAnalyticsAsync(int auctionId) =>
            _db.AuctionAnalytics.FirstOrDefaultAsync(a => a.AuctionId == auctionId);
        public async Task SaveAuctionAnalyticsAsync(AuctionAnalytics analytics)
        {
            if (_db.Entry(analytics).State == EntityState.Detached)
                _db.AuctionAnalytics.Add(analytics);

            await _db.SaveChangesAsync();
        }
        public Task<int> GetAuctionViewsAsync(int auctionId)
        {
            return _db.AnalyticsEvents
                .Where(e => e.AuctionId == auctionId && e.EventType == AnalyticsEventType.AuctionView)
                .CountAsync();
        }

        public async Task<decimal> CalculateUserEngagementAsync(int userId, DateTime start, DateTime end)
        {
            var views = await _db.AnalyticsEvents
                .CountAsync(e => e.UserId == userId
                              && e.EventType == AnalyticsEventType.AuctionView
                              && e.CreatedAt >= start && e.CreatedAt < end);

            var bids = await _db.Bids
                .CountAsync(b => b.UserId == userId
                              && b.CreatedUtc >= start && b.CreatedUtc < end);

            var watchlistAdds = await _db.Watchlists
                .CountAsync(w => w.UserId == userId
                              && w.CreatedUtc >= start && w.CreatedUtc < end);

            var engagementScore = views * 1 + bids * 2 + watchlistAdds * 1;

            return engagementScore;
        }


    }

}
