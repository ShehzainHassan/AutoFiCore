using AutoFiCore.Models;

namespace AutoFiCore.Data
{
    public interface IBidRepository
    {
        Task<Bid> AddBidAsync(Bid bid);
        Task<List<Bid>> GetBidsByAuctionIdAsync(int auctionId);
        Task<Bid?> GetHighestBidAsync(int auctionId);
    }
}
