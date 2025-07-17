namespace AutoFiCore.Data
{
    public interface IAutoBidRepository
    {
        Task<AutoBid> AddAutoBidAsync(AutoBid autoBid);
        Task<bool> IsActiveAsync(int userId, int auctionId);
        Task<AutoBid?> GetByIdAsync(int id);
        Task SetInactiveAsync(int autoBidId);
        Task<List<AutoBid>> GetActiveAutoBidsByUserAsync(int userId);
        Task<List<AutoBid>> GetActiveAutoBidsByAuctionIdAsync(int auctionId);
    }
}
