using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class DbAuctionRepository : IAuctionRepository, IBidRepository, IWatchlistRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbAuctionRepository> _logger;

    public DbAuctionRepository(ApplicationDbContext db, ILogger<DbAuctionRepository> log)
    {
        _dbContext = db;
        _logger = log;
    }

    public async Task<Auction> AddAuctionAsync(Auction auction)
    {
        _dbContext.Auctions.Add(auction);
        await _dbContext.SaveChangesAsync();
        return auction;
    }
    public Task<bool> VehicleHasAuction(int vehicleId) => _dbContext.Auctions.AnyAsync(a => a.VehicleId == vehicleId);

    public async Task<Auction?> UpdateAuctionStatusAsync(int auctionId, AuctionStatus status)
    {
        var auction = await _dbContext.Auctions.FindAsync(auctionId);
        if (auction == null)
            return null;

        auction.Status = status;
        auction.UpdatedUtc = DateTime.UtcNow;

        _dbContext.Auctions.Update(auction);
        return auction;
    }
    public IQueryable<Auction> Query()
    {
        return _dbContext.Auctions.AsQueryable().Include(a => a.Vehicle);
    }
    public async Task<Auction?> GetAuctionByIdAsync(int id)
    {
        return await _dbContext.Auctions
            .Include(a => a.Vehicle)
            .Include(a => a.Bids.OrderByDescending(b => b.CreatedUtc))
                .ThenInclude(b => b.User)              
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AuctionId == id);
    }
    public async Task<Bid> AddBidAsync(Bid bid)
    {
        _dbContext.Bids.Add(bid);
        await _dbContext.SaveChangesAsync();
        return bid;
    }
    public async Task<List<Bid>> GetBidsByAuctionIdAsync(int auctionId)
    {
        return await _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.CreatedUtc)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<List<Bid>> GetBidsByUserIdAsync(int userId)
    {
        return await _dbContext.Bids
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedUtc)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Bid?> GetHighestBidAsync(int auctionId)
    {
        return await _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateCurrentPriceAsync(int auctionId)
    {
        var highestBid = await _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .Select(b => b.Amount)
            .FirstOrDefaultAsync();       

        await _dbContext.Auctions
            .Where(a => a.AuctionId == auctionId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(a => a.CurrentPrice, a => highestBid)
                .SetProperty(a => a.UpdatedUtc, a => DateTime.UtcNow));
    }

    public async Task AddToWatchlistAsync(int userId, int auctionId)
    {
        if (!await IsWatchingAsync(userId, auctionId))
        {
            var watch = new Watchlist { UserId = userId, AuctionId = auctionId };
            _dbContext.Watchlists.Add(watch);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveFromWatchlistAsync(int userId, int auctionId)
    {
        var entry = await _dbContext.Watchlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.AuctionId == auctionId);

        if (entry != null)
        {
            _dbContext.Watchlists.Remove(entry);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<Watchlist>> GetUserWatchlistAsync(int userId)
    {
        return await _dbContext.Watchlists
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Watchlist>> GetAuctionWatchersAsync(int auctionId)
    {
        return await _dbContext.Watchlists
            .Where(w => w.AuctionId == auctionId)
            .ToListAsync();
    }

    public Task<bool> IsWatchingAsync(int userId, int auctionId)
    {
        return _dbContext.Watchlists.AnyAsync(w => w.UserId == userId && w.AuctionId == auctionId);
    }

}
