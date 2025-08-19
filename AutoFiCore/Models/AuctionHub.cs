using AutoFiCore.Models;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AutoFiCore.Hubs
{
    /// <summary>
    /// SignalR hub for managing real-time auction connections and group memberships.
    /// </summary>
    public class AuctionHub : Hub
    {
        /// <summary>
        /// Called when a client connects to the hub. Automatically adds the connection to an auction group if auctionId is provided.
        /// </summary>
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

        /// <summary>
        /// Allows manually join a specific auction group.
        /// </summary>
        /// <param name="auctionId">The ID of the auction group to join.</param>
        public async Task JoinAuctionGroup(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
            Console.WriteLine($"Joined group auction-{auctionId} via method");
        }
    }
}
