using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IVehicleRepository Vehicles { get; }
        IUserRepository Users { get; }
        IContactInfoRepository ContactInfo { get; }
        IAuctionRepository Auctions { get; }
        IBidRepository Bids { get; }
        IWatchlistRepository Watchlist { get; }
        INewsLetterRepository NewsLetter { get; }
        IAutoBidRepository AutoBid { get; }
        INotificationRepository Notification { get; }
        IAnalyticsRepository Analytics { get; }
        IReportRepository Report { get; }
        IMetricsRepository Metrics { get; }
        IPerformanceRepository Performance { get; }
        IChatRepository ChatRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        ApplicationDbContext DbContext { get; }
    }
}
