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
                .SendAsync("ReservePriceMet", new
                {
                    auctionId
                });
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

    }
}
