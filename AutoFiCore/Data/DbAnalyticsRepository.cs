using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoFiCore.Data
{
    public interface IAnalyticsRepository
    {
        Task AddEventAsync(AnalyticsEvent analyticsEvent);
        Task<Auction?> GetAuctionWithAnalyticsAsync(int auctionId);
    }
    public class DbAnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DbAnalyticsRepository> _logger;

        public DbAnalyticsRepository(ApplicationDbContext context, ILogger<DbAnalyticsRepository> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task AddEventAsync(AnalyticsEvent analyticsEvent)
        {
            _dbContext.AnalyticsEvents.Add(analyticsEvent);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Auction?> GetAuctionWithAnalyticsAsync(int auctionId)
        {
            return await _dbContext.Auctions
                .Include(a => a.AuctionAnalytics)
                .FirstOrDefaultAsync(a => a.AuctionId == auctionId);
        }
    }
}
