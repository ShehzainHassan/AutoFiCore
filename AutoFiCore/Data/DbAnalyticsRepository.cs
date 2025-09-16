using AutoFiCore.Data.Interfaces;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoFiCore.Data
{
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
        }
        public async Task<Auction?> GetAuctionWithAnalyticsAsync(int auctionId)
        {
            return await _dbContext.Auctions
                .Include(a => a.AuctionAnalytics)
                .FirstOrDefaultAsync(a => a.AuctionId == auctionId);
        }
        public async Task<bool> IsPaymentCompletedAsync(int auctionId)
        {
            return await _dbContext.AnalyticsEvents
                .AnyAsync(e => e.AuctionId == auctionId && e.EventType == AnalyticsEventType.PaymentCompleted);
        }
    }
}
