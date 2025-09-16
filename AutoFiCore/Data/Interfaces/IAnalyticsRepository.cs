using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task AddEventAsync(AnalyticsEvent analyticsEvent);
        Task<Auction?> GetAuctionWithAnalyticsAsync(int auctionId);
        Task<bool> IsPaymentCompletedAsync(int auctionId);
    }
}
