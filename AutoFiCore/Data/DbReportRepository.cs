using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
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
        Task<Dictionary<string, decimal>> GetSummaryDataAsync(DateTime startDate, DateTime endDate, string dataType);
        Task AddRecentDownload(RecentDownloads recentDownloads);
        Task<PagedResult<RecentDownloads>> GetRecentDownloadsPagedAsync(int page = 1, int pageSize = 10);
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
                .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet && a.ScheduledStartTime >= startDate && a.ScheduledStartTime <= endDate)
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
        public async Task<Dictionary<string, decimal>> GetSummaryDataAsync(DateTime startDate, DateTime endDate, string dataType)
        {
            bool groupByDay = (endDate - startDate).TotalDays <= 31;

            if (dataType == "Revenue")
            {
                var query = _db.Auctions
                    .Where(a => a.Status == AuctionStatus.Ended && a.IsReserveMet
                                && a.ScheduledStartTime >= startDate
                                && a.ScheduledStartTime < endDate);

                if (groupByDay)
                {
                    var data = await query
                        .GroupBy(a => a.ScheduledStartTime.Date)
                        .Select(g => new
                        {
                            DateKey = g.Key.ToString("yyyy-MM-dd"),
                            Total = g.Sum(a => a.CurrentPrice)
                        })
                        .ToListAsync();

                    var allDays = Enumerable.Range(0, (endDate - startDate).Days)
                        .Select(offset => startDate.AddDays(offset).ToString("yyyy-MM-dd"))
                        .ToDictionary(d => d, _ => 0m);

                    foreach (var item in data)
                        allDays[item.DateKey] = item.Total;

                    return allDays;
                }
                else
                {
                    var data = await query
                        .GroupBy(a => new { a.ScheduledStartTime.Year, a.ScheduledStartTime.Month })
                        .Select(g => new
                        {
                            MonthKey = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                            Total = g.Sum(a => a.CurrentPrice)
                        })
                        .ToListAsync();

                    var monthDiff = ((endDate.Year - startDate.Year) * 12) + (endDate.Month - startDate.Month);
                    var allMonths = Enumerable.Range(0, monthDiff)
                        .Select(offset => startDate.AddMonths(offset))
                        .ToDictionary(
                            d => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(d.Month)} {d.Year}",
                            _ => 0m
                        );

                    foreach (var item in data)
                        allMonths[item.MonthKey] = item.Total;

                    return allMonths;
                }
            }
            else if (dataType == "Users")
            {
                var query = _db.Users
                    .Where(u => u.CreatedUtc >= startDate && u.CreatedUtc < endDate);

                if (groupByDay)
                {
                    var data = await query
                        .GroupBy(u => u.CreatedUtc.Date)
                        .Select(g => new
                        {
                            DateKey = g.Key.ToString("yyyy-MM-dd"),
                            Total = g.Count()
                        })
                        .ToListAsync();

                    var allDays = Enumerable.Range(0, (endDate - startDate).Days)
                        .Select(offset => startDate.AddDays(offset).ToString("yyyy-MM-dd"))
                        .ToDictionary(d => d, _ => 0m);

                    foreach (var item in data)
                        allDays[item.DateKey] = item.Total;

                    return allDays;
                }
                else
                {
                    var data = await query
                        .GroupBy(u => new { u.CreatedUtc.Year, u.CreatedUtc.Month })
                        .Select(g => new
                        {
                            MonthKey = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                            Total = g.Count()
                        })
                        .ToListAsync();

                    var monthDiff = ((endDate.Year - startDate.Year) * 12) + (endDate.Month - startDate.Month);
                    var allMonths = Enumerable.Range(0, monthDiff)
                        .Select(offset => startDate.AddMonths(offset))
                        .ToDictionary(
                            d => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(d.Month)} {d.Year}",
                            _ => 0m
                        );

                    foreach (var item in data)
                        allMonths[item.MonthKey] = item.Total;

                    return allMonths;
                }
            }

            throw new ArgumentException("Invalid data type");
        }
        public async Task AddRecentDownload(RecentDownloads recentDownloads)
        {
            _db.RecentDownloads.Add(recentDownloads);
            await _db.SaveChangesAsync();
        }
        public async Task<PagedResult<RecentDownloads>> GetRecentDownloadsPagedAsync(int page = 1, int pageSize = 10)
        {
            var query = _db.RecentDownloads
                .OrderByDescending(rd => rd.DownloadedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<RecentDownloads>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}