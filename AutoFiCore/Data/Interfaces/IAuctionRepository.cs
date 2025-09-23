using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Enums;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAuctionRepository
    {
        Task<Auction> AddAuctionAsync(Auction auction);
        Task<bool> VehicleHasAuction(int vehicleId);
        void UpdateAuction(Auction auction);
        Task<Auction?> UpdateAuctionStatusAsync(int auctionId, AuctionStatus status);
        IQueryable<Auction> Query();
        Task<Auction?> GetAuctionByIdAsync(int id);
        Task UpdateCurrentPriceAsync(int auctionId, decimal newPrice);
        Task<List<Auction>> GetAuctionsWithActiveAutoBidsAsync();
        Task<Auction?> UpdateReserveStatusAsync(int auctionId);
        Task<Auction?> UpdateAuctionEndTimeAsync(int auctionId, int extensionMinutes);
        Task<List<Auction>> GetEndedAuctions();
        Task<DateTime?> GetOldestAuctionDateAsync();
        Task AddAuctionWinnerAsync(int userId, int auctionId, decimal winningBid, int vehicleId, string username);
    }
}