using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Globalization;

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
        Task<decimal> GetSuccessfulPaymentPercentageAsync(DateTime start, DateTime end);
        Task<decimal> GetCommissionEarnedAsync(DateTime start, DateTime end);
        Task<List<CategoryPerformance>> GetPopularCategoriesReportAsync(DateTime start, DateTime end);
        Task<double> GetAverageBidCountAsync(DateTime start, DateTime end);
        Task<List<string>> GetTopAuctionItemsAsync(DateTime start, DateTime end);
        Task<double> GetUserRetentionRateAsync(DateTime start, DateTime end);
        Task<decimal> GetAverageAuctionViewsAsync(DateTime start, DateTime end);
        Task<decimal> GetAverageAuctionBidsAsync(DateTime start, DateTime end);
        Task<int> GetAuctionViewCountAsync(int auctionId, DateTime start, DateTime end);
        Task<int> GetUniqueBiddersCountAsync(int auctionId, DateTime start, DateTime end);
        Task<int> GetTotalBidsCountAsync(int auctionId, DateTime start, DateTime end);
        Task<List<Auction>> GetAuctionsInDateRangeAsync(DateTime start, DateTime end);
        Task<List<AuctionAnalyticsTableDTO>> GetAuctionAnalyticsTableAsync(DateTime start, DateTime end, string? category);
        Task<List<UserAnalyticsTableDTO>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueTableAnalyticsDTO>> GetRevenueTableAnalyticsAsync(DateTime start, DateTime end);
        Task<decimal> GetRevenueForRangeAsync(DateTime from, DateTime to);
        Task<Dictionary<DateTime, decimal>> GetRevenueGroupedByDayAsync(DateTime from, DateTime to);
        Task<Dictionary<DateTime, decimal>> GetRevenueGroupedByMonthAsync(DateTime? from, DateTime? to);
        Task<Dictionary<DateTime, decimal>> GetAllTimeRevenueAsync();
        Task<Dictionary<DateTime, int>> GetUserRegistrationsGroupedByDayAsync(DateTime from, DateTime to);
        Task<Dictionary<DateTime, int>> GetUserRegistrationsGroupedByMonthAsync(DateTime from, DateTime to);
        Task<Dictionary<DateTime, int>> GetAllTimeUserRegistrationsAsync();
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
        public async Task<decimal> GetAverageAuctionBidsAsync(DateTime start, DateTime end)
        {
            var totalBids = await _db.AnalyticsEvents
                .Where(a => a.CreatedAt >= start && a.CreatedAt < end && a.EventType == AnalyticsEventType.BidPlaced)
                .CountAsync();

            var totalDays = (end.Date - start.Date).Days;

            return totalDays > 0 ? (decimal)totalBids / totalDays : totalBids;
        }
        public async Task<decimal> GetAverageAuctionViewsAsync(DateTime start, DateTime end)
        {
            var totalViews = await _db.AnalyticsEvents
                .Where(a => a.CreatedAt >= start && a.CreatedAt < end && a.EventType == AnalyticsEventType.AuctionView)
                .CountAsync();

            var totalDays = (end.Date - start.Date).Days;

            return totalDays > 0 ? (decimal)totalViews / totalDays : totalViews;
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

            return Math.Round(total * 0.10m, 2);
        }
        public async Task<decimal> GetSuccessfulPaymentPercentageAsync(DateTime start, DateTime end)
        {
            var successfulPayments = await _db.AnalyticsEvents
                .CountAsync(e => e.EventType == AnalyticsEventType.PaymentCompleted &&
                                 e.CreatedAt >= start && e.CreatedAt < end);

            var successfulAuctions = await _db.Auctions
                .CountAsync(a => a.Status == AuctionStatus.Ended &&
                                 a.IsReserveMet &&
                                 a.EndUtc >= start && a.EndUtc < end);

            if (successfulAuctions == 0)
                return 0;

            return Math.Round((decimal)successfulPayments * 100 / successfulAuctions, 2);
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
        public async Task<int> GetAuctionViewCountAsync(int auctionId, DateTime start, DateTime end)
        {
            return await _db.AnalyticsEvents
                .CountAsync(e => e.AuctionId == auctionId
                              && e.EventType == AnalyticsEventType.AuctionView
                              && e.CreatedAt >= start && e.CreatedAt < end);
        }
        public async Task<int> GetUniqueBiddersCountAsync(int auctionId, DateTime start, DateTime end)
        {
            return await _db.Bids
                .Where(b => b.AuctionId == auctionId
                         && b.CreatedUtc >= start && b.CreatedUtc < end)
                .Select(b => b.UserId)
                .Distinct()
                .CountAsync();
        }
        public async Task<int> GetTotalBidsCountAsync(int auctionId, DateTime start, DateTime end)
        {
            return await _db.Bids
                .CountAsync(b => b.AuctionId == auctionId
                              && b.CreatedUtc >= start && b.CreatedUtc < end);
        }
        public async Task<List<Auction>> GetAuctionsInDateRangeAsync(DateTime start, DateTime end)
        {
            return await _db.Auctions.Include(a => a.Vehicle).Where(a => a.CreatedUtc >= start && a.EndUtc < end).ToListAsync();
        }
        public async Task<List<AuctionAnalyticsTableDTO>> GetAuctionAnalyticsTableAsync(DateTime start, DateTime end, string? category)
        {
            var auctionsQuery = _db.Auctions
                .Include(a => a.Vehicle)
                .Where(a => a.CreatedUtc >= start && a.EndUtc < end);

            if (!string.IsNullOrWhiteSpace(category))
            {
                auctionsQuery = auctionsQuery.Where(a => a.Vehicle.FuelType == category);
            }

            var auctions = await auctionsQuery.ToListAsync();

            var result = new List<AuctionAnalyticsTableDTO>();

            foreach (var a in auctions)
            {
                var views = await GetAuctionViewCountAsync(a.AuctionId, start, end);
                var bidders = await GetUniqueBiddersCountAsync(a.AuctionId, start, end);
                var bids = await GetTotalBidsCountAsync(a.AuctionId, start, end);

                result.Add(new AuctionAnalyticsTableDTO
                {
                    AuctionId = a.AuctionId,
                    VehicleName = $"{a.Vehicle.Year} {a.Vehicle.Make} {a.Vehicle.Model}",
                    VehicleCategory = a.Vehicle.FuelType!.ToString(),
                    Views = views,
                    Bidders = bidders,
                    Bids = bids,
                    FinalPrice = a.CurrentPrice,
                    Status = a.Status == AuctionStatus.Ended
                        ? (a.IsReserveMet ? "Sold" : "Unsold")
                        : "Pending"
                });
            }

            return result.OrderBy(r => r.AuctionId).ToList();
        }
        public async Task<List<UserAnalyticsTableDTO>> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var filteredUsers = await _db.Users
                .Where(u =>
                    u.CreatedUtc >= startDate && u.CreatedUtc <= endDate &&
                    u.LastLoggedIn >= startDate && u.LastLoggedIn <= endDate)
                .OrderBy(u => u.Id)
                .ToListAsync();

            var winningBidUserIds = await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet)
                .Select(a => new
                {
                    a.AuctionId,
                    LastBidUserId = _db.Bids
                        .Where(b => b.AuctionId == a.AuctionId)
                        .OrderByDescending(b => b.CreatedUtc)
                        .Select(b => b.UserId)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return filteredUsers.Select(user =>
            {
                var totalBids = _db.Bids.Count(b =>
                    b.UserId == user.Id &&
                    b.CreatedUtc >= startDate &&
                    b.CreatedUtc <= endDate);

                var totalWins = winningBidUserIds.Count(w => w.LastBidUserId == user.Id);

                return new UserAnalyticsTableDTO
                {
                    UserName = user.Name,
                    RegistrationDate = user.CreatedUtc,
                    LastActive = user.LastLoggedIn,
                    TotalBids = totalBids,
                    TotalWins = totalWins
                };
            }).ToList();
        }
        public async Task<List<RevenueTableAnalyticsDTO>> GetRevenueTableAnalyticsAsync(DateTime start, DateTime end)
        {
            var auctions = await _db.Auctions
                .Include(a => a.Vehicle)
                .Include(a => a.Bids)
                .Where(a => a.StartUtc >= start && a.StartUtc < end)
                .ToListAsync();

            var auctionWinners = await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet)
                .Select(a => new
                {
                    a.AuctionId,
                    LastBidUserId = _db.Bids
                        .Where(b => b.AuctionId == a.AuctionId)
                        .OrderByDescending(b => b.CreatedUtc)
                        .Select(b => b.UserId)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var userIds = auctionWinners
                .Select(w => w.LastBidUserId)
                .Distinct()
                .ToList();

            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            var revenueList = auctions.Select(auction =>
            {
                var isSold = auction.Status == AuctionStatus.Ended && auction.IsReserveMet;
                var winner = auctionWinners.FirstOrDefault(w => w.AuctionId == auction.AuctionId);
                var buyerName = (isSold && winner != null && users.ContainsKey(winner.LastBidUserId))
                    ? users[winner.LastBidUserId]
                    : "---";

                return new RevenueTableAnalyticsDTO
                {
                    ScheduledStartTime = auction.StartUtc,
                    AuctionId = auction.AuctionId,
                    Vehicle = $"{auction.Vehicle?.Year} {auction.Vehicle?.Make} {auction.Vehicle?.Model}",
                    Buyer = isSold ? buyerName : "---",
                    Revenue = auction.CurrentPrice,
                    Commission = Math.Round(auction.CurrentPrice * 0.10m, 2)
                };
            }).OrderBy(r => r.AuctionId).ToList();

            return revenueList;
        }
        public async Task<decimal> GetRevenueForRangeAsync(DateTime from, DateTime to)
        {
            from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            return await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet &&
                            a.ScheduledStartTime >= from && a.ScheduledStartTime < to)
                .SumAsync(a => a.CurrentPrice);
        }
        public async Task<Dictionary<DateTime, decimal>> GetRevenueGroupedByDayAsync(DateTime from, DateTime to)
        {
            from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var data = await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet && a.ScheduledStartTime >= from && a.ScheduledStartTime < to)
                .GroupBy(a => a.ScheduledStartTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(a => a.CurrentPrice)
                })
                .ToListAsync();

            var result = new Dictionary<DateTime, decimal>();
            for (DateTime date = from.Date; date < to.Date; date = date.AddDays(1))
            {
                result[date] = 0;
            }

            foreach (var item in data)
            {
                result[item.Date] = item.Revenue;
            }

            return result;
        }
        public async Task<Dictionary<DateTime, decimal>> GetRevenueGroupedByMonthAsync(DateTime? from, DateTime? to)
        {
            DateTime start = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : DateTime.MinValue;
            DateTime end = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : DateTime.UtcNow;

            var data = await _db.Auctions
                .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet && a.ScheduledStartTime >= start && a.ScheduledStartTime < end)
                .GroupBy(a => new { a.ScheduledStartTime.Year, a.ScheduledStartTime.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Revenue = g.Sum(a => a.CurrentPrice)
                })
                .ToListAsync();

            var result = new Dictionary<DateTime, decimal>();
            var temp = new DateTime(start.Year, start.Month, 1);
            var endMonth = new DateTime(end.Year, end.Month, 1);

            while (temp < endMonth)
            {
                result[temp] = 0;
                temp = temp.AddMonths(1);
            }

            foreach (var item in data)
            {
                result[item.Month] = item.Revenue;
            }

            return result;
        }
        public async Task<Dictionary<DateTime, decimal>> GetAllTimeRevenueAsync()
        {
            var oldestAuction = await _db.Auctions
                .OrderBy(a => a.ScheduledStartTime)
                .Select(a => a.ScheduledStartTime)
                .FirstOrDefaultAsync();

            if (oldestAuction == default)
            {
                return new Dictionary<DateTime, decimal>();
            }

            var oldestMonth = DateTime.SpecifyKind(new DateTime(oldestAuction.Year, oldestAuction.Month, 1), DateTimeKind.Utc);
            var currentMonth = DateTime.SpecifyKind(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTimeKind.Utc);
            var fromMonth = oldestMonth;

            if (oldestMonth == currentMonth)
            {
                fromMonth = currentMonth.AddMonths(-1);
            }

            fromMonth = DateTime.SpecifyKind(fromMonth, DateTimeKind.Utc);

            var result = new Dictionary<DateTime, decimal>();

            var temp = fromMonth;
            while (temp <= currentMonth)
            {
                result[temp] = 0;
                temp = DateTime.SpecifyKind(temp.AddMonths(1), DateTimeKind.Utc);
            }

            var toMonthExclusive = DateTime.SpecifyKind(currentMonth.AddMonths(1), DateTimeKind.Utc);

            var groupedRevenue = await _db.Auctions
                .Where(a =>
                    a.Status == AuctionStatus.Ended &&
                    a.IsReserveMet &&
                    a.ScheduledStartTime >= fromMonth &&
                    a.ScheduledStartTime < toMonthExclusive)
                .GroupBy(a => new { a.ScheduledStartTime.Year, a.ScheduledStartTime.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Revenue = g.Sum(a => a.CurrentPrice)
                })
                .ToListAsync();

            foreach (var item in groupedRevenue)
            {
                var monthKey = DateTime.SpecifyKind(item.Month, DateTimeKind.Utc);
                result[monthKey] = item.Revenue;
            }

            return result;
        }
        public async Task<Dictionary<DateTime, int>> GetUserRegistrationsGroupedByDayAsync(DateTime from, DateTime to)
        {
            from = DateTime.SpecifyKind(from.Date, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to.Date.AddDays(1), DateTimeKind.Utc); 

            var allDays = Enumerable.Range(0, (to - from).Days)
                .Select(offset => from.AddDays(offset))
                .ToDictionary(date => date, _ => 0);

            var registrations = await _db.Users
                .Where(u => u.CreatedUtc >= from && u.CreatedUtc < to)
                .GroupBy(u => u.CreatedUtc.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in registrations)
            {
                var date = DateTime.SpecifyKind(item.Date, DateTimeKind.Utc);
                allDays[date] = item.Count;
            }

            return allDays;
        }
        public async Task<Dictionary<DateTime, int>> GetUserRegistrationsGroupedByMonthAsync(DateTime from, DateTime to)
        {
            from = DateTime.SpecifyKind(new DateTime(from.Year, from.Month, 1), DateTimeKind.Utc);
            to = DateTime.SpecifyKind(new DateTime(to.Year, to.Month, 1).AddMonths(1), DateTimeKind.Utc);

            var months = new Dictionary<DateTime, int>();
            var cursor = from;
            while (cursor < to)
            {
                months[cursor] = 0;
                cursor = cursor.AddMonths(1);
            }

            var registrations = await _db.Users
                .Where(u => u.CreatedUtc >= from && u.CreatedUtc < to)
                .GroupBy(u => new { u.CreatedUtc.Year, u.CreatedUtc.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToListAsync();

            foreach (var item in registrations)
            {
                var month = DateTime.SpecifyKind(item.Month, DateTimeKind.Utc);
                months[month] = item.Count;
            }

            return months;
        }
        public async Task<Dictionary<DateTime, int>> GetAllTimeUserRegistrationsAsync()
        {
            var oldestUser = await _db.Users
                .OrderBy(u => u.CreatedUtc)
                .Select(u => u.CreatedUtc)
                .FirstOrDefaultAsync();

            if (oldestUser == default)
                return new Dictionary<DateTime, int>();

            var oldestMonth = DateTime.SpecifyKind(new DateTime(oldestUser.Year, oldestUser.Month, 1), DateTimeKind.Utc);
            var currentMonth = DateTime.SpecifyKind(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTimeKind.Utc);
            var fromMonth = oldestMonth;

            if (oldestMonth == currentMonth)
                fromMonth = currentMonth.AddMonths(-1);

            return await GetUserRegistrationsGroupedByMonthAsync(fromMonth, currentMonth);
        }

    }
}