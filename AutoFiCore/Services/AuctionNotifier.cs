using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using AutoFiCore.Hubs; 
namespace AutoFiCore.Services
{
    public interface IAuctionNotifier
    {
        Task NotifyNewBid(int auctionId);
        Task AuctionEnded(int auctionId);
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
        public async Task AuctionEnded(int auctionId)
        {
            await _hubContext.Clients.Group($"auction-{auctionId}")
                .SendAsync("AuctionEnded", auctionId);
        }
    }
}
