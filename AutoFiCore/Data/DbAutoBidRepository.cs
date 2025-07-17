using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AutoFiCore.Data
{
    public class DbAutoBidRepository:IAutoBidRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DbAutoBidRepository> _logger;

        public DbAutoBidRepository(ApplicationDbContext db, ILogger<DbAutoBidRepository> log)
        {
            _dbContext = db;
            _logger = log;
        }
        public async Task<AutoBid> AddAutoBidAsync(AutoBid autoBid)
        {
            _dbContext.AutoBids.Add(autoBid);
            await _dbContext.SaveChangesAsync();
            return autoBid;
        }
        public async Task<bool> IsActiveAsync(int userId, int auctionId)
        {
            return await _dbContext.AutoBids
                .AnyAsync(ab => ab.UserId == userId && ab.AuctionId == auctionId && ab.IsActive);
        }
        public async Task<AutoBid?> GetByIdAsync(int id)
        {
            return await _dbContext.AutoBids.FindAsync(id);
        }
        public async Task SetInactiveAsync(int autoBidId)
        {
            var autoBid = await _dbContext.AutoBids.FindAsync(autoBidId);
            if (autoBid != null)
            {
                autoBid.IsActive = false;
                autoBid.UpdatedAt = DateTime.UtcNow;
            }
        }
        public async Task<List<AutoBid>> GetActiveAutoBidsByUserAsync(int userId)
        {
            return await _dbContext.AutoBids
                .Include(ab => ab.Auction)
                .Where(ab => ab.UserId == userId && ab.IsActive)
                .ToListAsync();
        }
        public async Task<List<AutoBid>> GetActiveAutoBidsByAuctionIdAsync(int auctionId)
        {
            return await _dbContext.AutoBids
                .Where(ab => ab.AuctionId == auctionId && ab.IsActive)
                .ToListAsync();
        }

    }
}
