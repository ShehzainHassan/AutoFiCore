using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Generic;

namespace AutoFiCore.Hubs
{
    public class AuctionHub : Hub
    {
        private static readonly Dictionary<int, List<string>> _userConnections = new();
        public Task JoinGroup(string groupName) =>
            Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        public Task LeaveGroup(string groupName) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        public async Task NotifyOutbid(int auctionId, int bidderUserId)
        {
            var groupName = $"auction-{auctionId}";

            await Clients.Group(groupName)
                         .SendAsync("ReceiveOutbidNotification",
                                    "You got outbid.",
                                    bidderUserId);
        }
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!_userConnections.ContainsKey(userId))
                _userConnections[userId] = new List<string>();

            _userConnections[userId].Add(Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = GetUserId();
            if (_userConnections.TryGetValue(userId, out var list))
            {
                list.Remove(Context.ConnectionId);
                if (list.Count == 0) _userConnections.Remove(userId);
            }
            await base.OnDisconnectedAsync(ex);
        }
        private int GetUserId()
        {
            var claim = Context.User?.FindFirst("id")
                      ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out var id) ? id : -1;
        }
    }
}
