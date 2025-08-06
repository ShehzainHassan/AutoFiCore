using AutoFiCore.Data;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

public class DbAuctionRepository : IAuctionRepository, IBidRepository, IWatchlistRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbAuctionRepository> _logger;
    public DbAuctionRepository(ApplicationDbContext db, ILogger<DbAuctionRepository> log)
    {
        _dbContext = db;
        _logger = log;
    }
    public Task<int> GetAuctionCountAsync(DateTime start, DateTime end) =>
    _dbContext.Auctions.CountAsync(a => a.CreatedUtc >= start && a.CreatedUtc < end);
    public Task<int> GetBidCountAsync(DateTime start, DateTime end) =>
    _dbContext.Bids.CountAsync(b => b.CreatedUtc >= start && b.CreatedUtc < end);
    public async Task<Auction> AddAuctionAsync(Auction auction)
    {
        _dbContext.Auctions.Add(auction);
        await _dbContext.SaveChangesAsync();
        return auction;
    }
    public async Task<List<Auction>> GetEndedAuctions()
    {

        return await _dbContext.Auctions
            .Where(a => a.Status == AuctionStatus.Ended)
            .ToListAsync();
    }
    public Task<bool> VehicleHasAuction(int vehicleId)
    {
        return _dbContext.Auctions
            .AnyAsync(a => a.VehicleId == vehicleId && a.Status != AuctionStatus.Scheduled);
    }
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
    public async Task<Auction?> UpdateReserveStatusAsync(int auctionId)
    {
        var auction = await _dbContext.Auctions.FindAsync(auctionId);
        if (auction == null)
            return null;

        auction.IsReserveMet = true;
        auction.ReserveMetAt = DateTime.UtcNow;
        auction.UpdatedUtc = DateTime.UtcNow;

        _dbContext.Auctions.Update(auction);
        await _dbContext.SaveChangesAsync();

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
    public async Task<decimal> GetHighestBidAmountAsync(int auctionId, decimal startingPrice)
    {
        var highestBid = await _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .Select(b => (decimal?)b.Amount)
            .FirstOrDefaultAsync();

        return highestBid ?? startingPrice;
    }
    public async Task<int?> GetHighestBidderIdAsync(int auctionId)
    {
        var highestBid = await _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .Select(b => b.UserId)
            .FirstOrDefaultAsync();

        return highestBid == 0 ? null : highestBid;
    }
    public async Task UpdateCurrentPriceAsync(int auctionId, decimal newPrice)
    {
        var auction = await _dbContext.Auctions.FirstOrDefaultAsync(a => a.AuctionId == auctionId);
        if (auction != null)
        {
            auction.CurrentPrice = newPrice;
            auction.UpdatedUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task<Auction?> UpdateAuctionEndTimeAsync(int auctionId, int extensionMinutes)
    {
        var auction = await _dbContext.Auctions.FindAsync(auctionId);
        if (auction == null)
            return null;
        auction.EndUtc = auction.EndUtc.AddMinutes(extensionMinutes);
        auction.ExtensionCount++;

        _dbContext.Auctions.Update(auction);
        return auction;
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
    public async Task<List<Auction>> GetAuctionsWithActiveAutoBidsAsync()
    {
        return await _dbContext.Auctions
            .Where(a => a.Status == AuctionStatus.Active &&
                        _dbContext.AutoBids.Any(ab => ab.AuctionId == a.AuctionId && ab.IsActive))
            .ToListAsync();
    }
    public void UpdateAuction(Auction auction)
    {
        _dbContext.Auctions.Update(auction);
    }
    public Task<int> GetUniqueBiddersCountAsync(int auctionId)
    {
        return _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .Select(b => b.UserId)
            .Distinct()
            .CountAsync();
    }
    public Task<int> GetTotalBidsAsync(int auctionId)
    {
        return _dbContext.Bids.CountAsync(b => b.AuctionId == auctionId);
    }
    public Task<List<int>> GetUniqueBidderIdsAsync(int auctionId)
    {
        return _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .Select(b => b.UserId)
            .Distinct()
            .ToListAsync();
    }

}
