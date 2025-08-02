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
        INotificationRepository Notification { get; }
        IAnalyticsRepository Analytics { get; }
        IReportRepository Report { get; }   
        IMetricsRepository Metrics { get; }
        IPerformanceRepository Performance { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
