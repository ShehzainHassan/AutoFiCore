using AutoFiCore.Models;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AutoFiCore.Hubs
{
    public class AuctionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         Context.User?.FindFirst("sub")?.Value;

            Console.WriteLine($"SignalR connected: ConnectionId = {Context.ConnectionId}, UserId = {userId}");

            if (httpContext != null)
            {
                var auctionId = httpContext.Request.Query["auctionId"];
                if (int.TryParse(auctionId, out int id))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{id}");
                    Console.WriteLine($"Added to group: auction-{id}");
                }
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinAuctionGroup(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
            Console.WriteLine($"Joined group auction-{auctionId} via method");
        }
    }
}
