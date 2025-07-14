using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Data
{
    public interface IAuctionRepository
    {
        Task<Auction> AddAuctionAsync(Auction auction);
        Task<bool> VehicleHasAuction(int vehicleId);
        Task<Auction?> UpdateAuctionStatusAsync(int auctionId, string status);

    }
}
