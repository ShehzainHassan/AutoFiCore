using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAuctionNotifier
    {
        Task NotifyNewBid(int auctionId);
        Task NotifyAuctionEnd(Auction auction);
        Task NotifyOutbid(int userId, int auctionId);
        Task NotifyReserveMet(int auctionId);
        Task NotifyAuctionExtended(int auctionId, DateTime newEndTime);
        Task NotifyAuctionWon(int userId, int auctionId);
        Task NotifyAuctionLost(int userId, int auctionId);
        Task NotifyBidderCount(int auctionId, int activeBidders);
        Task NotifyAuctionStatusChanged(int userId, int auctionId, string status);
        Task NotifyAutoBidExecuted(int userId, int auctionId, decimal amount);
    }
}
