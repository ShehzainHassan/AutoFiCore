using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using AutoFiCore.Hubs; 
namespace AutoFiCore.Services
{
    public interface IAuctionNotifier
    {
        Task NotifyNewBid(int auctionId);
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
                .SendAsync("ReceiveNewBid", auctionId);
        }
    }
}
