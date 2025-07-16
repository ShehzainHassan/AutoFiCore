namespace AutoFiCore.Data
{
    public interface IAutoBidRepository
    {
        Task<AutoBid> AddAutoBidAsync(AutoBid autoBid);
        Task<bool> IsActiveAsync(int userId, int auctionId);

    }
}
