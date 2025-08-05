using AutoFiCore.Hubs;
using AutoFiCore.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace AutoFiCore.Services
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
    public class AuctionNotifier : IAuctionNotifier
    {
        private readonly IHubContext<AuctionHub> _hubContext;
        public AuctionNotifier(IHubContext<AuctionHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task NotifyNewBid(int auctionId)
        {
            await _hubContext.Clients.Group($"auction-{auctionId}")
                .SendAsync("ReceiveNewBid", new { auctionId });
        }
        public async Task NotifyOutbid(int userId, int auctionId)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("Outbid", new { auctionId });
        }
        public async Task NotifyAuctionEnd(Auction auction)
        {
            await _hubContext.Clients.Group($"auction-{auction.AuctionId}")
                .SendAsync("AuctionEnded", new
                {
                    auctionId = auction.AuctionId,
                    finalPrice = auction.CurrentPrice,
                    isReserveMet = auction.IsReserveMet,
                });
        }
        public async Task NotifyReserveMet(int auctionId)
        {
            await _hubContext.Clients.Group($"auction-{auctionId}")
                .SendAsync("ReservePriceMet", new { auctionId });
        }
        public async Task NotifyAuctionExtended(int auctionId, DateTime newEndTime)
        {
            await _hubContext.Clients.Group($"auction-{auctionId}")
                .SendAsync("AuctionExtended", new
                {
                    auctionId,
                    newEndTime
                });
        }
        public async Task NotifyAuctionWon(int userId, int auctionId)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("AuctionWon", new
                {
                    auctionId
                });
        }
        public async Task NotifyAuctionLost(int userId, int auctionId)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("AuctionLost", new
                {
                    auctionId
                });
        }
        public async Task NotifyBidderCount(int auctionId, int activeBidders)
        {
            await _hubContext.Clients.Group($"auction-{auctionId}")
                .SendAsync("BidderCountUpdated", new
                {
                    auctionId,
                    activeBidders
                });
        }
        public async Task NotifyAuctionStatusChanged(int userId, int auctionId, string status)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("AuctionStatusChanged", new
                {
                    auctionId,
                    newStatus = status
                });
        }
        public async Task NotifyAutoBidExecuted(int userId, int auctionId, decimal amount)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("AutoBidExecuted", new
                {
                    auctionId,
                    amount
                });
        }
    }
}
