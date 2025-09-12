using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AutoFiCore.Data
{
    public class DbAutoBidRepository : IAutoBidRepository
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
        public async Task<BidStrategy> AddBidStrategyAsync(BidStrategy bidStrategy)
        {
            _dbContext.BidStrategies.Add(bidStrategy);
            await _dbContext.SaveChangesAsync();
            return bidStrategy;
        }
        public async Task<BidStrategy?> GetBidStrategyByUserAndAuctionAsync(int userId, int auctionId)
        {
            return await _dbContext.BidStrategies
                .Include(bs => bs.User)
                .Include(bs => bs.Auction)
                .FirstOrDefaultAsync(bs => bs.UserId == userId && bs.AuctionId == auctionId);
        }
        public async Task<bool> IsActiveAsync(int userId, int auctionId)
        {
            return await _dbContext.AutoBids
                .AnyAsync(ab => ab.UserId == userId && ab.AuctionId == auctionId && ab.IsActive);
        }
        public async Task<AutoBid?> GetByIdAsync(int userId, int auctionId)
        {
            return await _dbContext.AutoBids
                .FirstOrDefaultAsync(ab => ab.UserId == userId && ab.AuctionId == auctionId);
        }
        public async Task SetInactiveAsync(int userId, int auctionId)
        {
            var autoBid = await GetByIdAsync(userId, auctionId);
            if (autoBid != null)
            {
                autoBid.IsActive = false;
                autoBid.UpdatedAt = DateTime.UtcNow;
            }
        }
        public async Task<CreateAutoBidDTO?> GetAutoBidWithStrategyAsync(int userId, int auctionId)
        {
            return await _dbContext.AutoBids
                .Where(ab => ab.UserId == userId && ab.AuctionId == auctionId)
                .Join(
                    _dbContext.BidStrategies,
                    ab => new { ab.UserId, ab.AuctionId },
                    bs => new { bs.UserId, bs.AuctionId },
                    (ab, bs) => new CreateAutoBidDTO
                    {
                        AuctionId = ab.AuctionId,
                        MaxBidAmount = ab.MaxBidAmount,
                        UserId = ab.UserId,
                        IsActive = ab.IsActive,
                        BidStrategyType = ab.BidStrategyType,
                        BidDelaySeconds = bs.BidDelaySeconds,
                        MaxBidsPerMinute = bs.MaxBidsPerMinute,
                        PreferredBidTiming = bs.PreferredBidTiming,
                        MaxSpreadBids = bs.MaxSpreadBids,
                    }
                )
                .FirstOrDefaultAsync();
        }
        public async Task<List<AutoBid>> GetActiveAutoBidsByAuctionIdAsync(int auctionId)
        {
            return await _dbContext.AutoBids
                .Where(ab => ab.AuctionId == auctionId && ab.IsActive)
                .ToListAsync();
        }
        public async Task<List<UserAutoBidSettings>> GetUserAutoBidSettingsAsync(int userId)
        {
            var autoBids = await _dbContext.AutoBids
                .Where(ab => ab.UserId == userId)
                .ToListAsync();

            var strategies = await _dbContext.BidStrategies
                .Where(bs => bs.UserId == userId)
                .ToListAsync();

            var result = (from ab in autoBids
                          join bs in strategies
                              on new { ab.UserId, ab.AuctionId }
                              equals new { bs.UserId, bs.AuctionId }
                              into gj
                          from bs in gj.DefaultIfEmpty()
                          select new UserAutoBidSettings
                          {
                              Id = ab.Id,
                              UserId = ab.UserId,
                              AuctionId = ab.AuctionId,
                              MaxBidAmount = ab.MaxBidAmount,
                              BidStrategyType = ab.BidStrategyType,
                              CreatedAt = ab.CreatedAt,
                              UpdatedAt = ab.UpdatedAt,
                              BidDelaySeconds = bs?.BidDelaySeconds,
                              MaxBidsPerMinute = bs?.MaxBidsPerMinute,
                              MaxSpreadBids = bs?.MaxSpreadBids,
                              PreferredBidTiming = bs?.PreferredBidTiming ?? PreferredBidTiming.Immediate
                          }).ToList();

            return result;
        }

    }
}
