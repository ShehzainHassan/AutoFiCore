using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Generic;

namespace AutoFiCore.Hubs
{
    public class AuctionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var auctionId = httpContext.Request.Query["auctionId"];
                if (int.TryParse(auctionId, out int id))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{id}");
                }
            }
            await base.OnConnectedAsync();
        }
        public async Task JoinAuctionGroup(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        }
    }
}
