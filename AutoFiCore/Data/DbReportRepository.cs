using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Data
{
    public interface IReportRepository
    {
        Task<int> GetTotalAuctionsAsync(DateTime start, DateTime end);
        Task<int> GetSuccessfulAuctionsAsync(DateTime start, DateTime end);
        Task<decimal> GetAverageAuctionPriceAsync(DateTime start, DateTime end);
        Task<int> GetActiveUserCountAsync(DateTime start, DateTime end);
        Task<int> GetNewUserCountAsync(DateTime start, DateTime end);
        Task<decimal> GetUserEngagementScoreAsync(DateTime start, DateTime end);
        Task<int> GetSuccessfulPaymentsCountAsync(DateTime start, DateTime end);
        Task<decimal> GetCommissionEarnedAsync(DateTime start, DateTime end);
        Task<List<CategoryPerformance>> GetPopularCategoriesReportAsync(DateTime start, DateTime end);
        Task<double> GetAverageBidCountAsync(DateTime start, DateTime end);
        Task<List<string>> GetTopAuctionItemsAsync(DateTime start, DateTime end);
        Task<double> GetUserRetentionRateAsync(DateTime start, DateTime end);

    }

    public class DbReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _db;

        public DbReportRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public Task<int> GetTotalAuctionsAsync(DateTime start, DateTime end)
        {
            return _db.Auctions.CountAsync(a => a.CreatedUtc >= start && a.CreatedUtc < end);
        }
        public Task<int> GetSuccessfulAuctionsAsync(DateTime start, DateTime end)
        {
            return _db.Auctions
                .CountAsync(a => a.Status == AuctionStatus.Ended && a.IsReserveMet && a.EndUtc >= start && a.EndUtc < end);
        }
        public async Task<decimal> GetAverageAuctionPriceAsync(DateTime start, DateTime end)
        {
            return await _db.Auctions.Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet && a.EndUtc >= start && a.EndUtc < end)
                .AverageAsync(a => (decimal?)a.CurrentPrice) ?? 0;
        }
        public Task<int> GetActiveUserCountAsync(DateTime start, DateTime end)
        {
            return _db.AnalyticsEvents
                .Where(e => e.CreatedAt >= start && e.CreatedAt < end && e.UserId != null)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();
        }
        public Task<int> GetNewUserCountAsync(DateTime start, DateTime end)
        {
            return _db.Users.CountAsync(u => u.CreatedUtc >= start && u.CreatedUtc < end);
        }
        public async Task<decimal> GetUserEngagementScoreAsync(DateTime start, DateTime end)
        {
            var totalViews = await _db.AnalyticsEvents
                .CountAsync(e => e.EventType == AnalyticsEventType.AuctionView &&
                                 e.CreatedAt >= start && e.CreatedAt < end);

            var totalBids = await _db.Bids
                .CountAsync(b => b.CreatedUtc >= start && b.CreatedUtc < end);

            var totalWatchlistAdds = await _db.Watchlists
                .CountAsync(w => w.CreatedUtc >= start && w.CreatedUtc < end);

            decimal engagementScore = totalViews * 1 + totalBids * 2 + totalWatchlistAdds * 1;

            return engagementScore;
        }
        public async Task<decimal> GetCommissionEarnedAsync(DateTime start, DateTime end)
        {
            var total = await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet && a.EndUtc >= start && a.EndUtc < end)
                .SumAsync(a => (decimal?)a.CurrentPrice) ?? 0;

            return Math.Round(total * 0.05m, 2);
        }
        public Task<int> GetSuccessfulPaymentsCountAsync(DateTime start, DateTime end)
        {
            return _db.AnalyticsEvents
                .CountAsync(e => e.EventType == AnalyticsEventType.PaymentCompleted && e.CreatedAt >= start && e.CreatedAt < end);
        }
        public async Task<List<CategoryPerformance>> GetPopularCategoriesReportAsync(DateTime start, DateTime end)
        {
            return await _db.Auctions
                .Where(a => a.CreatedUtc >= start && a.CreatedUtc < end && a.Vehicle != null)
                .Include(a => a.Vehicle)
                .GroupBy(a => a.Vehicle!.FuelType)
                .Where(g => g.Key != null) 
                .Select(g => new CategoryPerformance
                {
                    CategoryName = g.Key!.ToString(), 
                    AuctionCount = g.Count(),
                    AveragePrice = g.Average(a => a.CurrentPrice),
                    SuccessRate = g.Count(a => a.Status == AuctionStatus.Ended && a.IsReserveMet) * 100.0 / g.Count()
                })
                .OrderByDescending(c => c.AuctionCount)
                .ToListAsync();
        }
        public async Task<double> GetAverageBidCountAsync(DateTime start, DateTime end)
        {
            return await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.EndUtc >= start && a.EndUtc < end)
                .Select(a => a.Bids.Count)
                .DefaultIfEmpty(0)
                .AverageAsync();
        }
        public async Task<List<string>> GetTopAuctionItemsAsync(DateTime start, DateTime end)
        {
            return await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.EndUtc >= start && a.EndUtc < end && a.Vehicle != null)
                .Include(a => a.Vehicle)
                .OrderByDescending(a => a.Bids.Count)
                .Take(5) 
                .Select(a => a.Vehicle.Make + " " + a.Vehicle.Model + " " + a.Vehicle.Year)
                .ToListAsync();
        }
        public async Task<double> GetUserRetentionRateAsync(DateTime start, DateTime end)
        {
            var previousUsers = await _db.Users
                .Where(u => u.CreatedUtc < start)
                .Select(u => u.Id)
                .ToListAsync();

            if (previousUsers.Count == 0)
                return 0;

            var returningUsers = await _db.AnalyticsEvents
                .Where(e => previousUsers.Contains(e.UserId ?? -1)
                    && e.CreatedAt >= start && e.CreatedAt < end)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();

            return Math.Round((double)returningUsers * 100 / previousUsers.Count, 2);
        }
    }
}
