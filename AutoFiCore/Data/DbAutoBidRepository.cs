using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
