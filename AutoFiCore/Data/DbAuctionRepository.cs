using AutoFiCore.Data;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class DbAuctionRepository : IAuctionRepository
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

    public Task<bool> VehicleHasAuction(int vehicleId) =>
        _dbContext.Auctions.AnyAsync(a => a.VehicleId == vehicleId);

    public async Task<Auction?> UpdateAuctionStatusAsync(int auctionId, string status)
    {
        var auction = await _dbContext.Auctions.FindAsync(auctionId);
        if (auction == null)
            return null;

        auction.Status = status;
        auction.UpdatedUtc = DateTime.UtcNow;

        _dbContext.Auctions.Update(auction);
        return auction;
    }

}
