using AutoFiCore.Models;

namespace AutoFiCore.Data
{
    public interface IBidRepository
    {
        Task<int> GetBidCountAsync(DateTime start, DateTime end);
        Task<Bid> AddBidAsync(Bid bid);
        Task<List<Bid>> GetBidsByAuctionIdAsync(int auctionId);
        Task<List<Bid>> GetBidsByUserIdAsync(int userId);
        Task<decimal> GetHighestBidAmountAsync(int auctionId, decimal startingPrice);
        Task<int?> GetHighestBidderIdAsync(int auctionId);
        Task<int> GetUniqueBiddersCountAsync(int auctionId);
        Task<int> GetTotalBidsAsync(int auctionId);
        Task<List<int>> GetUniqueBidderIdsAsync(int auctionId);

    }
}
