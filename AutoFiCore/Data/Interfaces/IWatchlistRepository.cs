using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface IWatchlistRepository
    {
        Task AddToWatchlistAsync(int userId, int auctionId, int VehicleId);
        Task RemoveFromWatchlistAsync(int userId, int auctionId);
        Task<List<WatchlistDTO>> GetUserWatchlistAsync(int userId);
        Task<List<Watchlist>> GetAuctionWatchersAsync(int auctionId);
        Task<bool> IsWatchingAsync(int userId, int auctionId);

    }
}
