using AutoFiCore.Models;

namespace AutoFiCore.Data
{
    public interface IBidRepository
    {
        Task<Bid> AddBidAsync(Bid bid);
        Task<List<Bid>> GetBidsByAuctionIdAsync(int auctionId);
        Task<List<Bid>> GetBidsByUserIdAsync(int userId);
        Task<Bid?> GetHighestBidAsync(int auctionId);
    }
}
