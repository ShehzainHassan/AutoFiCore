namespace AutoFiCore.Data
{
    public interface IUnitOfWork:IDisposable
    {
        IVehicleRepository Vehicles { get; }
        IUserRepository Users { get; }
        IContactInfoRepository ContactInfo { get; }
        INewsLetterRepository NewsLetter { get; }
        IAuctionRepository Auctions { get; }    
        IBidRepository Bids { get; }
        IWatchlistRepository Watchlist { get; }
        IAutoBidRepository AutoBid { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
