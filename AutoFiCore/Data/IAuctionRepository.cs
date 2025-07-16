using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Data
{
    public interface IAuctionRepository
    {
        Task<Auction> AddAuctionAsync(Auction auction);
        Task<bool> VehicleHasAuction(int vehicleId);
        Task<Auction?> UpdateAuctionStatusAsync(int auctionId, AuctionStatus status);
        IQueryable<Auction> Query();
        Task<Auction?> GetAuctionByIdAsync(int id);
        Task UpdateCurrentPriceAsync(int auctionId);
    }
}
