using AutoFiCore.Enums;
using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAuctionLifecycleService
    {
        Task HandleNewBid(int auctionId);
        Task HandleOutbid(Auction auction, int? previousBidderId);
        Task HandleAuctionWonAsync(Auction auction, int userId);
        Task HandleAuctionLostAsync(Auction auction, int userId);
        Task HandleAuctionEndAsync(Auction auction);
        Task HandleReserveMet(Auction auction, int? newBidUserId = null);
        Task HandleAuctionExtended(Auction auction);
        Task HandleBidderCountUpdate(Auction auction, List<int> previousBidders, List<int> updatedBidders);
        Task HandleAuctionStatusChangedAsync(Auction auction, AuctionStatus previousStatus);
        Task HandleAutoBidAsync(int auctionId, int userId, decimal amount);
    }
}
